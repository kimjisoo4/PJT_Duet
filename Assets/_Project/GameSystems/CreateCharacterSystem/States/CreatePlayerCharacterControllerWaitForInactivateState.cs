using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Controller/State/Create Player Character Controller Wait For Inactivate State")]
    public class CreatePlayerCharacterControllerWaitForInactivateState : CreatePlayerCharacterControllerState
    {
        protected override void EnterState()
        {
            base.EnterState();

            CreateCharacterDisplay.Inactivate();
        }
    }
}
