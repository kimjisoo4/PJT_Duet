using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Controller/State/Create Player Character Controller Wait For Deactivate State")]
    public class CreatePlayerCharacterControllerWaitForDeactivateState : CreatePlayerCharacterControllerState
    {
        protected override void EnterState()
        {
            base.EnterState();

            CreateCharacterDisplay.Deactivate();
        }
    }
}
