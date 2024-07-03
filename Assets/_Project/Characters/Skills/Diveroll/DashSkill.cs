using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;
using StudioScor.MovementSystem;
using StudioScor.AbilitySystem;
using StudioScor.RotationSystem;
using PF.PJT.Duet.Pawn.Effect;
using StudioScor.GameplayEffectSystem;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Dash Skill", fileName = "GA_Skill_Dash")]
    public class DashSkill : GASAbility, ISkill
    {
        [Header(" [ Dash Skill ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private bool _isSkill;
        [SerializeField] private ESkillType _skillType;

        [Header(" Animation")]
        [SerializeField] private string _animationName = "Slide";
        [SerializeField] private string _motionTime = "motionTime";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;
        [SerializeField][Range(0f, 1f)] private float _endTime = 0.8f;

        [Header(" Movement ")]
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _distance = 5f;
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.Linear(0, 0, 1, 1);

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
            protected new readonly DashSkill _ability;
            private readonly IPawnSystem _pawnSystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
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
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);

                _animationID = Animator.StringToHash(_ability._animationName);
                _motionTime = Animator.StringToHash(_ability._motionTime);
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

                _animationPlayer.Play(_animationID, _ability._fadeInTime);

                _dashDirection = _pawnSystem.MoveDirection;
                
                if(_dashDirection.SafeEquals(Vector3.zero))
                {
                    _dashDirection = transform.forward;
                }

                Quaternion rotation = Quaternion.LookRotation(_dashDirection, transform.up);
                _rotationSystem.SetRotation(rotation);

                if(_timer.IsPlaying)
                {
                    _timer.EndTimer();
                }

                _timer.OnTimer(_ability._duration);
                _reachValueToTime.OnMovement(_ability._distance, _ability._moveCurve);

            }

            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

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
