using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.MovementSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Appear Sword Skill", fileName = "GA_Skill_Appear_Sword")]
    public class AppearSwordSkill : CharacterSkill
    {
        [Header(" [ Appear Sword ]")]
        [Header(" Animation ")]
        [SerializeField] private string _airAnimation = "AppearSword_Air";
        [SerializeField][Range(0f, 1f)] private float _airAnimFadeTime = 0f;

        [SerializeField] private string _attackAnimation = "AppearSword_Slash";
        [SerializeField][Range(0f, 1f)] private float _attackAnimFadeTime = 0.2f;

        [Header(" Movement ")]
        [SerializeField] private float _moveDistance = 5f;
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [Space(10f)]
        [SerializeField] private float _jumpHeight = 5f;
        [SerializeField] private AnimationCurve _jumpMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header(" Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private float _traceRadius = 2f;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private TakeRadialKnockbackEffect _takeRadialKnockbackEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToSelf;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _onAbilityCue;
        [SerializeField] private FGameplayCue _onImpactCue;
        [SerializeField] private FGameplayCue _onHitToOtherCue;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit;

        [SerializeField] private LayerMask _testLayer;
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, ISkillState, IUpdateableAbilitySpec
        {
            public enum EState
            {
                None,
                JumpMovement,
                WaitGround,
                Attack,
            }

            public abstract class SkillState : BaseStateClass
            {
                private readonly Spec _spec;

                protected Spec Spec => _spec;
                protected AppearSwordSkill Ability => _spec._ability;
                protected AnimationPlayer AnimationPlayer => _spec._animationPlayer;
                protected FiniteStateMachineSystemWithKey<EState, SkillState> StateMachine => _spec._stateMachine;

                protected Transform transform => _spec.transform;
                protected GameObject gameObject => _spec.gameObject;


                public override bool UseDebug => _spec.UseDebug;
                public override UnityEngine.Object Context => _spec.Context;

                public SkillState(Spec spec)
                {
                    _spec = spec;
                }

                public virtual void UpdateState(float deltaTime)
                {

                }
            }

            public class AttackState : SkillState
            {
                private readonly int _animationHash;
                private readonly AnimationPlayer.Events _animationEvents;
                private bool _wasStarted;

                public AttackState(Spec spec) : base(spec)
                {
                    _animationHash = Animator.StringToHash(Ability._attackAnimation);

                    _animationEvents = new();
                    _animationEvents.OnStarted += _animationEvents_OnStarted;
                    _animationEvents.OnFailed += _animationEvents_OnFailed;
                    _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                    _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                    _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                    _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;
                    _animationEvents.OnNotify += _animationEvents_OnNotify;
                }

                

                protected override void EnterState()
                {
                    _wasStarted = false;

                    AnimationPlayer.Play(_animationHash, Ability._attackAnimFadeTime);
                    AnimationPlayer.AnimationEvents = _animationEvents;
                }

                private void _animationEvents_OnStartedBlendOut()
                {
                    Spec.TryFinishAbility();
                }

                private void _animationEvents_OnCanceled()
                {
                    Spec.CancelAbility();
                }

                private void _animationEvents_OnStarted()
                {
                    _wasStarted = true;
                }
                private void _animationEvents_OnFailed()
                {
                    if (!_wasStarted)
                        return;

                    Spec.CancelAbility();
                }

                private void _animationEvents_OnNotify(string eventName)
                {
                    switch(eventName)
                    {
                        case "Impact":
                            Spec.PlayImpactFX();
                            break;
                        default:
                            break;
                    }
                }
                private void _animationEvents_OnEnterNotifyState(string eventName)
                {
                    switch (eventName)
                    {
                        case "Trace":
                            Spec.OnTrace();
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
                            Spec.EndTrace();
                            break;

                        default:
                            break;
                    }
                }
            }
            public class JumpMovementState : SkillState
            {
                private readonly int _animationHash;

                private readonly ReachValueToTime _horizontalMove;
                private readonly ReachValueToTime _verticalMove;

                private Vector3 _moveDirection;

                public JumpMovementState(Spec spec) : base(spec)
                {
                    _animationHash = Animator.StringToHash(Ability._airAnimation);

                    _horizontalMove = new ReachValueToTime(Ability._moveDistance, Ability._moveCurve);
                    _verticalMove = new ReachValueToTime(Ability._jumpHeight, Ability._jumpMoveCurve);
                }

                protected override void EnterState()
                {
                    AnimationPlayer.Play(_animationHash, Ability._airAnimFadeTime);

                    _horizontalMove.OnMovement(Ability._moveDistance);
                    _verticalMove.OnMovement(Ability._jumpHeight);

                    _moveDirection = transform.HorizontalDirection(Spec._pawnSystem.LookPosition);

                    Spec._rotationSystem.SetRotation(Quaternion.LookRotation(_moveDirection, Vector3.up), false);
                }
                protected override void ExitState()
                {
                    base.ExitState();

                    _horizontalMove.EndMovement();
                    _verticalMove.EndMovement();
                }

                public override void UpdateState(float deltaTime)
                {
                    if (AnimationPlayer.IsPlaying)
                    {
                        _horizontalMove.UpdateMovement(AnimationPlayer.NormalizedTime);
                        _verticalMove.UpdateMovement(AnimationPlayer.NormalizedTime);

                        Vector3 movePosition = _moveDirection * _horizontalMove.DeltaDistance;
                        movePosition.y = _verticalMove.DeltaDistance;

                        Spec._movementSystem.MovePosition(movePosition);

                        Log($"Normalized Time {AnimationPlayer.NormalizedTime} " +
                            $":: Horizontal Delta Position {_horizontalMove.DeltaDistance} " +
                            $":: Vertical Delta Position {_verticalMove.DeltaDistance}");
                    }
                    

                    if(AnimationPlayer.NormalizedTime >= 0.5f && Spec._movementSystem.IsGrounded)
                    {
                        StateMachine.TrySetState(EState.Attack);
                    }
                }
            }

            protected new readonly AppearSwordSkill _ability;

            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private readonly TrailSphereCast _sphereCast = new();

            private readonly FiniteStateMachineSystemWithKey<EState, SkillState> _stateMachine;
            private readonly JumpMovementState _jumpMovementState;
            private readonly AttackState _attackState;


            private CoolTimeEffect.Spec _coolTimeSpec;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 0f : _coolTimeSpec.RemainTime;
            public float NormalizedCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 1f : _coolTimeSpec.NormalizedTime;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AppearSwordSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _pawnSystem = gameObject.GetPawnSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _bodySystem = gameObject.GetBodySystem();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();

                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _stateMachine = new FiniteStateMachineSystemWithKey<EState, SkillState>();
                _jumpMovementState = new(this);
                _attackState = new(this);

                _stateMachine.SetDefaultState(EState.JumpMovement, _jumpMovementState);
                _stateMachine.AddState(EState.Attack, _attackState);

            }

            public override bool CanActiveAbility()
            {
                if (_ability._coolTimeEffect && _gameplayEffectSystem.HasEffect(_ability._coolTimeEffect))
                    return false;

                if (!base.CanActiveAbility())
                    return false;

                return true;
            }


            protected override void EnterAbility()
            {
                base.EnterAbility();

                _stateMachine.Start();
                if (_ability._coolTimeEffect)
                {
                    if (_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                    {
                        _coolTimeSpec = spec as CoolTimeEffect.Spec;
                        _coolTimeSpec.OnEndedEffect += _coolTimeSpec_OnEndedEffect;
                    }
                }

                if(_ability._onAbilityCue.Cue)
                {
                    _ability._onAbilityCue.PlayFromTarget(transform);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _stateMachine.End();
            }
            private void _coolTimeSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeSpec_OnEndedEffect;

                _coolTimeSpec = null;
            }

            private void PlayImpactFX()
            {
                if(_ability._onImpactCue.Cue)
                {
                    _ability._onImpactCue.PlayFromTarget(transform);
                }
            }

            
            public void UpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    _stateMachine.CurrentState.UpdateState(deltaTime);
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                UpdateTrace();
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

                        Log($"HIT :: {hit.transform.name}");

                        if (hit.transform.TryGetReleationshipSystem(out IRelationshipSystem hitRelationship) && _relationshipSystem.CheckRelationship(hitRelationship) != ERelationship.Hostile)
                            continue;

                        if (hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        {
                            if (_ability._takeDamageEffect)
                            {
                                var data = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, _sphereCast.StartPosition.Direction(_sphereCast.EndPosition), _sphereCast.Owner, gameObject);

                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamageEffect, gameObject, Level, data))
                                {
                                    isHit = true;
                                }

                                data.Release();
                            }

                            if(_ability._takeRadialKnockbackEffect)
                            {
                                var data = TakeRadialKnockbackEffect.Element.Get(_sphereCast.Owner.transform.position);

                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeRadialKnockbackEffect, gameObject, Level, data))
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
                                                                    : hit.collider.ClosestPoint(_sphereCast.StartPosition);
                                Quaternion hitRotation = hit.normal.SafeEquals(Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(hit.normal, Vector3.up);
                                Vector3 rotation = hitRotation.eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
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
        }
    }
}