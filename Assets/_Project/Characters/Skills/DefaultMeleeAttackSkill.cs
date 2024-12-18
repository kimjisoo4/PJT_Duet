﻿using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Default Melee Attack Skill", fileName = "GA_Skill_", order = -999999)]
    public class DefaultMeleeAttackSkill : CharacterSkill
    {
        [Header(" [ Default Melee Attack Skill ] ")]
        [Header(" Animation ")]
        [SerializeField] private string _animationName = "Attack01";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;

        [Header(" Turn ")]
        [SerializeField] private GameplayTag _turnTag;

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
        [Header(" Attack Cue")]
        [SerializeField][Range(0f, 1f)] private float _attackCueTime = 0.2f;
        [SerializeField] private FGameplayCue _onAttackCue = FGameplayCue.Default;
        [Header(" Hit Cue ")]
        [SerializeField] private FGameplayCue _onHitToOtherCue = FGameplayCue.Default;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit = FGameplayCue.Default;

        [Header(" Combo ")]
        [SerializeField] private GameplayTag _comboTag;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }
        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly DefaultMeleeAttackSkill _ability;

            private readonly int _animationID;
            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IDilationSystem _dilationSystem;

            private readonly AnimationPlayer.Events _animationEvents;
            private readonly TrailSphereCast _trailSphereCast = new();
            private bool _wasStartedAnimation;

            private bool _wasPlayAttackCue;
            private Cue _onAttackCue;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DefaultMeleeAttackSkill;
                
                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _pawnSystem = gameObject.GetPawnSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _bodySystem = gameObject.GetBodySystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _dilationSystem = gameObject.GetDilationSystem();

                _dilationSystem.OnChangedDilation += _dilationSystem_OnChangedDilation;

                _animationID = Animator.StringToHash(_ability._animationName);
                _animationEvents = new();

                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;
            }

            

            private void _dilationSystem_OnChangedDilation(IDilationSystem dilation, float currentDilation, float prevDilation)
            {
                if (_onAttackCue is null)
                    return;

                if(currentDilation.SafeEquals(0f))
                {
                    _onAttackCue.Pause();
                }
                else if(currentDilation.SafeEquals(1f))
                {
                    _onAttackCue.Resume();
                }
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

                _wasStartedAnimation = false;

                _animationPlayer.Play(_animationID, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                _wasPlayAttackCue = !_ability._onAttackCue.Cue;
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                _animationPlayer.TryStopAnimation(_animationID);

                if (_onAttackCue is not null)
                {
                    _onAttackCue.Stop();
                    _onAttackCue = null;
                }
            }
            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

                if (_onAttackCue is not null)
                {
                    _onAttackCue.Stop();
                    _onAttackCue = null;
                }
            }
            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    if (!_wasPlayAttackCue)
                    {
                        float normalizedTime = _animationPlayer.NormalizedTime;

                        if(normalizedTime >= _ability._attackCueTime)
                        {
                            _wasPlayAttackCue = true;

                            OnAttackCue();
                        }
                    }
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
                if(_ability._comboTag)
                    GameplayTagSystem.AddOwnedTag(_ability._comboTag);
            }
            private void EndCombo()
            {
                if(_ability._comboTag)
                    GameplayTagSystem.RemoveOwnedTag(_ability._comboTag);
            }

            private void OnTurn()
            {
                if (_ability._turnTag)
                    GameplayTagSystem.AddOwnedTag(_ability._turnTag);
            }
            private void EndTurn()
            {
                if (_ability._turnTag)
                    GameplayTagSystem.RemoveOwnedTag(_ability._turnTag);
            }
            private void OnTrace()
            {
                if (_trailSphereCast.IsPlaying)
                    return;

                var bodypart = _bodySystem.GetBodyPart(_ability._tracePoint);

                _trailSphereCast.SetOwner(bodypart.gameObject);

                _trailSphereCast.AddIgnoreTransform(transform);

                _trailSphereCast.TraceRadius = _ability._traceRadius;
                _trailSphereCast.TraceLayer = _ability._traceLayer.Value;
                _trailSphereCast.MaxHitCount = 20;
                _trailSphereCast.UseDebug = UseDebug;

                _trailSphereCast.OnTrace();
            }
            private void EndTrace()
            {
                _trailSphereCast.EndTrace();
            }

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

            private void UpdateTrace()
            {
                if (!_trailSphereCast.IsPlaying)
                    return;

                var (hitCount, hitResults) = _trailSphereCast.UpdateTrace();

                if (hitCount > 0)
                {
                    bool wasHit = false;

                    for (int i = 0; i < hitCount; i++)
                    {
                        bool isHit = false;
                        var hit = hitResults[i];

                        if (!CheckAffilation(hit.transform))
                            continue;

                        Log($"HIT :: {hit.transform.name}");

                        if (hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        {
                            if (_ability._takeDamageEffect)
                            {
                                var data = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, _trailSphereCast.StartPosition.Direction(_trailSphereCast.EndPosition), _trailSphereCast.Owner.gameObject, gameObject);

                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamageEffect, gameObject, Level, data))
                                {
                                    isHit = true;
                                }

                                data.Release();
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
                                                                    : hit.collider.ClosestPoint(_trailSphereCast.StartPosition);
                                Vector3 rotation = Quaternion.LookRotation(hit.normal, Vector3.up).eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
                                Vector3 scale = _ability._onHitToOtherCue.Scale;
                                float volume = _ability._onHitToOtherCue.Volume;

                                _ability._onHitToOtherCue.Cue.Play(position, rotation, scale, volume);
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

                                _gameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level, null);
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

            private void OnAttackCue()
            {
                if (!IsPlaying)
                    return;

                if (!_ability._onAttackCue.Cue)
                    return;

                _onAttackCue = _ability._onAttackCue.PlayAttached(transform);
                _onAttackCue.OnEndedCue += _onAttackCue_OnEndedCue;
            }

            private void _onAttackCue_OnEndedCue(Cue cue)
            {
                _onAttackCue = null;
            }

            private void _animationEvents_OnStarted()
            {
                _wasStartedAnimation = true;
            }
            private void _animationEvents_OnCanceled()
            {
                if (!_wasStartedAnimation)
                    return;

                CancelAbility();
            }
            private void _animationEvents_OnFailed()
            {
                CancelAbility();
            }
            private void _animationEvents_OnStartedBlendOut()
            {
                TryFinishAbility();
            }

            private void _animationEvents_OnEnterNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Trace":
                        OnTrace();
                        break;
                    case "Combo":
                        OnCombo();
                        break;
                    case "Turn":
                        OnTurn();
                        break;
                    default:
                        break;
                }
            }

            private void _animationEvents_OnExitNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Trace":
                        EndTrace();
                        break;
                    case "Combo":
                        EndCombo();
                        break;
                    case "Turn":
                        EndTurn();
                        break;
                    default:
                        break;
                }
            }

            
        }
    }
}
