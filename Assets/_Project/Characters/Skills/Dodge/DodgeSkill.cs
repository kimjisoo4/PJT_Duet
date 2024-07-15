using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;
using StudioScor.MovementSystem;
using StudioScor.AbilitySystem;
using StudioScor.RotationSystem;
using PF.PJT.Duet.Pawn.Effect;
using StudioScor.GameplayEffectSystem;
using System;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Dodge Skill", fileName = "GA_Skill_Dodge")]
    public class DodgeSkill : GASAbility, ISkill
    {
        [Serializable]
        public struct FAnimationData
        {
            [SerializeField] private string _animName;
            [SerializeField][Range(0f, 1f)] private float _fadeInTime;
            [SerializeField] private AnimationCurve _movementCurve;

            public readonly string AnimName => _animName;
            public readonly float FadeInTime => _fadeInTime;
            public readonly AnimationCurve MovementCurve => _movementCurve;
        }                  
        [Serializable]
        public struct FDirectionalAnimationData
        {
            [SerializeField] private FAnimationData _forward;

            [SerializeField] private FAnimationData _backward;

            [SerializeField] private FAnimationData _left;

            [SerializeField] private FAnimationData _right;

            public readonly FAnimationData Forward => _forward;
            public readonly FAnimationData Backward => _backward;
            public readonly FAnimationData Left => _left;
            public readonly FAnimationData Right => _right;
        }

        [Header(" [ Dodge Skill ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private bool _isSkill;
        [SerializeField] private ESkillType _skillType;

        [Header(" Animation")]
        [SerializeField] private float _animDuration = 1f;
        [SerializeField] private FDirectionalAnimationData _animationDatas;
        [SerializeField] private string _motionTime = "motionTime";

        [Header(" Movement ")]
        [SerializeField] private float _distance = 5f;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;

        public Sprite Icon => _icon;
        public bool IsSkill => _isSkill;
        public ESkillType SkillType => _skillType;
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly DodgeSkill _ability;
            private readonly IPawnSystem _pawnSystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly AnimationPlayer _animationPlayer;
            private readonly AnimationPlayer.Events _animationEvents;
            private readonly ReachValueToTime _moveValueToTime = new();
            private readonly int[] _animationIDs;

            private int _animationID;
            private int _motionTimeID;
            private readonly Timer _moveTimer = new();
            private FAnimationData _animationData;
            private Vector3 _moveDirection;

            private readonly MatchTargetWeightMask _matchTargetWeight = new MatchTargetWeightMask(new Vector3(1, 0, 1), 0);

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DodgeSkill;

                _pawnSystem = gameObject.GetPawnSystem();
                _movementSystem = gameObject.GetMovementSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _animationEvents = new();

                _animationEvents.OnFailed += _animationPlayer_OnFailed;
                _animationEvents.OnCanceled += _animationPlayer_OnCanceled;
                _animationEvents.OnStartedBlendOut += _animationEvets_OnStartedBlendOut;

                _animationIDs = new int[] {Animator.StringToHash(_ability._animationDatas.Forward.AnimName),
                                           Animator.StringToHash(_ability._animationDatas.Backward.AnimName),
                                           Animator.StringToHash(_ability._animationDatas.Left.AnimName),
                                           Animator.StringToHash(_ability._animationDatas.Right.AnimName),
                };
                _motionTimeID = Animator.StringToHash(_ability._motionTime);
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

                _moveDirection = _pawnSystem.MoveDirection;

                if (_moveDirection.SafeEquals(Vector3.zero))
                {
                    _moveDirection = -transform.forward;
                    _moveDirection.y = 0;
                    _moveDirection.Normalize();
                }

                Log($"Move Direction - {_moveDirection}");

                float angle = Vector3.SignedAngle(_pawnSystem.transform.forward, _moveDirection, Vector3.up);

                Log($"Anlge - {angle:f2}");

                if (angle.InRange(-45, 45))
                {
                    _animationID = _animationIDs[0];
                    _animationData = _ability._animationDatas.Forward;
                }
                else if (angle.InRange(-135, -45))
                {
                    _animationID = _animationIDs[2];
                    _animationData = _ability._animationDatas.Left;
                }
                else if (angle.InRange(45, 135))
                {
                    _animationID = _animationIDs[3];
                    _animationData = _ability._animationDatas.Right;
                }
                else
                {
                    _animationID = _animationIDs[1];
                    _animationData = _ability._animationDatas.Backward;
                }
                

                _animationPlayer.Play(_animationID, _animationData.FadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                if (_moveTimer.IsPlaying)
                    _moveTimer.CancelTimer();

                if (_moveValueToTime.IsPlaying)
                    _moveValueToTime.EndMovement();

                _moveTimer.OnTimer(_ability._animDuration);
                _moveValueToTime.OnMovement(_ability._distance, _animationData.MovementCurve);
            }

            
            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

                _moveTimer.CancelTimer();
                _moveValueToTime.EndMovement();
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                if (_animationPlayer.IsPlayingHash(_animationID))
                {
                    _animationPlayer.AnimationEvents = null;
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                if (_moveTimer.IsPlaying)
                {
                    if (!IsPlaying && !_animationPlayer.IsPlayingHash(_animationID))
                    {
                        _moveTimer.CancelTimer();
                        _moveValueToTime.EndMovement();

                        return;
                    }
                    _moveTimer.UpdateTimer(deltaTime);

                    float normalizedTime = _moveTimer.NormalizedTime;

                    _animationPlayer.Animator.SetFloat(_motionTimeID, normalizedTime);

                    UpdateMovement(normalizedTime);
                }
            }


            private void UpdateMovement(float normalizedTime)
            {
                _moveValueToTime.UpdateMovement(normalizedTime);

                Vector3 movePosition = _moveValueToTime.DeltaDistance * _moveDirection;

                _movementSystem.MovePosition(movePosition);
            }

            private void _animationPlayer_OnCanceled()
            {
                CancelAbility();
            }
            private void _animationPlayer_OnFailed()
            {
                CancelAbility();
            }
            private void _animationEvets_OnStartedBlendOut()
            {
                TryFinishAbility();
            }

        }
    }
}
