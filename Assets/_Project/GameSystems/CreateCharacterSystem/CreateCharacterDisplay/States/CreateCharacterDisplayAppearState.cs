using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Display/State/Create Character Display Appear State")]
    public class CreateCharacterDisplayAppearState : CreateCharacterDisplayState
    {
        [Header(" [ Appear State ] ")]
        [SerializeField] private GameObject _uiActor;
        [SerializeField] private CreateCharacterButton[] _createCharacterButtons;

        private int _remainActivateButtonCount = 0;

        private void Awake()
        {
            foreach (var createCharacterButton in _createCharacterButtons)
            {
                createCharacterButton.OnFInishedBlendIn += CreateCharacterButton_OnFInishedBlendIn;
            }    
        }
        private void OnDestroy()
        {
            if (!_createCharacterButtons.IsNullOrEmpty())
            {
                foreach (var createCharacterButton in _createCharacterButtons)
                {
                    if (createCharacterButton)
                        createCharacterButton.OnFInishedBlendIn -= CreateCharacterButton_OnFInishedBlendIn;
                }
            }
        }

        protected override void EnterState()
        {
            base.EnterState();

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);

            OnActivateButtons();
        }

        private void OnActivateButtons()
        {
            _remainActivateButtonCount = _createCharacterButtons.Length;

            foreach (var createCharacterButton in _createCharacterButtons)
            {
                createCharacterButton.Activate();
            }
        }

        private void CreateCharacterButton_OnFInishedBlendIn(CreateCharacterButton selectCharacterUI)
        {
            _remainActivateButtonCount--;

            if(_remainActivateButtonCount <= 0)
            {
                CreateCharacterDisplay.FinishedState(this);
            }
        }
    }
}
