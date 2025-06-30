using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Button/State/Create Character Button State")]
    public class CreateCharacterButtonState : BaseStateMono
    {
        [Header(" [ Create Character Button State ] ")]
        [SerializeField] private CreateCharacterButton _createCharacterButtonComponent;
        protected CreateCharacterButton CreateCharacterButton => _createCharacterButtonComponent;

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            base.OnValidate();

            if (!_createCharacterButtonComponent)
            {
                _createCharacterButtonComponent = GetComponentInParent<CreateCharacterButton>();
            }
#endif
        }
    }
}
