using StudioScor.AbilitySystem;
using StudioScor.GameplayTagSystem;
using System.Collections.Generic;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface ITakeDamageAbility
    {
        public void OnHit(ISpawnedActorByAbility spawnedActor, RaycastHit[] hits, int hitCount);
    }
    public interface IAreaAbility
    {
        public void EnterArea(ISpawnedActorByAbility spawnedActor, GameObject enterActor);
        public void ExitArea(ISpawnedActorByAbility spawnedActor, GameObject exitActor);
    }

    public interface ISpawnedActorByAbility
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }

        public IGameplayTagSystem GameplayTagSystem { get; }

        public void Activate(GameObject newOwner, IAbilitySpec spawnAbility, IEnumerable<GameplayTag> ownedTags = null);
        public void Inactivate();
        public void Play();
    }
}
