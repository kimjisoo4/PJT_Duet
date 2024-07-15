using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class ControllerSetTurnDirectionState : PlayerSystemAction
    {
        [Header(" [ Controller Move To Target ] ")]
        [SerializeField] private ETurnDirectionState _turnDirectionState;

        public override TaskStatus OnUpdate()
        {
            ControllerSystem.SetTurnDirectionState(_turnDirectionState);

            return TaskStatus.Success;
        }
    }
}

