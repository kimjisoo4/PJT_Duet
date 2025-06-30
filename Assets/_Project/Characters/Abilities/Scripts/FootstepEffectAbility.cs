using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.MovementSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Effect/new Footstep Effect Ability", fileName = "GA_PawnAbiltiy_Footstep")]
    public class FootstepEffectAbility : GASAbility
    {
        [Header(" [ Footstep Effect Ability ] ")]
        [SerializeField] private GameplayTag _footstepTriggerTag;
        [SerializeField] private FGameplayCue _footstepCue;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            protected new readonly FootstepEffectAbility _ability;
            private readonly IMovementSystem _movementSystem;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as FootstepEffectAbility;
                _movementSystem = gameObject.GetMovementSystem();
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                GameplayTagSystem.OnTriggeredTag += GameplayTagSystem_OnTriggeredTag;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                GameplayTagSystem.OnTriggeredTag -= GameplayTagSystem_OnTriggeredTag;
            }

            private void GameplayTagSystem_OnTriggeredTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag, object data = null)
            {
                if (!IsPlaying)
                    return;

                if (!_movementSystem.IsGrounded)
                    return;

                if (_ability._footstepTriggerTag != gameplayTag)
                    return;

                if(data is OnFootstepData footstepData)
                {
                    Log(" Play Footstep Cue ");

                    Vector3 position = footstepData.Position;
                    Vector3 rotation = transform.eulerAngles;
                    Vector3 scale = _ability._footstepCue.Scale;
                    float volume = _ability._footstepCue.Volume;

                    _ability._footstepCue.Cue.Play(position, rotation, scale, volume);
                }
            }
        }
    }
}
