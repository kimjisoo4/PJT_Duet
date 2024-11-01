using PF.PJT.Duet.Pawn;
using TMPro;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class CharacterStateNameUI : CharacterStateUIModifier
    {
        [Header(" [ Character Name UI ] ")]
        [SerializeField] private TMP_Text _name;

        private const string NAME_UNKNOWN = "Unknown";
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!_name)
            {
                _name = GetComponentInChildren<TMP_Text>();
            }
#endif
        }

        public override void UpdateData(ICharacter character)
        {
            if (character is not null)
            {
                if(character.CharacterInformationData)
                {
                    _name.text = character.CharacterInformationData.Name;
                }
                else
                {
                    _name.text = NAME_UNKNOWN;
                }
                
            }
            else
            {
                _name.text = "";
            }
        }
    }
}
