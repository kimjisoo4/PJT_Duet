using PF.PJT.Duet.UISystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    public class StageFlowSystem : BaseGameFlowSystem
    {
        [Header(" [ Stage Flow System Controller ] ")]
        [SerializeField] private StageMenuUIFlowController _stageMenuController;
        [SerializeField] private PlayerStatusSystem _playerStatusSystem;
        [SerializeField] private ResultController _resultController;

        [Header(" States ")]
        [SerializeField] private GameFlowState _idleState;
        [SerializeField] private GameFlowState _waitForSceneLoadedState;
        [SerializeField] private GameFlowState _waitForFadeInState;
        [SerializeField] private GameFlowState _createPlayerCharacterState;
        [SerializeField] private GameFlowState _ingameMenuState;
        [SerializeField] private GameFlowState _stageRewordState;

        [Header("ㄴBattle")]
        [SerializeField] private GameFlowState _battleState;
        [SerializeField] private GameFlowState _bossBattleState;

        [Header("ㄴResult")]
        [SerializeField] private GameFlowState _stageClearState;
        [SerializeField] private GameFlowState _gameoverState;

        [Header("ㄴScene Load ")]
        [SerializeField] private GameFlowState _reloadStageState;
        [SerializeField] private GameFlowState _loadMainmnuState;

        [Header(" Events ")]
        [Header(" Listener ")]
        [SerializeField] private GameEvent _onAllPlayerCharactersDead;
        [SerializeField] private GameEvent _onRoomStarted;
        [SerializeField] private GameEvent _onBossRoomStarted;
        [SerializeField] private GameEvent _onMenuInputPressed;

        private GameFlowState _prevState;


        private void Awake()
        {
            _onRoomStarted.OnTriggerEvent += OnRoomStarted_OnTriggerEvent;
            _onBossRoomStarted.OnTriggerEvent += OnBossRoomStarted_OnTriggerEvent;
            _onMenuInputPressed.OnTriggerEvent += OnMenuInputPressed_OnTriggerEvent;
            _onAllPlayerCharactersDead.OnTriggerEvent += OnAllPlayerCharactersDead_OnTriggerEvent;

            _stageMenuController.OnContinue += StageMenuController_OnContinue;
            _stageMenuController.OnStageExit += StageMenuController_OnStageExit;

            _resultController.OnRestart += ResultController_OnRestart;
            _resultController.OnExit += ResultController_OnExit;

            _waitForSceneLoadedState.OnStateComplated += WaitForLoadSceneState_OnStateComplated;
            _waitForFadeInState.OnStateComplated += WaitForFadeInState_OnStateComplated;
            _createPlayerCharacterState.OnStateComplated += CreatePlayerCharacterState_OnStateComplated;

            _battleState.OnStateComplated += BattleInRoomState_OnStateComplated;
            _bossBattleState.OnStateComplated += BossBattleState_OnStateComplated;
            _stageRewordState.OnStateComplated += StageRewordState_OnStateComplated;
        }

        private void OnDestroy()
        {
            _onRoomStarted.OnTriggerEvent -= OnRoomStarted_OnTriggerEvent;
            _onBossRoomStarted.OnTriggerEvent -= OnBossRoomStarted_OnTriggerEvent;
            _onMenuInputPressed.OnTriggerEvent -= OnMenuInputPressed_OnTriggerEvent;
            _onAllPlayerCharactersDead.OnTriggerEvent -= OnAllPlayerCharactersDead_OnTriggerEvent;

            if (_waitForSceneLoadedState)
            {
                _waitForSceneLoadedState.OnStateComplated -= WaitForLoadSceneState_OnStateComplated;
            }
            if (_waitForFadeInState)
            {
                _waitForFadeInState.OnStateComplated -= WaitForFadeInState_OnStateComplated;
            }
            if(_battleState)
            {
                _battleState.OnStateComplated -= BattleInRoomState_OnStateComplated;
            }
            if(_stageRewordState)
            {
                _stageRewordState.OnStateComplated -= StageRewordState_OnStateComplated;
            }
        }

        private void Start()
        {
            StateMachine.Start();

            if(!StateMachine.TrySetState(_waitForSceneLoadedState))
            {
                if (!StateMachine.TrySetState(_waitForFadeInState))
                {
                    if (!StateMachine.TrySetState(_createPlayerCharacterState))
                    {
                        StateMachine.TrySetState(_idleState);
                    }
                }
            }
        }

        private void OnRoomStarted_OnTriggerEvent()
        {
            StateMachine.TrySetState(_battleState);
        }
        private void OnBossRoomStarted_OnTriggerEvent()
        {
            StateMachine.TrySetState(_bossBattleState);
        }

        private void OnMenuInputPressed_OnTriggerEvent()
        {
            var prevState = StateMachine.CurrentState;
            
            if (StateMachine.TrySetState(_ingameMenuState))
            {
                _prevState = prevState;
            }
        }
        private void OnAllPlayerCharactersDead_OnTriggerEvent()
        {
            StateMachine.TrySetState(_gameoverState);
        }


        private void WaitForFadeInState_OnStateComplated(GameFlowState gameFlowState)
        {
            if (!StateMachine.TrySetState(_createPlayerCharacterState))
            {
                StateMachine.TrySetState(_idleState);
            }
        }
        private void WaitForLoadSceneState_OnStateComplated(GameFlowState gameFlowState)
        {
            if(!StateMachine.TrySetState(_waitForFadeInState))
            {
                if (!StateMachine.TrySetState(_createPlayerCharacterState))
                {
                    StateMachine.TrySetState(_idleState);
                }
            }
        }

        private void CreatePlayerCharacterState_OnStateComplated(GameFlowState gameFlowState)
        {
            StateMachine.TrySetState(_idleState);
        }

        private void StageRewordState_OnStateComplated(GameFlowState gameFlowState)
        {
            StateMachine.TrySetState(_idleState);
        }


        private void BattleInRoomState_OnStateComplated(GameFlowState gameFlowState)
        {
            StateMachine.TrySetState(_stageRewordState);
        }
        private void BossBattleState_OnStateComplated(GameFlowState gameFlowState)
        {
            StateMachine.TrySetState(_stageClearState);
        }



        private void StageMenuController_OnStageExit(StageMenuUIFlowController stageMenuController)
        {
            StateMachine.TrySetState(_loadMainmnuState);
        }

        private void StageMenuController_OnContinue(StageMenuUIFlowController stageMenuController)
        {
            if(!StateMachine.TrySetState(_prevState))
            {
                StateMachine.TrySetState(_idleState);
            }

            _prevState = null;
        }

        private void ResultController_OnRestart(ResultController resultController)
        {
            StateMachine.TrySetState(_reloadStageState);
        }
        private void ResultController_OnExit(ResultController resultController)
        {
            StateMachine.TrySetState(_loadMainmnuState);
        }
    }
}
