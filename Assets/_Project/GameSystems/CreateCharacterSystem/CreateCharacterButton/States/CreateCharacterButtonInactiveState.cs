using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Button/State/Create Character Button Inactive State")]
    public class CreateCharacterButtonInactiveState : CreateCharacterButtonState
    {
        [Header(" [ Inactive State ] ")]
        [SerializeField] private GameObject _uiActor;

        public override bool CanEnterState()
        {
            return base.CanEnterState() && !CreateCharacterButton.IsInTransition;
        }
        protected override void EnterState()
        {
            base.EnterState();

            _uiActor.SetActive(false);
        }
    }
}
