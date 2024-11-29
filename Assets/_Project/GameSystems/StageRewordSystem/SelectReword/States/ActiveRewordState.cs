using UnityEngine;

namespace PF.PJT.Duet
{
    public class ActiveRewordState : SelectRewordState
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
