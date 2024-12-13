using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet
{
    public class DialogueSelectButton : BaseMonoBehaviour
    {
        [Header(" [ Dialogue Select Button ] ")]
        [SerializeField] private GameObject _buttonActor;
        [SerializeField] private GameObject _eventListenerActor;
        [SerializeField] private TMP_Text _dialogueText;

        private bool _wasInit = false;

        private IDialogue _dialogue;
        private ISubmitEventListener _submitEvent;

        public IDialogue Dialogue => _dialogue;
        public event UnityAction<DialogueSelectButton> OnSubmited;


        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            _submitEvent = _eventListenerActor.GetComponent<SubmitEventListener>();
            _submitEvent.OnSubmited += _submitEvent_OnSubmited;
        }

        public void SetSelectableDialogue(IDialogue dialogue)
        {
            _dialogue = dialogue;
            _dialogueText.text = _dialogue.DialogueText;
        }
        public void Activate()
        {
            _buttonActor.SetActive(true);
        }
        public void Inactivate()
        {
            _buttonActor.SetActive(false);
        }
        private void _submitEvent_OnSubmited(ISubmitEventListener submitEventListener, BaseEventData eventData)
        {
            OnSubmited?.Invoke(this);
        }
    }
}
