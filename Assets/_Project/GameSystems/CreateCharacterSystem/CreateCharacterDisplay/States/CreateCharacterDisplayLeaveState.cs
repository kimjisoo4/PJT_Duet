using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Display/State/Create Character Display Leave State")]
    public class CreateCharacterDisplayLeaveState : CreateCharacterDisplayState
    {
        [Header(" [ Leave State ] ")]
        [SerializeField] private GameObject _uiActor;
        [SerializeField] private CreateCharacterButton[] _createCharacterButtons;

        private int _remainInactivateButtonCount;

        private void Awake()
        {
            foreach (var createCharacterButton in _createCharacterButtons)
            {
                createCharacterButton.OnInactivated += CreateCharacterButton_OnInactivated;
            }
        }
        private void OnDestroy()
        {
            if(!_createCharacterButtons.IsNullOrEmpty())
            {
                foreach (var createCharacterButton in _createCharacterButtons)
                {
                    if(createCharacterButton)
                        createCharacterButton.OnInactivated -= CreateCharacterButton_OnInactivated;
                }
            }
        }
        protected override void EnterState()
        {
            base.EnterState();

            OnInactivateButtons();
        }

        private void OnInactivateButtons()
        {
            _remainInactivateButtonCount = _createCharacterButtons.Length;

            foreach (var createCharacterButton in _createCharacterButtons)
            {
                createCharacterButton.Inactivate();
            }
        }

        private void CreateCharacterButton_OnInactivated(CreateCharacterButton selectCharacterUI)
        {
            _remainInactivateButtonCount--;

            if (_remainInactivateButtonCount <= 0)
            {
                CreateCharacterDisplay.FinishedState(this);
            }
        }
    }
}
