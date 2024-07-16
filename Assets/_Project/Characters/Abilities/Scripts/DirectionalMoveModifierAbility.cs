using UnityEngine;
using StudioScor.MovementSystem;
using StudioScor.AbilitySystem;
using StudioScor.StatSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Directional Move Modifier Ability", fileName = "GA_PawnAbiltiy_DirectionalMoveModifier")]
    public class DirectionalMoveModifierAbility : GASAbility
    {
        [Header(" [ Directional Move Modifier Ability ] ")]
        [SerializeField] private StatTag _moveSpeedTag;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            protected new readonly DirectionalMoveModifierAbility _ability;
            private readonly IMovementSystem _movementSystem;
            private readonly DirectionalAccelerationModifier _accelerateMove;
            private readonly Stat _moveSpeedStat;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DirectionalMoveModifierAbility;

                _movementSystem = gameObject.GetMovementSystem();


                var statSystem = gameObject.GetStatSystem();

                _moveSpeedStat = statSystem.GetOrCreateValue(_ability._moveSpeedTag);

                var movementModuleSystem = gameObject.GetMovementModuleSystem();
                
                _accelerateMove = new DirectionalAccelerationModifier(_movementSystem, movementModuleSystem);

            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _accelerateMove.SetMaxSpeed(_moveSpeedStat.Value);
                _moveSpeedStat.OnChangedValue += _moveSpeedStat_OnChangedValue;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if(_moveSpeedStat is not null)
                {
                    _moveSpeedStat.OnChangedValue -= _moveSpeedStat_OnChangedValue;
                }
            }

            private void _moveSpeedStat_OnChangedValue(Stat stat, float currentValue, float prevValue)
            {
                _accelerateMove.SetMaxSpeed(currentValue);
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _accelerateMove.EnableModifier();

                if (_movementSystem.MoveStrength > 0f)
                {
                    _accelerateMove.SetCurrentSpeed(_movementSystem.PrevSpeed);
                }
                else
                {
                    _accelerateMove.SetCurrentSpeed(0f);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _accelerateMove.DisableModifier();
            }
        }
    }
}
