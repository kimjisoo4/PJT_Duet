using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Controller/State/Create Player Character Controller Wait For Select Character State")]
    public class CreatePlayerCharacterControllerWaitForSelectCharacterState : CreatePlayerCharacterControllerState
    {
        protected override void EnterState()
        {
            base.EnterState();

            EventSystem.current.SetSelectedGameObject(CreateCharacterButtons.ElementAt(Mathf.FloorToInt(CreateCharacterButtons.Count * 0.5f)).gameObject);
        }

        public override void SubmitedButton(CreateCharacterButton selectCharacterUI)
        {
            base.SubmitedButton(selectCharacterUI);

            if (selectCharacterUI.CharacterData is null)
                return;

            CreatePlayerCharacterController.AddPlayerCharacter(selectCharacterUI.CharacterData);

            selectCharacterUI.SetCharacterData(null);
        }
    }
}
