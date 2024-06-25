using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using StudioScor.MovementSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Knockback Movement Ability", fileName = "GA_PawnAbiltiy_KnockbackMovement")]
    public class KnockbackMovementAbility : GASAbility
    {
        [Header(" [ Knockback Movement Ability ] ")]
        [SerializeField] private AnimationCurve _knockbackCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly KnockbackMovementAbility _ability;
            private readonly IMovementSystem _movementSystem;
            private readonly IKnockbackable _knockbackable;

            private readonly ReachValueToTime _reachValueToTime = new();
            private readonly Timer _timer = new();

            private Vector3 _direction;
            private float _distance;
            private float _duration;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as KnockbackMovementAbility;

                _movementSystem = gameObject.GetMovementSystem();
                _knockbackable = gameObject.GetComponent<IKnockbackable>();
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _knockbackable.OnTakeKnockback += _knockbackable_OnTakeKnockback;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                _knockbackable.OnTakeKnockback -= _knockbackable_OnTakeKnockback;
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                _timer.OnTimer(_duration);
                _reachValueToTime.OnMovement(_distance, _ability._knockbackCurve);
            }
            protected override void OnReTriggerAbility()
            {
                base.OnReTriggerAbility();

                _timer.CancelTimer();
                _reachValueToTime.EndMovement();

                _timer.OnTimer(_duration);
                _reachValueToTime.OnMovement(_distance, _ability._knockbackCurve);
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                _timer.FinisheTimer();
                _reachValueToTime.EndMovement();
            }
            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    _timer.UpdateTimer(deltaTime);

                    float normalizedTime = _timer.NormalizedTime;

                    _reachValueToTime.UpdateMovement(normalizedTime);

                    _movementSystem.MovePosition(_direction * _reachValueToTime.DeltaDistance);

                    if (!_timer.IsPlaying)
                        TryFinishAbility();
                }
            }
            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            private void _knockbackable_OnTakeKnockback(IKnockbackable knockbackable, Vector3 direction, float distance, float duration)
            {
                _direction = direction;
                _distance = distance;
                _duration = duration;

                TryActiveAbility();
            }
        }
    }
}
