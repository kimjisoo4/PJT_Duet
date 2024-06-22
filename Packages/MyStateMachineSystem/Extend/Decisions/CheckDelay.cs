using UnityEngine;
using System.Collections.Generic;
using StudioScor.Utilities;

namespace StudioScor.StateMachine
{

    [CreateAssetMenu(menuName ="StudioScor/StateMachine/Decisions/new Check Delay", fileName = "Check_Delay")]
    public class CheckDelay : Decision
    {
        [Header(" [ Check Delay ] ")]
        [SerializeField, Min(0f)] private float _delay = 2f;
        [SerializeField] private bool _useRand = true;
        [SerializeField][SCondition(nameof(_useRand))] private float _randDelay = 0.5f;

        private Dictionary<StateMachineComponent, float> _datas;

        protected override void OnReset()
        {
            base.OnReset();

            _datas = null;
        }

        public override void EnterDecide(StateMachineComponent stateMachine)
        {
            if(_useRand)
            {
                if (_datas is null)
                    _datas = new();

                float delay = _delay + Random.Range(0, _randDelay);

                _datas.Add(stateMachine, delay);
                Log("Delay Time - " + delay.ToString("N1"));
            }
        }

        public override void ExitDecide(StateMachineComponent stateMachine)
        {
            if(_useRand && _datas is not null)
            {
                _datas.Remove(stateMachine);
            }
        }

        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            float delay;

            if(_useRand)
            {
                delay = _datas[stateMachine];
            }
            else
            {
                delay = _delay;
            }

            Log("State Elapesed - " + stateMachine.StateElapsed.ToString("N1") + " Delay Time - " + delay.ToString("N1"));

            return stateMachine.StateElapsed > delay;
        }
    }
}
