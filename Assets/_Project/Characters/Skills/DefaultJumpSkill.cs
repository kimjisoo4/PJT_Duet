using StudioScor.AbilitySystem;
using StudioScor.MovementSystem;
using StudioScor.StatSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Default Jump Skill", fileName = "GA_Skill_")]
    public class DefaultJumpSkill : GASAbility
    {
        [Header(" [ Default Jump Skill ] ")]
        [SerializeField] private float _jumpStrangth = 10f;
        [SerializeField] private float _falldownSpeed = 10f;
        [SerializeField] private StatTag _gravityStat;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec
        {
            protected new readonly DefaultJumpSkill _ability;
            private readonly IAddForceable _addforceable;
            private readonly IMovementSystem _movementSystem;
            private readonly IGroundChecker _groundChecker;
            private readonly IStatSystem _statSystem;

            private readonly Stat _gravityStat;
            private readonly StatModifier _gravityModifier;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DefaultJumpSkill;

                _addforceable = gameObject.GetComponent<IAddForceable>();
                _movementSystem = gameObject.GetMovementSystem();
                _groundChecker = gameObject.GetGroundChecker();
                _statSystem = gameObject.GetStatSystem();

                _gravityStat = _statSystem.GetOrCreateValue(_ability._gravityStat);
                _gravityModifier = new();
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _movementSystem.OnLanded += _movementSystem_OnLanded;
            }

            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if(_movementSystem is not null)
                {
                    _movementSystem.OnLanded -= _movementSystem_OnLanded;
                }
            }

            private void _movementSystem_OnLanded(IMovementSystem movementSystem)
            {
                TryFinishAbility();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _addforceable.AddForce(Vector3.up * _ability._jumpStrangth);
                _movementSystem.ForceUnGrounded();
                _groundChecker.SetGrounded(false);

                _gravityModifier.Setup(_ability._falldownSpeed, EStatModifierType.Add);
                _gravityStat.AddModifier(_gravityModifier);
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                _gravityStat.RemoveModifier(_gravityModifier);
            }
        }
    }
}
