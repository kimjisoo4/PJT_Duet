using UnityEngine;

namespace PF.PJT.Duet
{
    public class SelectCharacterUIInactiveState : SelectCharacterUIState
    {
        [Header(" [ Inactive State ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            _uiActor.SetActive(false);
        }
    }
}
