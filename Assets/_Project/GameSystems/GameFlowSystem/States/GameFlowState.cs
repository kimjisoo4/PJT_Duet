using PF.PJT.Duet.UISystem;
using StudioScor.Utilities;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{


    [AddComponentMenu("Duet/GameFlow/State/Game Flow State")]
    public class GameFlowState : BaseStateMono
    {
        public delegate void GameFlowStateEventHandler(GameFlowState gameFlowState);

        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<GameFlowState> { }

        [Header(" [ Game Flow State ] ")]
        [SerializeField] private GameObject _gameFlowSystemActor;

        [Header(" Block Input ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInputState;

        [Header(" HUD Visibility ")]
        [SerializeField] private HUDVisibilityController _hudVisivility;
        [SerializeField] private bool _hideHUD = false;

        private IGameFlowSystem _gameFlowSystem;
        private bool _isComplate;
        protected IGameFlowSystem GameFlowSystem => _gameFlowSystem;
        public bool IsComplate => _isComplate;

        public event GameFlowStateEventHandler OnStateComplated;

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            base.OnValidate();

            if(!_gameFlowSystemActor)
            {
                _gameFlowSystemActor = gameObject.GetGameObjectByTypeInParent<IGameFlowSystem>();
            }

            if(!_inputBlocker)
            {
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();
            }

            if(!_hudVisivility)
            {
                _hudVisivility = SUtility.FindAssetByType<HUDVisibilityController>();
            }
#endif
        }

        private void Awake()
        {
            if(_gameFlowSystemActor)
            {
                _gameFlowSystem = _gameFlowSystemActor.GetComponent<IGameFlowSystem>();
            }
            else
            {
                if(!gameObject.TryGetComponentInParent(out _gameFlowSystem))
                {
                    LogError($"{nameof(_gameFlowSystem)} is Not Find");
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnStateComplated = null;
            _inputBlocker.UnblockInput(this);

            if (_hideHUD)
                _hudVisivility.RequestShow(this);
        }

        protected override void EnterState()
        {
            base.EnterState();

            _isComplate = false;
            _inputBlocker.BlockInput(this, _blockInputState);
            if (_hideHUD)
                _hudVisivility.RequestHide(this);
        }
        protected override void ExitState()
        {
            base.ExitState();

            _inputBlocker.UnblockInput(this);
            if (_hideHUD)
                _hudVisivility.RequestShow(this);
        }

        protected void ComplateState()
        {
            if (_isComplate)
                return;

            Log(nameof(ComplateState));

            _isComplate = true;

            OnStateComplated?.Invoke(this);
        }

        public override string ToString()
        {
            return $"{gameObject.name} ({GetType().Name})";
        }
    }
}
