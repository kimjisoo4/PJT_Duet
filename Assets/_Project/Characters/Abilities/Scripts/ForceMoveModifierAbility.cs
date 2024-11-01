using UnityEngine;
using StudioScor.MovementSystem;
using StudioScor.AbilitySystem;
using StudioScor.StatSystem;
using StudioScor.Utilities;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Force Move Modifier Ability", fileName = "GA_PawnAbiltiy_ForceMoveModifier")]
    public class ForceMoveModifierAbility : GASAbility
    {
        [Header(" [ Force Move Modifier Ability ] ")]
        [SerializeField] private StatTag _dragTag;
        [SerializeField] private StatTag _massTag;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            protected new readonly ForceMoveModifierAbility _ability;
            private readonly IMovementSystem _movementSystem;
            private readonly IAddForceable _addforceable;
            private readonly ForceModifier _forceMove;
            private readonly Stat _dragStat;
            private readonly Stat _massStat;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as ForceMoveModifierAbility;

                _movementSystem = gameObject.GetMovementSystem();
                _addforceable = gameObject.GetComponent<IAddForceable>();

                var statSystem = gameObject.GetStatSystem();

                _dragStat = statSystem.GetOrCreateValue(_ability._dragTag, 1f);
                _massStat = statSystem.GetOrCreateValue(_ability._massTag, 1f);

                var movementModuleSystem = gameObject.GetMovementModuleSystem();

                _forceMove = new ForceModifier(_movementSystem, movementModuleSystem);

            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _forceMove.SetDrag(_dragStat.Value);
                _forceMove.SetMass(_massStat.Value);

                _forceMove.OnEndedForce += _forceMove_OnEndedForce;

                _dragStat.OnChangedValue += _dargStat_OnChangedValue;
                _massStat.OnChangedValue += _massStat_OnChangedValue;

                _addforceable.OnAddForce += _addforceable_OnAddForce;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if(_forceMove is not null)
                {
                    _forceMove.OnEndedForce -= _forceMove_OnEndedForce;
                }
                if (_dragStat is not null)
                {
                    _dragStat.OnChangedValue -= _dargStat_OnChangedValue;
                }

                if (_massStat is not null)
                {
                    _massStat.OnChangedValue -= _massStat_OnChangedValue;
                }

                if(_addforceable is not null)
                {
                    _addforceable.OnAddForce -= _addforceable_OnAddForce;
                }
            }

            private void _massStat_OnChangedValue(Stat stat, float currentValue, float prevValue)
            {
                _forceMove.SetMass(currentValue);
            }

            private void _dargStat_OnChangedValue(Stat stat, float currentValue, float prevValue)
            {
                _forceMove.SetDrag(currentValue);
            }
            private void _addforceable_OnAddForce(IAddForceable addForceable, Vector3 force)
            {
                if (TryActiveAbility())
                {
                    _forceMove.AddForce(force);
                }
            }
            private void _forceMove_OnEndedForce(IMovementSystem movementSystem, IForceModifier forceModifier)
            {
                TryFinishAbility();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _forceMove.EnableModifier();
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _forceMove.DisableModifier();
            }
        }
    }
}
