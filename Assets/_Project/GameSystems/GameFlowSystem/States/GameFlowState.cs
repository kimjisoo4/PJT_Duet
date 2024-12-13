using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{

    [AddComponentMenu("Duet/GameFlow/State/Game Flow State")]
    public class GameFlowState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<GameFlowState> { }

        [Header(" [ Game Flow State ] ")]
        [SerializeField] private GameFlowSystemComponent _gameFlowSystem;


        [Header(" Next States ")]
        [SerializeField] GameFlowState[] _nextStates;

        protected GameFlowSystemComponent GameFlowSystem => _gameFlowSystem;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if(!_gameFlowSystem)
            {
                _gameFlowSystem = GetComponentInParent<GameFlowSystemComponent>();
            }
#endif
        }

        protected bool TryNextState()
        {
            foreach (var state in _nextStates)
            {
                if (_gameFlowSystem.StateMachine.TrySetState(state))
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{gameObject.name} ({GetType().Name})";
        }
    }
}
