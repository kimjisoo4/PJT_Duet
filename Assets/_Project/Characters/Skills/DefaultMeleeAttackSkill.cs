using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.MovementSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Default Melee Attack Skill", fileName = "GA_Skill_", order = -999999)]
    public class DefaultMeleeAttackSkill : CharacterSkill
    {
        [Header(" [ Default Melee Attack Skill ] ")]
        [Header(" Chase ")]
        [Header(" Chase Animation ")]
        [SerializeField] private string _chaseAnimationName = "Attack01_Chase";
        [SerializeField][Range(0f, 1f)] private float _chaseFadeInTime = 0.2f;

        [Header(" Find Attack Target ")]
        [SerializeField][SUnit(SUtility.UNIT_METER)] private float _findDistance = 10f;
        [SerializeField][SUnit(SUtility.UNIT_DEGREE)] private float _findAngle = 90f;
        [SerializeField] private int _findTargetCount = 20;

        [Header(" Chase Attack Target ")]
        [SerializeField][SUnit(SUtility.UNIT_METER)] private float _chaseDistance = 3f;
        [SerializeField][SUnit(SUtility.UNIT_METER_PER_SEC)] private float _chaseSpeed = 10f;

        [Header(" Turning Target ")]
        [SerializeField][SUnit(SUtility.UNIT_DEGREE_PER_SEC)] private float _chaseTurnSpeed = 720f;

        [Header(" Attack ")]
        [Header(" Animation ")]
        [SerializeField] private string _animationName = "Attack01";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;

        [Header(" Turning ")]
        [SerializeField][SUnit(SUtility.UNIT_DEGREE_PER_SEC)] private float _attackTurnSpeed = 720f;

        [Header(" Attack Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private SOLayerMaskVariable _traceLayer;
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
            private readonly int _chaseAnimationID;
            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IDilationSystem _dilationSystem;

            private readonly AnimationPlayer.Events _animationEvents;
            private readonly TrailSphereCast _trailSphereCast = new();

            private Camera _camera;
            private Camera Camera
            {
                get
                {
                    if (!_camera)
                        _camera = Camera.main;

                    return _camera;
                }
            }

            private bool _isChasing;
            private bool _canTurn;

            private bool _wasStartedAnimation;

            private bool _wasPlayAttackCue;
            private Cue _onAttackCue;

            private Collider[] _findColliders;
            private readonly List<Collider> _tempColliders = new();
            private GameObject _attackTarget;

            private Collider _selfCollider;
            private Collider _attackTargetCollider;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DefaultMeleeAttackSkill;
                
                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _pawnSystem = gameObject.GetPawnSystem();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();
                _bodySystem = gameObject.GetBodySystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _dilationSystem = gameObject.GetDilationSystem();
                _selfCollider = gameObject.GetComponent<Collider>();

                _dilationSystem.OnChangedDilation += DilationSystem_OnChangedDilation;

                _animationID = Animator.StringToHash(_ability._animationName);
                _chaseAnimationID = Animator.StringToHash(_ability._chaseAnimationName);
                _animationEvents = new();

                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;
            }

            

            private void DilationSystem_OnChangedDilation(IDilationSystem dilation, float currentDilation, float prevDilation)
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

                if(_pawnSystem.IsPlayer)
                {
                    _attackTarget = null;

                    FindAttackTarget();

                    if (_attackTarget)
                    {
                        Vector3 startPosition = _selfCollider.ClosestPointOnBounds(_attackTarget.transform.position);
                        Vector3 targetPosition = _attackTargetCollider.ClosestPoint(transform.position);

                        if (Vector3.Distance(startPosition, targetPosition) > _ability._chaseDistance)
                        {
                            OnChase();
                        }
                        else
                        {
                            PlayAttackAnimation();
                        }
                    }
                    else
                    {
                        PlayAttackAnimation();
                    }
                }
                else
                {
                    PlayAttackAnimation();
                }
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                EndTurn();
                EndChase();

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
                    UpdateChase(deltaTime);
                    UpdateRotation(deltaTime);

                    if(!_isChasing)
                    {
                        if (!_wasPlayAttackCue)
                        {
                            float normalizedTime = _animationPlayer.NormalizedTime;

                            if (normalizedTime >= _ability._attackCueTime)
                            {
                                _wasPlayAttackCue = true;

                                OnAttackCue();
                            }
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

            private void PlayChaseAnimation()
            {
                _animationPlayer.Play(_chaseAnimationID, _ability._chaseFadeInTime);
            }

            private void PlayAttackAnimation()
            {
                _wasStartedAnimation = false;

                _animationPlayer.Play(_animationID, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                _wasPlayAttackCue = !_ability._onAttackCue.Cue;
            }
            private void FindAttackTarget()
            {
                Log(nameof(FindAttackTarget));

                if(_findColliders is null || _findColliders.Length != _ability._findTargetCount)
                {
                    _findColliders = new Collider[_ability._findTargetCount];
                }

                var hitCount = SUtility.Physics.DrawOverlapSphereNoneAlloc(transform.position, 
                                                                           _ability._findDistance, 
                                                                           _findColliders, 
                                                                           _ability._traceLayer.Value, 
                                                                           QueryTriggerInteraction.UseGlobal, 
                                                                           UseDebug);

                if(hitCount > 0)
                {
                    _tempColliders.Clear();

                    Vector3 origin = Camera ? Camera.transform.position : transform.position;
                    Vector3 forward = _pawnSystem.LookDirection;

                    for (int i = 0; i < hitCount; i++)
                    {
                        var findTarget = _findColliders[i];

                        if (findTarget == null)
                            continue;

                        Vector3 direction = origin.Direction(findTarget.transform);
                        float angle = Vector3.SignedAngle(forward, direction, transform.up);

                        Log($"{nameof(FindAttackTarget)} - {findTarget.gameObject} is {angle}{SUtility.UNIT_DEGREE}");

                        if (angle.InRangeAngle(_ability._findAngle))
                        {
                            Log($"{nameof(FindAttackTarget)} - is in angle target : {findTarget.gameObject}");

                            _tempColliders.Add(findTarget);
                        }
                    }

                    if(_tempColliders.Count > 0)
                    {
                        SUtility.Sort.SortDistanceToPointByBoundsCenter(transform, _tempColliders);

                        for(int i = 0; i < _tempColliders.Count; i++)
                        {
                            var target = _tempColliders[i];

                            if (!target.TryGetActor(out IActor actor))
                                continue;

                            if (actor.transform == transform)
                                continue;

                            if (!CheckAffilation(actor.transform))
                                continue;

                            Log($"Attack Target - {actor.gameObject.name}");
                            _attackTarget = actor.gameObject;
                            _attackTargetCollider = actor.gameObject.GetComponent<Collider>();

                            break;
                        }
                    }
                }
            }



            // Chase


            private void OnChase()
            {
                if (_isChasing)
                    return;

                _isChasing = true;

                PlayChaseAnimation();
            }
            private void UpdateChase(float deltaTime)
            {
                if (!_isChasing)
                    return;

                Vector3 direction = transform.HorizontalDirection(_attackTarget.transform);
                Vector3 velocity = direction * (_ability._chaseSpeed * deltaTime);

                _movementSystem.MovePosition(velocity);

                Vector3 startPosition = _selfCollider.ClosestPoint(_attackTarget.transform.position);
                Vector3 targetPosition = _attackTargetCollider.ClosestPoint(transform.position);

                float distance = Vector3.Distance(startPosition, targetPosition);

                if(distance < _ability._chaseDistance)
                {
                    EndChase();
                }
            }
            private void EndChase()
            {
                if (!_isChasing)
                    return;

                _isChasing = false;

                PlayAttackAnimation();
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


            // Rotation 

            private void OnTurn()
            {
                if (_canTurn)
                    return;

                _canTurn = true;
            }
            private void UpdateRotation(float deltaTime)
            {
                if (!_canTurn && !_isChasing)
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

                float speed = _isChasing ? _ability._chaseTurnSpeed : _ability._attackTurnSpeed;
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection, transform.up);
                float yaw = Mathf.MoveTowardsAngle(transform.eulerAngles.y, lookRotation.eulerAngles.y, deltaTime * speed);

                _rotationSystem.SetRotation(Quaternion.Euler(0, yaw, 0));
            }
            private void EndTurn()
            {
                if (!_canTurn)
                    return;

                _canTurn = false;
            }

            // Attack 
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
