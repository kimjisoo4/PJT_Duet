using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Controller/State/Create Player Character Controller Wait For Activate State")]
    public class CreatePlayerCharacterControllerWaitForActivateState : CreatePlayerCharacterControllerState
    {
        protected override void EnterState()
        {
            base.EnterState();

            CreateCharacterDisplay.Activate();
        }
    }
}
