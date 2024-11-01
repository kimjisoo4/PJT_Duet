using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using StudioScor.GameplayTagSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Effect/new Apply Stiffen Ability", fileName = "GA_PawnAbiltiy_ApplyStiffen")]
    public class ApplyStiffenAbility : GASAbility
    {
        [Header(" [ Apply Stiffen Ability ] ")]
        [SerializeField] private GameplayTagSO _triggerTag;
        [SerializeField][Range(0f, 1f)] private float _animationOffset = 0.2f;
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec
        {
            protected new readonly ApplyStiffenAbility _ability;
            private readonly AnimationPlayer _animationPlayer;
            private const string ANIM_STIFFEN = "Stiffen";
            private readonly int ANIM_STIFFEN_ID = Animator.StringToHash(ANIM_STIFFEN);
            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as ApplyStiffenAbility;
                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
            }
            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                GameplayTagSystem.OnAddedOwnedTag += GameplayTagSystem_OnAddedOwnedTag;
                GameplayTagSystem.OnRemovedOwnedTag += GameplayTagSystem_OnRemovedOwnedTag;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                GameplayTagSystem.OnAddedOwnedTag -= GameplayTagSystem_OnAddedOwnedTag;
                GameplayTagSystem.OnRemovedOwnedTag -= GameplayTagSystem_OnRemovedOwnedTag;
            }

            public override bool CanReTriggerAbility()
            {
                if (!base.CanReTriggerAbility())
                    return false;

                if (!CheckGameplayTags())
                    return false;

                return true;
            }


            private void GameplayTagSystem_OnAddedOwnedTag(IGameplayTagSystem gameplayTagSystem, IGameplayTag gameplayTag)
            {
                if(_ability._triggerTag == gameplayTag)
                    TryActiveAbility();
            }
            private void GameplayTagSystem_OnRemovedOwnedTag(IGameplayTagSystem gameplayTagSystem, IGameplayTag gameplayTag)
            {
                if (_ability._triggerTag == gameplayTag)
                    TryFinishAbility();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _animationPlayer.Play(ANIM_STIFFEN_ID, 0f, _ability._animationOffset);
            }

            protected override void OnReTriggerAbility()
            {
                base.OnReTriggerAbility();

                _animationPlayer.Play(ANIM_STIFFEN_ID, 0f, _ability._animationOffset);
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _animationPlayer.TryStopAnimation(ANIM_STIFFEN_ID);
            }
        }
    }
}
