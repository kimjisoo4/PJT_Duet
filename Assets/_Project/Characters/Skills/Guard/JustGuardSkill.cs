using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Just Guard Skill", fileName = "GA_Skill_JustGuard")]
    public class JustGuardSkill : CharacterSkill
    {
        [Header(" [ Just Guard Skill ] ")]
        [SerializeField] private string _animationName = "JustGuard";
        [SerializeField][Range(0f, 1f)] private float _fadeTime = 0.2f;
        [SerializeField][Range(0f, 1f)] private float _offsetTime = 0.2f;
        [SerializeField] private float _slowWorldSpeed = 0.2f;
        [SerializeField] private float _slowDuration = 1f;

        [Header(" Gameplay Effect ")]
        [SerializeField] private TakeDamageEffect _takeDamage;
        [SerializeField] private TakeKnockbackEffect _takeKnockback;
        [SerializeField] private TakeStiffenEffect _takeStiffen;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Attack Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private SOLayerMaskVariable _traceLayer;
        [SerializeField] private float _traceRadius = 1f;

        [Header(" Gameplay Cue ")]
        [Header(" Attack Cue")]
        [SerializeField][Range(0f, 1f)] private float _attackCueTime = 0.2f;
        [SerializeField] private FGameplayCue _onAttackCue = FGameplayCue.Default;
        [Header(" Hit Cue ")]
        [SerializeField] private FGameplayCue _onHitToOtherCue = FGameplayCue.Default;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit = FGameplayCue.Default;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IReceiveDamageSkill, IUpdateableAbilitySpec
        {
            protected new readonly JustGuardSkill _ability;

            private readonly ICharacter _character;
            private readonly AnimationPlayer _animationPlayer;
            private readonly AnimationPlayer.Events _animationEvents;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IPawnSystem _pawnSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IBodySystem _bodySystem;
            
            private readonly int ANIM_JUST_GUARD;

            private DamageInfoData _damageInfoData;
            private bool _wasStartedAnimation;
            private Cue _onAttackCue;
            private bool _wasPlayedAttackCue;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as JustGuardSkill;

                _character = gameObject.GetComponent<ICharacter>();
                _animationPlayer = _character.Model.GetComponent<AnimationPlayer>();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _pawnSystem = gameObject.GetPawnSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _bodySystem = gameObject.GetBodySystem();

                _animationEvents = new();
                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnNotify += _animationEvents_OnNotify;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;

                ANIM_JUST_GUARD = Animator.StringToHash(_ability._animationName);
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                _wasStartedAnimation = false;
                _animationPlayer.Play(ANIM_JUST_GUARD, _ability._fadeTime, _ability._offsetTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                _testTime = _ability._slowDuration;
                Time.timeScale = _ability._slowWorldSpeed;

                OnTrace();
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _animationPlayer.TryStopAnimation(ANIM_JUST_GUARD);
                
            }

            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

                if(_onAttackCue is not null)
                {
                    _onAttackCue.Stop();
                    _onAttackCue = null;
                }
            }


            private float _testTime;
            public void UpdateAbility(float deltaTime)
            {
                if (_testTime > 0f)
                {
                    _testTime -= Time.unscaledDeltaTime;

                    if (_testTime <= 0f)
                    {
                        Time.timeScale = 1f;
                    }
                }

                if (!IsPlaying)
                    return;

                
                

                if (!_wasPlayedAttackCue && _animationPlayer.NormalizedTime >= _ability._attackCueTime)
                {
                    _wasPlayedAttackCue = true;

                    OnPlayAttackCue();
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            private void OnPlayAttackCue()
            {
                if (!IsPlaying)
                    return;

                if (!_ability._onAttackCue.Cue)
                    return;

                _onAttackCue = _ability._onAttackCue.PlayAttached(transform);
                _onAttackCue.OnEndedCue += _onAttackCue_OnEndedCue;
            }



            public bool TryActiveReceiveDamageSkill(DamageInfoData damageInfoData)
            {
                if (!CanActiveAbility())
                    return false;

                _damageInfoData = damageInfoData;

                ForceActiveAbility();

                return true;
            }

            private readonly RaycastHit[] _hitColliders = new RaycastHit[20];

            private bool CheckAffilation(Transform target)
            {
                if (!target)
                    return false;

                if (!target.TryGetReleationshipSystem(out IRelationshipSystem targetRelationship))
                    return false;

                if (_relationshipSystem.CheckRelationship(targetRelationship) != ERelationship.Hostile)
                    return false;

                return true;
            }

            private void OnTrace()
            {
                var bodyPart = _bodySystem.GetBodyPart(_ability._tracePoint);

                Transform point = bodyPart is null ? transform : bodyPart.transform;

                Vector3 startPosition = point.position;
                Vector3 endPosition = point.position;

                var hitCount = SUtility.Physics.DrawSphereCastAllNonAlloc(startPosition, endPosition, _ability._traceRadius, _hitColliders, _ability._traceLayer.Value, useDebug:UseDebug);

                Log($"HIT COUNT :: {hitCount}");

                if (hitCount > 0)
                {
                    bool wasHit = false;

                    for (int i = 0; i < hitCount; i++)
                    {
                        bool isHit = false;
                        var hit = _hitColliders[i];

                        Log($"HIT TARGET :: {hit.transform.name}");

                        if (!CheckAffilation(hit.transform))
                            continue;

                        Log($"HIT ENEMY :: {hit.transform.name}");

                        if (hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        {
                            if (_ability._takeDamage)
                            {
                                var data = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, startPosition.Direction(endPosition), gameObject, gameObject);

                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamage, gameObject, Level, data))
                                {
                                    isHit = true;
                                }

                                data.Release();
                            }

                            if(_ability._takeKnockback)
                            {
                                if(hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeKnockback, gameObject, Level, null))
                                    isHit = true;
                            }

                            if (_ability._takeStiffen)
                            {
                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeStiffen, gameObject, Level, null))
                                    isHit = true;
                            }

                            for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnHitToOther.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnHitToOther[effectIndex];

                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level, null))
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
                                                                    : hit.collider.ClosestPoint(startPosition);
                                Vector3 rotation = Quaternion.LookRotation(hit.normal, Vector3.up).eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
                                Vector3 scale = _ability._onHitToOtherCue.Scale;
                                float volume = _ability._onHitToOtherCue.Volume;

                                _ability._onHitToOtherCue.Cue.Play(position, rotation, scale, volume);
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

                                _gameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level, null);
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



            private void _onAttackCue_OnEndedCue(Cue cue)
            {
                _onAttackCue = null;
            }

            private void _animationEvents_OnExitNotifyState(string eventName)
            {
            }

            private void _animationEvents_OnEnterNotifyState(string eventName)
            {
            }

            private void _animationEvents_OnNotify(string eventName)
            {
                if (!IsPlaying)
                    return;

                switch (eventName)
                {
                    case "Swing":
                        OnPlayAttackCue();
                        break;
                    default:
                        break;
                }
            }

            private void _animationEvents_OnStartedBlendOut()
            {
                TryFinishAbility();
            }

            private void _animationEvents_OnFailed()
            {
                CancelAbility();
            }

            private void _animationEvents_OnCanceled()
            {
                if (!_wasStartedAnimation)
                    return;

                CancelAbility();
            }

            private void _animationEvents_OnStarted()
            {
                _wasStartedAnimation = true;
            }

            
        }
    }
}
