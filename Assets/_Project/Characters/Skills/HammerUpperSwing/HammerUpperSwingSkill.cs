using PF.PJT.Duet.Pawn;
using PF.PJT.Duet.Pawn.Effect;
using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.MovementSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Assets._Project.Characters.Skills.HammerUpperSwing
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Hammer Upper Swing Skill", fileName = "GA_Skill_HammerUpperSwing")]
    public class HammerUpperSwingSkill : CharacterSkill
    {
        [Header(" [ Hammer Upper Swing ] ")]
        [SerializeField] private string _animationName = "Hammer_UpperSwing";
        [SerializeField][Range(0f, 1f)]private float _fadeTime = 0.2f;

        [Header(" Find Target ")]
        [SerializeField][SUnit(SUtility.UNIT_METER)] private float _findTargetRadius = 10f;
        [SerializeField][SUnit(SUtility.UNIT_DEGREE)] private float _findTargetAngle = 90f;
        [SerializeField] private int _findTargetCount = 20;

        [Header(" Movement ")]
        [SerializeField][SUnit(SUtility.UNIT_METER)] private float _moveDistance = 10f;
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField][SUnit(SUtility.UNIT_METER)] private float _chaseOffset = 2f;
        
        [Header(" Turninig ")]
        [SerializeField][SUnit(SUtility.UNIT_DEGREE_PER_SEC)]private float _turnSpeed = 720f;
        
        
        [Header(" Attack ")]
        [SerializeField] private BodyTag _attackBodyPoint;
        [SerializeField][SUnit(SUtility.UNIT_METER)] private float _attackRadius = 1f;
        [SerializeField] private SOLayerMaskVariable _attackLayer;
        [SerializeField] private int _attackTraceCount = 20;

        [Header(" Gameplay Effect ")]
        [Header(" On Hit Target ")]
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private TakeStiffenEffect _takeStiffenEffect;
        [SerializeField] private TakeKnockbackEffect _takeKnockbackEffect;
        [SerializeField] private GameplayEffect[] _takeGameplayEffects;

        [Header(" On Hit Self ")]
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitSuccessed;

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

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly HammerUpperSwingSkill _ability;
            private readonly ICharacter _character;
            private readonly IPawnSystem _pawnSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly AnimationPlayer _animationPlayer;

            private readonly AnimationPlayer.Events _animationEvent = new();
            private readonly ReachValueToTime _movementValue = new();

            private readonly int _animationID;
            private bool _wasAnimationStart;

            private bool _wasPlayAttackCue;
            private Cue _onAttackCue;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as HammerUpperSwingSkill;
                _character = gameObject.GetComponent<ICharacter>();
                _pawnSystem = gameObject.GetPawnSystem();
                _bodySystem = gameObject.GetBodySystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();

                _animationPlayer = _character.Model.GetAnimationPlayer();
                _animationID = Animator.StringToHash(_ability._animationName);
                _animationEvent.OnStarted += _animationEvent_OnStarted;
                _animationEvent.OnFailed += _animationEvent_OnFailed;
                _animationEvent.OnCanceled += _animationEvent_OnCanceled;
                _animationEvent.OnStartedBlendOut += _animationEvent_OnStartedBlendOut;
                _animationEvent.OnEnterNotifyState += _animationEvent_OnEnterNotifyState;
                _animationEvent.OnExitNotifyState += _animationEvent_OnExitNotifyState;
            }

            

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _wasPlayAttackCue = false;

                if(_pawnSystem.IsPlayer)
                {
                    FindAttackTarget();
                }

                PlayAnimation();
                OnMovement();
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                StopAnimation();
                EndAttack();
                EndMovement();
                EndTurn();
            }
            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    TryPlayAttackCue();
                    UpdateMovement();
                    UpdateTurning(deltaTime);
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    UpdateAttack();
                }
            }

            // animation 
            private void PlayAnimation()
            {
                _wasAnimationStart = false;

                _animationPlayer.Play(_animationID, _ability._fadeTime);
                _animationPlayer.AnimationEvents = _animationEvent;
            }
            private void StopAnimation()
            {
                _animationPlayer.TryStopAnimation(_animationID);
            }

            // Enemy AI
            public void SetAttackTarget(GameObject attackTarget)
            {
                _attackTarget = attackTarget;
                _attackCollider = _attackTarget ? _attackTarget.GetComponent<Collider>() : null;
            }

            // Find Target
            private Collider[] _findTargets;
            private Camera _camera;
            private GameObject _attackTarget;
            private Collider _attackCollider;
            private readonly List<Collider> _tempColliders = new();
            private Camera Camera
            {
                get
                {
                    if (!_camera)
                        _camera = Camera.main;

                    return _camera;
                }
            }

            private void FindAttackTarget()
            {
                Log(nameof(FindAttackTarget));

                _attackTarget = null;
                _attackCollider = null;

                if (_findTargets is null || _findTargets.Length != _ability._findTargetCount)
                    _findTargets = new Collider[_ability._findTargetCount];

                var hitCount = SUtility.Physics.DrawOverlapSphereNoneAlloc(transform.position, _ability._findTargetRadius, _findTargets, _ability._attackLayer.Value, QueryTriggerInteraction.UseGlobal, UseDebug);

                if(hitCount > 0)
                {
                    _tempColliders.Clear();

                    Vector3 origin = Camera ? Camera.transform.position : transform.position;
                    Vector3 forward = _pawnSystem.LookDirection;

                    for (int i = 0; i < hitCount; i++)
                    {
                        var findTarget = _findTargets[i];

                        if (findTarget == null)
                            continue;

                        Vector3 direction = origin.Direction(findTarget.transform);
                        float angle = Vector3.SignedAngle(forward, direction, transform.up);

                        Log($"{nameof(FindAttackTarget)} - {findTarget.gameObject} is {angle}{SUtility.UNIT_DEGREE}");

                        if (angle.InRangeAngle(_ability._findTargetAngle))
                        {
                            Log($"{nameof(FindAttackTarget)} - is in angle target : {findTarget.gameObject}");

                            _tempColliders.Add(findTarget);
                        }
                    }

                    if (_tempColliders.Count > 0)
                    {
                        SUtility.Sort.SortDistanceToPointByBoundsCenter(transform, _tempColliders);

                        for (int i = 0; i < _tempColliders.Count; i++)
                        {
                            var target = _tempColliders[i];

                            if (!target.TryGetActor(out IActor actor))
                                continue;

                            if (actor.transform == transform)
                                continue;

                            if (actor.transform.TryGetReleationshipSystem(out IRelationshipSystem relationship) 
                                && _relationshipSystem.CheckRelationship(relationship) == ERelationship.Friendly)
                                continue;

                            Log($"Attack Target - {actor.gameObject.name}");

                            _attackTarget = actor.gameObject;
                            _attackCollider = target;

                            break;
                        }
                    }
                }
            }

            // Turning
            private bool _canTurn = false;

            private void OnTurn()
            {
                if (_canTurn)
                    return;

                Log(nameof(OnTurn));

                _canTurn = true;
            }
            private void EndTurn()
            {
                if (!_canTurn)
                    return;

                Log(nameof(EndTurn));

                _canTurn = false;
            }
            private void UpdateTurning(float deltaTime)
            {
                if (!_canTurn)
                    return;

                Vector3 lookDirection;

                if(_attackTarget)
                {
                    lookDirection = transform.Direction(_attackTarget.transform);
                }
                else
                {
                    lookDirection = _pawnSystem.LookDirection;
                }

                Quaternion newRotation = Quaternion.LookRotation(lookDirection, transform.up);
                float yaw = Mathf.MoveTowardsAngle(transform.eulerAngles.y, newRotation.eulerAngles.y, _ability._turnSpeed * deltaTime);

                _rotationSystem.SetRotation(Quaternion.Euler(0, yaw, 0), false);
            }

            // Movement
            private void OnMovement()
            {
                if (_movementValue.IsPlaying)
                    return;

                Log(nameof(OnMovement));

                float distance;

                if(_attackTarget)
                {
                    distance = transform.position.HorizontalDistance(_attackCollider.ClosestPoint(transform.position));
                    distance -= _ability._chaseOffset;

                    if (distance <= 0)
                        distance = 0f;
                }
                else
                {
                    distance = _ability._moveDistance;
                }

                _movementValue.OnMovement(distance, _ability._moveCurve);
            }
            private void EndMovement()
            {
                if (!_movementValue.IsPlaying)
                    return;
                
                Log(nameof(EndMovement));

                _movementValue.EndMovement();
            }
            private void UpdateMovement()
            {
                if (!_animationPlayer.IsPlaying)
                    return;

                var speed = _movementValue.UpdateMovement(_animationPlayer.NormalizedTime);

                _movementSystem.MovePosition(transform.forward * speed);
            }


            // Attack 
            private bool _canAttackTrace;
            private Transform _attackTracePoint;
            private Vector3 _prevAttackPosition;
            private RaycastHit[] _hitResults;
            private readonly List<Transform> _ignoreTransforms = new();
            private void OnAttack()
            {
                if (_canAttackTrace)
                    return;

                Log(nameof(OnAttack));

                _canAttackTrace = true;

                _ignoreTransforms.Clear();
                _ignoreTransforms.Add(transform);

                if(_hitResults is null || _hitResults.Length != _ability._attackTraceCount)
                {
                    _hitResults = new RaycastHit[_ability._attackTraceCount];
                }

                var bodyPart = _bodySystem.GetBodyPart(_ability._attackBodyPoint);

                _attackTracePoint = bodyPart is not null ? bodyPart.transform : transform;
                _prevAttackPosition = _attackTracePoint.position;
            }
            private void EndAttack()
            {
                if (!_canAttackTrace)
                    return;
                
                Log(nameof(EndAttack));

                _canAttackTrace = false;

            }
            private void UpdateAttack()
            {
                if (!_canAttackTrace)
                    return;

                Vector3 startPosition = _prevAttackPosition;
                Vector3 endPosition = _attackTracePoint.position;
                
                _prevAttackPosition = endPosition;

                var hitCount = SUtility.Physics.DrawSphereCastAllNonAlloc(startPosition, endPosition, _ability._attackRadius, _hitResults, _ability._attackLayer.Value, QueryTriggerInteraction.UseGlobal, UseDebug);
                
                if(hitCount > 0)
                {
                    bool anyHit = false;

                    for(int i = 0; i < hitCount; i++)
                    {
                        var hitResult = _hitResults[i];

                        if (_ignoreTransforms.Contains(hitResult.transform))
                            continue;

                        _ignoreTransforms.Add(hitResult.transform);

                        if (!hitResult.transform.TryGetActor(out IActor actor))
                            continue;

                        if (transform == hitResult.transform)
                            continue;

                        if (actor.transform.TryGetReleationshipSystem(out IRelationshipSystem relationship) && _relationshipSystem.CheckRelationship(relationship) == ERelationship.Friendly)
                            continue;

                        if (actor.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem gameplayEffectSystem))
                        {
                            bool isHit = false;

                            if(_ability._takeDamageEffect)
                            {
                                var element = TakeDamageEffect.Element.Get(hitResult.point, hitResult.normal, hitResult.collider, transform.forward, gameObject, gameObject);

                                if (gameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamageEffect, gameObject, Level, element))
                                    isHit = true;
                            }

                            if(_ability._takeKnockbackEffect)
                            {
                                if (gameplayEffectSystem.TryApplyGameplayEffect(_ability._takeKnockbackEffect, gameObject, Level))
                                    isHit = true;
                            }

                            if(_ability._takeStiffenEffect)
                            {
                                if (gameplayEffectSystem.TryApplyGameplayEffect(_ability._takeStiffenEffect, gameObject, Level))
                                    isHit = true;
                            }

                            if(!_ability._takeGameplayEffects.IsNullOrEmpty())
                            {
                                for (int j = 0; j < _ability._takeGameplayEffects.Length; j++)
                                {
                                    var gameplayEffect = _ability._takeGameplayEffects[j];
                                    if (gameplayEffectSystem.TryApplyGameplayEffect(gameplayEffect, gameObject, Level))
                                        isHit = true;
                                }
                            }

                            if(isHit)
                            {
                                anyHit = true;

                                if (_ability._onHitToOtherCue.Cue)
                                {
                                    Vector3 position = hitResult.distance > 0 ? hitResult.point + hitResult.transform.TransformDirection(_ability._onHitToOtherCue.Position)
                                                                        : hitResult.collider.ClosestPoint(startPosition);
                                    Vector3 rotation = Quaternion.LookRotation(hitResult.normal, Vector3.up).eulerAngles + hitResult.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
                                    Vector3 scale = _ability._onHitToOtherCue.Scale;
                                    float volume = _ability._onHitToOtherCue.Volume;

                                    _ability._onHitToOtherCue.Cue.Play(position, rotation, scale, volume);
                                }
                            }
                        }
                    }

                    if (anyHit && !_ability._applyGameplayEffectsOnHitSuccessed.IsNullOrEmpty())
                    {
                        for(int i = 0; i < _ability._applyGameplayEffectsOnHitSuccessed.Length; i++)
                        {
                            var gameplayEffect = _ability._applyGameplayEffectsOnHitSuccessed[i];

                            _gameplayEffectSystem.TryApplyGameplayEffect(gameplayEffect, gameObject, Level);
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


            // Cue
            private void TryPlayAttackCue()
            {
                if (_wasPlayAttackCue)
                    return;

                if(_animationPlayer.NormalizedTime >= _ability._attackCueTime)
                {
                    OnAttackCue();
                }
            }
            private void OnAttackCue()
            {
                if (!IsPlaying)
                    return;

                if (!_ability._onAttackCue.Cue)
                    return;

                if (_wasPlayAttackCue)
                    return;


                _wasPlayAttackCue = true;

                _onAttackCue = _ability._onAttackCue.PlayAttached(transform);
                _onAttackCue.OnEndedCue += _onAttackCue_OnEndedCue;
            }

            private void _onAttackCue_OnEndedCue(Cue cue)
            {
                _onAttackCue = null;
            }

            private void _animationEvent_OnStarted()
            {
                _wasAnimationStart = true;
            }
            private void _animationEvent_OnFailed()
            {
                CancelAbility();
            }
            private void _animationEvent_OnCanceled()
            {
                if (_wasAnimationStart)
                    return;

                CancelAbility();
            }
            private void _animationEvent_OnStartedBlendOut()
            {
                TryFinishAbility();
            }

            private void _animationEvent_OnEnterNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Trace":
                        OnAttack();
                        break;
                    case "Turn":
                        OnTurn();
                        break;
                    default:
                        break;
                }
            }
            private void _animationEvent_OnExitNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Trace":
                        EndAttack();
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
