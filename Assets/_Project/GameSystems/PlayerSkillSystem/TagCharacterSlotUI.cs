using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace PF.PJT.Duet.System
{
    public class TagCharacterSlotUI : BaseMonoBehaviour
    {
        [Header(" [ Tag Character Slot UI ] ")]
        [SerializeField] private Image _icon;
        public void SetCharacter(ICharacter character)
        {
            if(character is not null)
            {
                var data = character.CharacterInformationData;

                _icon.sprite = data.Icon;
                _icon.color = Color.white;
            }
            else
            {
                _icon.sprite = null;
                _icon.color = Color.clear;
            }
        }
    }
}
