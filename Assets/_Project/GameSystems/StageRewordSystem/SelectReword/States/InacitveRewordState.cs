using UnityEngine;

namespace PF.PJT.Duet
{
    public class InacitveRewordState : SelectRewordState
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
