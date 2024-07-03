using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn
{
    [CreateAssetMenu(menuName = "Project/Duet/Data/new Character Information Data", fileName = "Data_CharacterInformation_")]
    public class CharacterInformationData : BaseScriptableObject
    {
        [Header(" [ Character Information Data ] ")]
        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;

        public string Name => _name;
        public Sprite Icon => _icon;
    }
}
