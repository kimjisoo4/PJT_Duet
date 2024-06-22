using UnityEngine;
using System.Collections.Generic;

namespace StudioScor.StateMachine
{
    public abstract class BlackBoardKey<T> : BlackBoardKeyBase, ISerializationCallbackReceiver
    {
        private Dictionary<StateMachineComponent, BlackBoardValue<T>> _BlackBoardValues;

        #region ISerializationCallbackReceiver
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            OnReset();
        }
        #endregion

        protected override void OnReset()
        {
            base.OnReset();

            _BlackBoardValues = null;
        }

        public bool TryGetValue(StateMachineComponent stateMachine, out T value)
        {
            if (_BlackBoardValues.ContainsKey(stateMachine) && _BlackBoardValues[stateMachine].TryGetValue(out value))
                return true;
            else
                value = default;

            return false;
        }
        public void SetValue(StateMachineComponent stateMachine, T value)
        {
            if (_BlackBoardValues.ContainsKey(stateMachine))
            {
                Log($"SetValue ({stateMachine.name} - {value}");

                _BlackBoardValues[stateMachine].SetValue(value);
            }
        }

        public override bool HasValue(StateMachineComponent stateMachine)
        {
            return _BlackBoardValues.ContainsKey(stateMachine) && _BlackBoardValues[stateMachine].HasValue;
        }

        public override void Create(StateMachineComponent stateMachine)
        {
            if (_BlackBoardValues is null)
                _BlackBoardValues = new();

            _BlackBoardValues.Add(stateMachine, new BlackBoardValue<T>());
        }
        public override void Clear(StateMachineComponent stateMachine)
        {
            if (_BlackBoardValues is null)
                _BlackBoardValues = new();

            if(_BlackBoardValues.TryGetValue(stateMachine, out BlackBoardValue<T> value))
            {
                value.Clear();
            }
        }
        public override void Remove(StateMachineComponent stateMachine)
        {
            if (_BlackBoardValues is null)
                _BlackBoardValues = new();

            _BlackBoardValues.Remove(stateMachine);
        }
    }
}