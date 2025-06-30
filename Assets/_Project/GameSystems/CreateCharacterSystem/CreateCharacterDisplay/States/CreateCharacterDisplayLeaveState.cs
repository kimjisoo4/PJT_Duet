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

        private int _remainDeactivateButtonCount;

        private void Awake()
        {
            foreach (var createCharacterButton in _createCharacterButtons)
            {
                createCharacterButton.OnDeactivated += CreateCharacterButton_OnDeactivated;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!_createCharacterButtons.IsNullOrEmpty())
            {
                foreach (var createCharacterButton in _createCharacterButtons)
                {
                    if(createCharacterButton)
                        createCharacterButton.OnDeactivated -= CreateCharacterButton_OnDeactivated;
                }
            }
        }
        protected override void EnterState()
        {
            base.EnterState();

            OnDeactivateButtons();
        }

        private void OnDeactivateButtons()
        {
            _remainDeactivateButtonCount = _createCharacterButtons.Length;

            foreach (var createCharacterButton in _createCharacterButtons)
            {
                createCharacterButton.Deactivate();
            }
        }

        private void CreateCharacterButton_OnDeactivated(CreateCharacterButton selectCharacterUI)
        {
            _remainDeactivateButtonCount--;

            if (_remainDeactivateButtonCount <= 0)
            {
                CreateCharacterDisplay.FinishedState(this);
            }
        }
    }
}
