using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using System.Collections;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/State/new Dead Ability", fileName = "GA_PawnAbiltiy_Dead")]
    public class DeadAbility : GASAbility
    {
        [Header(" [ Dead Ability ] ")]
        [SerializeField] private Ability[] _grantAbilities;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            protected new readonly DeadAbility _ability;
            private readonly AnimationPlayer _animationPlayer;
            private readonly AnimationPlayer.Events _animationEvents;
            private readonly ICharacter _character;


            private readonly int ANIM_DEAD_ID = Animator.StringToHash("Dead");

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DeadAbility;
                _character = gameObject.GetComponent<ICharacter>();
                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                
                _animationEvents = new();
                _animationEvents.OnFailed += _animationPlayer_OnFailed;
                _animationEvents.OnFinished += _animationPlayer_OnFinished;
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _animationPlayer.Play(ANIM_DEAD_ID);
                _animationPlayer.AnimationEvents = _animationEvents;
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _animationPlayer.TryStopAnimation(ANIM_DEAD_ID);

                foreach (var ability in _ability._grantAbilities)
                {
                    AbilitySystem.RemoveAbility(ability);
                }
            }

            private void EndAnimation()
            {
                Log(nameof(EndAnimation));

                _character.OnDie();

                foreach (var ability in _ability._grantAbilities)
                {
                    AbilitySystem.TryGrantAbility(ability);
                }
            }


            private void _animationPlayer_OnFailed()
            {
                Debug.LogError("Failed");
            }

            private void _animationPlayer_OnFinished()
            {
                EndAnimation();
            }
        }
    }
}
