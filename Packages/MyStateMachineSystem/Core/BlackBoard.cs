using UnityEngine;
using StudioScor.Utilities;
using System.Collections.Generic;

namespace StudioScor.StateMachine
{
    [CreateAssetMenu(menuName ="StudioScor/StateMachine/new BlackBoard")]
    public class BlackBoard : BaseScriptableObject
    {
        [Header(" [ Blackbaord ] ")]
        [SerializeField] private List<BlackBoardKeyBase> _BlackBoardKeys;

        public void Create(StateMachineComponent stateMachine)
        {
            if (!stateMachine)
                return;

            foreach (var key in _BlackBoardKeys)
            {
                key.Create(stateMachine);
            }
        }
        public void Clear(StateMachineComponent stateMachine)
        {
            if (!stateMachine)
                return;

            foreach (var blackBoardValue in _BlackBoardKeys)
            {
                blackBoardValue.Clear(stateMachine);
            }
        }
        public void Remove(StateMachineComponent stateMachine)
        {
            if (!stateMachine)
                return;

            foreach (var blackBoardValue in _BlackBoardKeys)
            {
                blackBoardValue.Remove(stateMachine);
            }
        }
    }
}