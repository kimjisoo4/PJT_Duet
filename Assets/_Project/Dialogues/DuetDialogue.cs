using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using UnityEngine;
using UnityEngine.Localization;

namespace PF.PJT.Duet
{

    [CreateAssetMenu(menuName = "Project/Duet/Dialogue/new Duet Dialogue", fileName = "Dialogue_")]
    public class DuetDialogue : Dialogue, IDisplayName, IDisplayDescriptionImage, IHasGameEventDialogue
    {
        [Header(" [ Dialogue With Talker ] ")]
        [SerializeField] private LocalizedString _talkerName;
        [SerializeField] private Sprite _descriptImage;

        [Header(" Event ")]
        [SerializeField] private GameEvent _onStartedDialogue;
        [SerializeField] private GameEvent _onFinishedDialogue;

        public string Name => _talkerName.GetLocalizedString();
        public Sprite DescriptionImage => _descriptImage;

        public void OnStart()
        {
            if (_onStartedDialogue)
                _onStartedDialogue.Invoke();
        }
        public void OnFinish()
        {
            if (_onFinishedDialogue)
                _onFinishedDialogue.Invoke();
        }
    }
}
