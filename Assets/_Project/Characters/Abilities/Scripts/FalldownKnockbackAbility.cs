using StudioScor.AbilitySystem;
using StudioScor.MovementSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Falldown Knockback Ability", fileName = "GA_PawnAbiltiy_FalldownKnockback")]
    public class FalldownKnockbackAbility : GASAbility
    {
        [Header(" [ Falldown Knockback Ability ] ")]
        [SerializeField] private float _knockbackPower = 1f;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            protected new readonly FalldownKnockbackAbility _ability;
            private readonly ICharacter _character;
            private readonly IAddForceable _addForceable;
            private readonly IMovementSystem _movementSystem;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as FalldownKnockbackAbility;
                _character = gameObject.GetComponent<ICharacter>();
                _addForceable = gameObject.GetComponent<IAddForceable>();
                _movementSystem = gameObject.GetMovementSystem();
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _character.OnCharacterColliderHit += _character_OnCharacterColliderHit;
            }

            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if (_character is not null)
                {
                    _character.OnCharacterColliderHit -= _character_OnCharacterColliderHit;
                }
            }

            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            private void DrawArrow(Vector3 direction, float power)
            {
                if (!UseDebug)
                    return;

                SUtility.Debug.DebugArrow(transform.position, direction * power, Color.red, 1f);
                SUtility.Debug.DebugArrow(transform.position, direction * -power, Color.blue, 1f);
            }

            private void _character_OnCharacterColliderHit(ICharacter character, ControllerColliderHit hit)
            {
                if (!IsPlaying)
                    return;

                if (_movementSystem.IsGrounded)
                    return;

                if (_movementSystem.PrevGravity > 0)
                    return;

                if (!hit.collider.TryGetActor(out IActor hitActor))
                    return;

                if (!hitActor.gameObject.TryGetComponent(out IAddForceable addForceable))
                    return;


                Vector3 direction = transform.HorizontalDirection(hit.point);
                float power = _ability._knockbackPower;

                addForceable.AddForce(direction * power);
                _addForceable.AddForce(direction * -power + Vector3.up);

                DrawArrow(direction, power);
            }
        }
    }
}
