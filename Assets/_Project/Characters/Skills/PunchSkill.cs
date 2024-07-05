using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Punch Skill", fileName = "GA_Skill_Punch")]
    public class PunchSkill : GASAbility, ISkill
    {
        [Header(" [ Punch Skill ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private ESkillType _skillType;

        [Header(" Animation ")]
        [SerializeField] private string _animationName = "Attack01";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;

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

        [Header(" Combo ")]
        [SerializeField] private GameplayTag _comboTag;

        public Sprite Icon => _icon;
        public ESkillType SkillType => _skillType;
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }
        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly PunchSkill _ability;

            private readonly int _animationHash;
            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private bool _wasEnabledTrace = false;
            private readonly List<Transform> _ignoreTransforms = new();
            private Vector3 _prevTracePoint;
            private RaycastHit[] _hitResults = new RaycastHit[10];


            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as PunchSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _bodySystem = gameObject.GetBodySystem();
                _pawnSystem = gameObject.GetPawnSystem();
                _rotationSystem = gameObject.GetRotationSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _animationHash = Animator.StringToHash(_ability._animationName);
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

                _wasEnabledTrace = false;

                _animationPlayer.Play(_animationHash, _ability._fadeInTime);

                _animationPlayer.OnEnterNotifyState += _animationPlayer_OnEnterNotifyState;
                _animationPlayer.OnExitNotifyState += _animationPlayer_OnExitNotifyState;
            }

            private void _animationPlayer_OnEnterNotifyState(string obj)
            {
                switch (obj)
                {
                    case "Trace":
                        OnTrace();
                        break;
                    case "Combo":
                        OnCombo();
                        break;
                    default:
                        break;
                }
            }

            private void _animationPlayer_OnExitNotifyState(string obj)
            {
                switch (obj)
                {
                    case "Trace":
                        EndTrace();
                        break;
                    case "Combo":
                        EndCombo();
                        break;
                    default:
                        break;
                }
            }

            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
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
                if(IsPlaying)
                {
                    UpdateTrace();
                }
            }

            private void OnCombo()
            {
                GameplayTagSystem.AddOwnedTag(_ability._comboTag);
            }
            private void EndCombo()
            {
                GameplayTagSystem.RemoveOwnedTag(_ability._comboTag);
            }

            private void OnTrace()
            {
                if (_wasEnabledTrace)
                    return;

                _wasEnabledTrace = true;
                
                _ignoreTransforms.Clear();
                _ignoreTransforms.Add(transform);

                var tracePart = _bodySystem.GetBodyPart(_ability._tracePoint);
                _prevTracePoint = tracePart.transform.position;
            }
            private void EndTrace()
            {
                if (!_wasEnabledTrace)
                    return;

                _wasEnabledTrace = false;
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
                if (!_wasEnabledTrace)
                    return;

                var bodyPart = _bodySystem.GetBodyPart(_ability._tracePoint);

                Vector3 prevPosition = _prevTracePoint;
                Vector3 currentPosition = bodyPart.transform.position;

                _prevTracePoint = currentPosition;

                var hitCount = SUtility.Physics.DrawSphereCastAllNonAlloc(prevPosition, currentPosition, _ability._traceRadius, _hitResults, _ability._traceLayer.Value, QueryTriggerInteraction.Ignore, _ability.UseDebug);

                if (hitCount > 0)
                {
                    bool wasHit = false;

                    for (int i = 0; i < hitCount; i++)
                    {
                        bool isHit = false;
                        var hit = _hitResults[i];

                        if (!_ignoreTransforms.Contains(hit.transform))
                        {
                            _ignoreTransforms.Add(hit.transform);
                            Log($"HIT :: {hit.transform.name}");

                            if (!CheckAffilation(hit.transform))
                                continue;

                            if(hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                            {
                                if(_ability._takeDamageEffect)
                                {
                                    var data = new TakeDamageEffect.FElement(hit.point, hit.normal, hit.collider, prevPosition.Direction(currentPosition), bodyPart.gameObject, gameObject);
                                    
                                    if(hitGameplayEffectSystem.TryTakeEffect(_ability._takeDamageEffect, gameObject, Level, data).isActivate)
                                    {
                                        isHit = true;
                                    }
                                }

                                for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnHitToOther.Length; effectIndex++)
                                {
                                    var effect = _ability._applyGameplayEffectsOnHitToOther[effectIndex];

                                    if(hitGameplayEffectSystem.TryTakeEffect(effect, gameObject, Level, null).isActivate)
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
                                                                        : hit.collider.ClosestPoint(prevPosition);
                                    Vector3 rotation = Quaternion.LookRotation(hit.normal, Vector3.up).eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
                                    Vector3 scale = _ability._onHitToOtherCue.Scale;

                                    _ability._onHitToOtherCue.Cue.Play(position, rotation, scale);
                                }
                            }
                        }
                    }

                    if(wasHit)
                    {
                        if(_gameplayEffectSystem is not null)
                        {
                            for(int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnSuccessedHit.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnSuccessedHit[effectIndex];

                                _gameplayEffectSystem.TryTakeEffect(effect, gameObject, Level, null);
                            }
                        }

                        if(_pawnSystem.IsPlayer)
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
