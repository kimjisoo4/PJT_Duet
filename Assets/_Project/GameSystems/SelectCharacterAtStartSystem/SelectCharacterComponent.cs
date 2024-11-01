using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class SelectCharacterComponent : BaseMonoBehaviour
    {
        public delegate void SelectCharacterState(SelectCharacterComponent selectCharacterUI);

        [Header(" [ Character Data UI ] ")]
        [SerializeField] private GameObject _submitActor;
        [SerializeField] private DataComponent _dataContainer;

        private CharacterData _characterData;
        private ISubmitEventListener _submit;

        public CharacterData CharacterData => _characterData;

        public event SelectCharacterState OnSubmited;


        private void Awake()
        {
            _submit = _submitActor.GetComponent<ISubmitEventListener>();

            _submit.OnSubmited += _submit_OnSubmited;
        }
        private void OnDestroy()
        {
            if(_submit is not null)
            {
                _submit.OnSubmited -= _submit_OnSubmited;
                _submit = null;
            }
        }

        public void SetCharacterData(CharacterData characterData)
        {
            Log($"{nameof(SetCharacterData)} - {(characterData is null ? "null" : characterData.ID)}");

            _characterData = characterData;

            _dataContainer.UpdateData(_characterData);
        }

        private void _submit_OnSubmited(ISubmitEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (_characterData is null)
                return;

            OnSubmited?.Invoke(this);
        }
    }
}
