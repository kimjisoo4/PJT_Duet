using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Controller/State/Create Player Character Controller State")]
    public class CreatePlayerCharacterControllerState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<CreatePlayerCharacterControllerState> { }

        [Header(" [ Create Player Character State ] ")]
        [SerializeField] private CreatePlayerCharacterController _createPlayerCharacterController;

        public CreatePlayerCharacterController CreatePlayerCharacterController => _createPlayerCharacterController;
        public ICreateCharacterDisplay CreateCharacterDisplay => _createPlayerCharacterController.CreateCharacterDisplay;
        public IReadOnlyCollection<CreateCharacterButton> CreateCharacterButtons => _createPlayerCharacterController.CreateCharacterButtons;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (!_createPlayerCharacterController)
            {
                _createPlayerCharacterController = GetComponentInParent<CreatePlayerCharacterController>();
            }
#endif
        }

        public virtual void SubmitedButton(CreateCharacterButton selectCharacterUI)
        {
            return;
        }
    }
}
