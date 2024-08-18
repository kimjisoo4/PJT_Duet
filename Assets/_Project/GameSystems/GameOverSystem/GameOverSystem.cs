using StudioScor.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet
{
    public class GameOverSystem : BaseMonoBehaviour
    {
        [Header(" [ GameOver System ] ")]
        [SerializeField] private GameObject _gameoverActor;
        [SerializeField] private GameEvent _gameoverEvent;
        [SerializeField] private GameObjectListVariable _activeUIVariable;

        [Header(" Animations ")]
        [SerializeField] private CanvasGroup _titleGroup;
        [SerializeField] private float _titleFadeTime = 0.5f;

        [Space(5f)]
        [SerializeField] private CanvasGroup _buttonGroup;
        [SerializeField] private float _buttonDelayTime = 0.5f;
        [SerializeField] private float _buttonFadeTime = 0.5f;

        [Header(" Restart Scene ")]
        [SerializeField] private GameObject _restartButton;
        [SerializeField] private SceneLoader _restartScene;

        [Header(" Title Scene ")]
        [SerializeField] private GameObject _titleButton;
        [SerializeField] private SceneLoader _titleScene;


        private ISubmitEventListener _restartSubmitEvent;
        private ISubmitEventListener _titleSubmitEvent;

        private void Awake()
        {
            _restartSubmitEvent = _restartButton.GetComponent<ISubmitEventListener>();
            _titleSubmitEvent = _titleButton.GetComponent<ISubmitEventListener>();

            _gameoverEvent.OnTriggerEvent += _gameoverEvent_OnTriggerEvent;
            _restartSubmitEvent.OnSubmited += _restartSubmitEvent_OnSubmited;
            _titleSubmitEvent.OnSubmited += _titleSubmitEvent_OnSubmited;

            if (_gameoverActor.activeSelf)
                _gameoverActor.SetActive(false);

            _titleGroup.alpha = 0f;

            _buttonGroup.alpha = 0f;
            _buttonGroup.interactable = false;
        }
        private void OnDestroy()
        {
            if (_gameoverEvent)
            {
                _gameoverEvent.OnTriggerEvent -= _gameoverEvent_OnTriggerEvent;
            }

            if(_restartSubmitEvent is not null)
            {
                _restartSubmitEvent.OnSubmited -= _restartSubmitEvent_OnSubmited;
            }

            if (_titleSubmitEvent is not null)
            {
                _titleSubmitEvent.OnSubmited -= _titleSubmitEvent_OnSubmited;
            }
        }

        private void OnSubmitedRestart()
        {
            Log($"{nameof(OnSubmitedRestart)}");

            _restartScene.LoadScene();

            EndGameoverUI();
        }

        private void OnSubmitedTitle()
        {
            Log($"{nameof(OnSubmitedTitle)}");

            _titleScene.LoadScene();

            EndGameoverUI();
        }

        [ContextMenu(nameof(OnGameoverUI))]
        private void OnGameoverUI()
        {
            _gameoverActor.SetActive(true);
            _activeUIVariable.Add(gameObject);

            StartCoroutine(UpdateGameoverUI());
        }
        private void EndGameoverUI()
        {
            _activeUIVariable.Remove(gameObject);
        }
        
        private IEnumerator UpdateGameoverUI()
        {
            var timer = new Timer();

            timer.OnTimer(_titleFadeTime);

            while (!timer.IsFinished)
            {
                yield return null;

                timer.UpdateTimer(Time.deltaTime);
                _titleGroup.alpha = timer.NormalizedTime;
            }

            _titleGroup.alpha = 1f;

            yield return new WaitForSeconds(_buttonDelayTime);

            timer.OnTimer(_buttonFadeTime);

            while (!timer.IsFinished)
            {
                yield return null;

                timer.UpdateTimer(Time.deltaTime);
                _buttonGroup.alpha = timer.NormalizedTime;
            }

            _buttonGroup.alpha = 1f;
            _buttonGroup.interactable = true;

            EventSystem.current.SetSelectedGameObject(_restartButton);

            timer = null;

            yield break;
        }
        private void _gameoverEvent_OnTriggerEvent()
        {
            OnGameoverUI();
        }
        private void _restartSubmitEvent_OnSubmited(ISubmitEventListener submitEventListener, BaseEventData obj)
        {
            OnSubmitedRestart();
        }
        private void _titleSubmitEvent_OnSubmited(ISubmitEventListener submitEventListener, BaseEventData obj)
        {
            OnSubmitedTitle();
        }
    }
}
