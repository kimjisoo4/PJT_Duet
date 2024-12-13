using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Button/State/Create Character Button Active State")]
    public class CreateCharacterButtonActiveState : CreateCharacterButtonState
    {
        [Header(" [ Activate State ] ")]
        [SerializeField] private GameObject _uiActor;

        public override bool CanEnterState()
        {
            return base.CanEnterState() && !CreateCharacterButton.IsInTransition && CreateCharacterButton.CharacterData is not null;
        }
        protected override void EnterState()
        {
            base.EnterState();

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);
        }
    }
}
