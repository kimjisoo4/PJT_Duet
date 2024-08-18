using StudioScor.AbilitySystem;
using StudioScor.GameplayEffectSystem;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface ISpawnedActorToAbility
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public void SetOwner(GameObject newOwner, IAbilitySpec abilitySpec);
        public void SetApplyGameplayEffectToOther(GameplayEffect[] gameplayEffects);
    }
}
