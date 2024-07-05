using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public class HammerWindmillSkill : GASAbility, ISkill
    {
        [Header(" [ Windmill Hammer Skill ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private ESkillType _skillType;

        [Header(" Animation ")]
        [SerializeField] private string _startAnimationName = "HammerWindmill_Start";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;
        [SerializeField] private string _loopAnimationName = "HammerWindmill_Loop";
        [SerializeField] private string _endAnimationName = "HammerWindmill_End";

        [Header(" Attack Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private Variable_LayerMask _traceLayer;
        [SerializeField] private float _traceRadius = 1f;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _onHitToOtherCue;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit;
        [SerializeField] private FGameplayCue _onHitGroundFX;

        public Sprite Icon => _icon;
        public ESkillType SkillType => _skillType;
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }
        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly HammerWindmillSkill _ability;

            private readonly int _animationHash;
            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private readonly TrailSphereCast _sphereCast = new();


            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as HammerWindmillSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _bodySystem = gameObject.GetBodySystem();
                _pawnSystem = gameObject.GetPawnSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _animationHash = Animator.StringToHash(_ability._startAnimationName);
            }

            public override bool CanActiveAbility()
            {
                if (_ability._coolTimeEffect && _gameplayEffectSystem.HasEffect(_ability._coolTimeEffect))
                    return false;

                return base.CanActiveAbility();
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                _animationPlayer.Play(_animationHash, _ability._fadeInTime);

                _animationPlayer.OnNotify += _animationPlayer_OnNotify;
            }

            private void PlayHitGroundFX()
            {
                if (_ability._onHitGroundFX.Cue)
                {
                    _ability._onHitGroundFX.PlayFromTarget(transform);
                }
            }

            private void _animationPlayer_OnNotify(string eventName)
            {
                switch (eventName)
                {
                    case "HitGround":
                        PlayHitGroundFX();
                        break;
                    case "OnTrace":
                        OnTrace();
                        break;
                    case "EndTrace":
                        EndTrace();
                        break;
                    default:
                        break;
                }
            }

            public void UpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    switch (_animationPlayer.State)
                    {
                        case EAnimationState.Failed | EAnimationState.Canceled:
                            CancelAbility();
                            break;
                        case EAnimationState.Playing:
                            break;
                        case EAnimationState.BlendOut:
                            TryFinishAbility();
                            break;
                        case EAnimationState.Finish:
                            break;
                        default:
                            break;
                    }
                }
                else
                {

                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    UpdateTrace();
                }
            }
            private void OnTrace()
            {
                var bodypart = _bodySystem.GetBodyPart(_ability._tracePoint);

                _sphereCast.SetOwner(bodypart.gameObject);

                _sphereCast.AddIgnoreTransform(transform);

                _sphereCast.TraceRadius = _ability._traceRadius;
                _sphereCast.TraceLayer = _ability._traceLayer.Value;
                _sphereCast.MaxHitCount = 20;
                _sphereCast.UseDebug = UseDebug;

                _sphereCast.OnTrace();
            }
            private void EndTrace()
            {
                _sphereCast.EndTrace();
            }

            private bool CheckAffilation(Transform target)
            {
                if (!target)
                    return false;

                if (!target.TryGetController(out IControllerSystem hitPawnController))
                    return false;

                if (!_pawnSystem.IsPossessed)
                    return false;

                if (_pawnSystem.Controller.CheckAffiliation(hitPawnController) != EAffiliation.Hostile)
                    return false;

                return true;
            }

            private void UpdateTrace()
            {
                if (!_sphereCast.IsPlaying)
                    return;

                var trace = _sphereCast.UpdateTrace();

                if (trace.hitCount > 0)
                {
                    bool wasHit = false;

                    for (int i = 0; i < trace.hitCount; i++)
                    {
                        bool isHit = false;
                        var hit = trace.raycastHits[i];


                        if (!CheckAffilation(hit.transform))
                            continue;

                        Log($"HIT :: {hit.transform.name}");

                        if (hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        {
                            if (_ability._takeDamageEffect)
                            {
                                var data = new TakeDamageEffect.FElement(hit.point, hit.normal, hit.collider, _sphereCast.StartPosition.Direction(_sphereCast.EndPosition), _sphereCast.Owner, gameObject);

                                if (hitGameplayEffectSystem.TryTakeEffect(_ability._takeDamageEffect, gameObject, Level, data).isActivate)
                                {
                                    isHit = true;
                                }
                            }

                            for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnHitToOther.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnHitToOther[effectIndex];

                                if (hitGameplayEffectSystem.TryTakeEffect(effect, gameObject, Level, null).isActivate)
                                {
                                    isHit = true;
                                }
                            }


                        }

                        if (isHit)
                        {
                            wasHit = true;

                            if (_ability._onHitToOtherCue.Cue)
                            {
                                Vector3 position = hit.distance > 0 ? hit.point + hit.transform.TransformDirection(_ability._onHitToOtherCue.Position)
                                                                    : hit.collider.ClosestPoint(_sphereCast.StartPosition);
                                Vector3 rotation = Quaternion.LookRotation(hit.normal, Vector3.up).eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
                                Vector3 scale = _ability._onHitToOtherCue.Scale;

                                _ability._onHitToOtherCue.Cue.Play(position, rotation, scale);
                            }
                        }
                    }

                    if (wasHit)
                    {
                        if (_gameplayEffectSystem is not null)
                        {
                            for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnSuccessedHit.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnSuccessedHit[effectIndex];

                                _gameplayEffectSystem.TryTakeEffect(effect, gameObject, Level, null);
                            }
                        }

                        if (_pawnSystem.IsPlayer)
                        {
                            if (_ability._onSuccessedPlayerHit.Cue)
                            {
                                _ability._onSuccessedPlayerHit.PlayFromTarget(transform);
                            }
                        }
                    }
                }
            }
        }
    }
}
