using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;
using StudioScor.MovementSystem;
using StudioScor.AbilitySystem;
using StudioScor.RotationSystem;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Dash Skill", fileName = "GA_Skill_Dash")]
    public class DashSkill : GASAbility
    {
        [Header(" [ Dash Skill ] ")]
        [Header(" Animation")]
        [SerializeField] private string _animationName = "Slide";
        [SerializeField] private string _motionTime = "motionTime";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;
        [SerializeField][Range(0f, 1f)] private float _endTime = 0.8f;

        [Header(" Movement ")]
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _distance = 5f;
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly DashSkill _ability;
            private readonly IPawnSystem _pawnSystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly AnimationPlayer _animationPlayer;

            private readonly ReachValueToTime _reachValueToTime = new();
            private readonly Timer _timer = new();
            private readonly int _animationID;
            private readonly int _motionTime;

            private Vector3 _dashDirection;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DashSkill;

                _pawnSystem = gameObject.GetPawnSystem();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);

                _animationID = Animator.StringToHash(_ability._animationName);
                _motionTime = Animator.StringToHash(_ability._motionTime);
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                _animationPlayer.Play(_animationID, _ability._fadeInTime);

                _dashDirection = _pawnSystem.MoveDirection;
                
                if(_dashDirection.SafeEquals(Vector3.zero))
                {
                    _dashDirection = transform.forward;
                }

                Quaternion rotation = Quaternion.LookRotation(_dashDirection, transform.up);
                _rotationSystem.SetRotation(rotation);

                _timer.OnTimer(_ability._duration);
                _reachValueToTime.OnMovement(_ability._distance, _ability._moveCurve);

            }

            public override void CancelAbility()
            {
                base.CancelAbility();

                _animationPlayer.TryStopAnimation(_animationID);
                _timer.EndTimer();
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _reachValueToTime.EndMovement();
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    _timer.UpdateTimer(deltaTime);

                    float normalizedTime = _timer.NormalizedTime;

                    _animationPlayer.Animator.SetFloat(_motionTime, normalizedTime);

                    _reachValueToTime.UpdateMovement(normalizedTime);
                    _movementSystem.MovePosition(_dashDirection * _reachValueToTime.DeltaDistance);

                    if(normalizedTime >= _ability._endTime)
                    {
                        TryFinishAbility();
                    }
                }
                else
                {
                    if(_timer.IsPlaying)
                    {
                        if(_animationPlayer.IsPlayingHash(_animationID))
                        {
                            _timer.UpdateTimer(deltaTime);

                            float normalizedTime = _timer.NormalizedTime;

                            _animationPlayer.Animator.SetFloat(_motionTime, normalizedTime);
                        }
                        else
                        {
                            _timer.EndTimer();
                        }
                    }
                }
            }

        }
    }
}
