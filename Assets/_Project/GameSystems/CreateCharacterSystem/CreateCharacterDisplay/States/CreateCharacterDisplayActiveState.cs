using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Display/State/Create Character Display Active State")]
    public class CreateCharacterDisplayActiveState : CreateCharacterDisplayState
    {
        [Header(" [ Active State ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);
        }
    }
}
