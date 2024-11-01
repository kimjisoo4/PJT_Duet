using PF.PJT.Duet.Pawn;
using UnityEngine;
using UnityEngine.UI;

namespace PF.PJT.Duet
{
    public class CharacterStateIconUI : CharacterStateUIModifier
    {
        [Header(" [ Character Icon UI ] ")]
        [SerializeField] private Image _image;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!_image)
            {
                _image = GetComponentInChildren<Image>();
            }
#endif
        }
        public override void UpdateData(ICharacter character)
        {
            if(character is not null)
            {
                if (character.CharacterInformationData)
                {
                    _image.sprite = character.CharacterInformationData.Icon;
                    _image.color = Color.white;
                }
                else
                {
                    _image.sprite = null;
                    _image.color = Color.white;
                }
            }
            else
            {
                _image.sprite = null;
                _image.color = Color.clear;
            }
        }
    }
}
