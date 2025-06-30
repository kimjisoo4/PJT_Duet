using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using StudioScor.MovementSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.GameplayCueSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Knockback Movement Ability", fileName = "GA_PawnAbiltiy_KnockbackMovement")]
    public class KnockbackMovementAbility : GASAbility
    {
        [Header(" [ Knockback Movement Ability ] ")]
        [SerializeField] private AnimationCurve _knockbackCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _dustCue;

        [Header(" Guard ")]
        [SerializeField] private GameplayTag _guardStateTag;
        [SerializeField] private float _guardKnockbackStrength = 0.5f;

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

            private Cue _dustCue;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as KnockbackMovementAbility;

                _movementSystem = gameObject.GetMovementSystem();
                _knockbackable = gameObject.GetComponent<IKnockbackable>();
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _knockbackable.OnTakeKnockback += Knockbackable_OnTakeKnockback;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                _knockbackable.OnTakeKnockback -= Knockbackable_OnTakeKnockback;
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                OnKnockback();

                _dustCue = _ability._dustCue.PlayAttached(transform);
            }

            protected override void OnReTriggerAbility()
            {
                base.OnReTriggerAbility();

                OnKnockback();
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                _timer.FinishTimer();
                _reachValueToTime.EndMovement();

                if (_dustCue is not null)
                {
                    _dustCue.Detach();
                    _dustCue.Stop();

                    _dustCue = null;
                }
            }

            private void OnKnockback()
            {
                Log(nameof(OnKnockback));

                _timer.CancelTimer();
                _reachValueToTime.EndMovement();

                if (GameplayTagSystem.ContainOwnedTag(_ability._guardStateTag))
                {
                    _timer.OnTimer(_duration * _ability._guardKnockbackStrength);
                    _reachValueToTime.OnMovement(_distance * _ability._guardKnockbackStrength, _ability._knockbackCurve);
                }
                else
                {
                    _timer.OnTimer(_duration);
                    _reachValueToTime.OnMovement(_distance, _ability._knockbackCurve);
                }
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

            private void Knockbackable_OnTakeKnockback(IKnockbackable knockbackable, Vector3 direction, float distance, float duration)
            {
                _direction = direction;
                _distance = distance;
                _duration = duration;

                TryActiveAbility();
            }
        }
    }
}
