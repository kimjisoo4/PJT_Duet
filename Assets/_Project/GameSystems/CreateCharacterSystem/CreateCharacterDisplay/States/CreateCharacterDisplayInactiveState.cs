using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Display/State/Create Character Display Inactive State")]
    public class CreateCharacterDisplayInactiveState : CreateCharacterDisplayState
    {
        [Header(" [ Inactive State ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            if (_uiActor.activeSelf)
                _uiActor.SetActive(false);
        }
    }
}
