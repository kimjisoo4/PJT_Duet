using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet
{
    public class StageClearSystem : BaseMonoBehaviour
    {
        [Header(" [ Stage Clear System ] ")]
        [SerializeField] private GameEvent _onStageClear;
        [SerializeField] private GameObject _stageClearUIActor;
        [SerializeField] private GameObjectListVariable _activeUIInputVariable;

        [Header(" UI ")]
        [SerializeField] private CanvasGroup _stageClearCanvasGroup;

        [Header(" Restart Scene ")]
        [SerializeField] private GameObject _restartButtonActor;
        [SerializeField] private SceneLoader _restartScene;

        [Header(" Title Scene ")]
        [SerializeField] private GameObject _titleButtonActor;
        [SerializeField] private SceneLoader _titleScene;

        private ISubmitEventListener _restartSubmit;
        private ISubmitEventListener _titleSubmit;

        private void Awake()
        {
            _onStageClear.OnTriggerEvent += _onStageClear_OnTriggerEvent;

            _restartSubmit = _restartButtonActor.GetComponent<ISubmitEventListener>();
            _titleSubmit = _titleButtonActor.GetComponent<ISubmitEventListener>();

            _restartSubmit.OnSubmited += _restartSubmit_OnSubmited;
            _titleSubmit.OnSubmited += _titleSubmit_OnSubmited;

            _stageClearUIActor.SetActive(false);
            _stageClearCanvasGroup.interactable = false;
        }

        private void OnDestroy()
        {
            if(_onStageClear)
            {
                _onStageClear.OnTriggerEvent -= _onStageClear_OnTriggerEvent;
            }

            if(_restartSubmit is not null)
            {
                _restartSubmit.OnSubmited -= _restartSubmit_OnSubmited;
                _restartSubmit = null;
            }

            if(_titleSubmit is not null)
            {
                _titleSubmit.OnSubmited -= _titleSubmit_OnSubmited;
                _titleSubmit = null;
            }

            if(_activeUIInputVariable)
            {
                _activeUIInputVariable.Remove(gameObject);
            }
        }

        [ContextMenu(nameof(OnStageClear))]
        private void OnStageClear()
        {
            _stageClearUIActor.SetActive(true);

            _activeUIInputVariable.Add(gameObject);
            _stageClearCanvasGroup.interactable = true;

            EventSystem.current.SetSelectedGameObject(_restartButtonActor);
        }

        private void OnTitleScene()
        {
            _stageClearCanvasGroup.interactable = false;

            _titleScene.LoadScene();
        }
        private void OnRestartScene()
        {
            _stageClearCanvasGroup.interactable = false;

            _restartScene.LoadScene();
        }

        private void _onStageClear_OnTriggerEvent()
        {
            OnStageClear();
        }
        private void _titleSubmit_OnSubmited(ISubmitEventListener submitEventListener, BaseEventData eventData)
        {
            OnTitleScene();
        }
        private void _restartSubmit_OnSubmited(ISubmitEventListener submitEventListener, BaseEventData eventData)
        {
            OnRestartScene();
        }
    }
}
