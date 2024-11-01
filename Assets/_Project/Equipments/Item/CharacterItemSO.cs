using PF.PJT.Duet.Controller;
using PF.PJT.Duet.Pawn;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/EquipmentItem/new Character Item", fileName = "Item_Character_")]
    public class CharacterItemSO : ItemSO, IEquipment, IItem, IDisplayIcon, IDisplayName, IDisplayDescription
    {
        [Header(" [ Character Item SO ] ")]
        [SerializeField] private CharacterInformationData _characterData;

        public Sprite Icon => _characterData.Icon;
        public string Name => _characterData.Name;
        public string Description => _characterData.Description;

        public Object Context => this;
        bool IEquipment.UseDebug => UseDebug;

        public override bool CanUseItem(GameObject actor)
        {
            return base.CanUseItem(actor) && actor.TryGetComponent(out IPlayerController _);
        }
        public override void UseItem(GameObject actor)
        {
            if (actor.TryGetComponent(out IPlayerController playerController))
            {
                var newCharacter = Instantiate(_characterData.Actor);

                var character = newCharacter.GetComponent<ICharacter>();

                var currentCharacter = playerController.CurrentCharacter;

                playerController.AddCharacter(character);
/*
                playerController.SetCurrentCharacter(character, false);

                playerController.RemoveCharacter(currentCharacter);
*/
            }
        }
    }
}
