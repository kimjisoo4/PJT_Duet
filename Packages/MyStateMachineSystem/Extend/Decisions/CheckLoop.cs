using UnityEngine;
using System.Collections.Generic;
using StudioScor.Utilities;

namespace StudioScor.StateMachine
{
    [CreateAssetMenu(menuName = "StudioScor/StateMachine/Decisions/new Check Loop", fileName = "Check_Loop_")]
    public class CheckLoop : Decision
    {
        [Header(" [ Check Loop ] ")]
        [SerializeField] private bool _useInfinity = false;
        [SerializeField][SCondition(nameof(_useInfinity),  true, true)] private int _loop = 2;

        private Dictionary<StateMachineComponent, int> _data;

        public override void EnterDecide(StateMachineComponent stateMachine)
        {
            if (!_useInfinity)
            {
                if (_data is null)
                    _data = new();

                if (_data.ContainsKey(stateMachine))
                {
                    _data[stateMachine] = _data[stateMachine] + 1;
                }
                else
                {
                    _data.Add(stateMachine, 0);
                }
            }
        }

        public override void ExitDecide(StateMachineComponent stateMachine)
        {
            base.ExitDecide(stateMachine);

            if (!_useInfinity && _data is not null)
            {
                _data.Remove(stateMachine);
            }
        }
        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            if (_useInfinity)
                return false;

            if (_data.TryGetValue(stateMachine, out int value) && value < _loop)
                return false;

            return true;
        }
    }
}
