using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class SelectCharacterUIState : BaseStateMono
    {
        [Header(" [ Select Character UI State ] ")]
        [SerializeField] private SelectCharacterComponent _selectCharacterComponent;
        protected SelectCharacterComponent SelectCharacterUI => _selectCharacterComponent;
    }
}
