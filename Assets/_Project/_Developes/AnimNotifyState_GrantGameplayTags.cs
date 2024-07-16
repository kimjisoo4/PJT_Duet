using StudioScor.GameplayTagSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "StudioScor/Utilities/Animation/AnimNotifyStateEvent/new Grant Gameplay Tag", fileName = "AnimNotifyState_GrantGameplayTag_")]
    public class AnimNotifyState_GrantGameplayTags : AnimNotifyStateEvent
    {
        [Header(" [ Grant Gameplay Tags ] ")]
        [SerializeField] private FGameplayTags _grantTags;

        public override void Enter(AnimNotifyComponent animNotifyComponent)
        {
            if(animNotifyComponent.Owner.TryGetGameplayTagSystem(out IGameplayTagSystem gameplayTagSystem))
            {
                gameplayTagSystem.GrantGameplayTags(_grantTags);
            }
        }

        public override void Exit(AnimNotifyComponent animNotifyComponent)
        {
            if (animNotifyComponent.Owner.TryGetGameplayTagSystem(out IGameplayTagSystem gameplayTagSystem))
            {
                gameplayTagSystem.RemoveGameplayTags(_grantTags);
            }
        }
    }
}
