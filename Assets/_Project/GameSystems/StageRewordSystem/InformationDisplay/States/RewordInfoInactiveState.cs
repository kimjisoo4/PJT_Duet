using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordInfoInactiveState : RewordInformationDisplayState
    {
        [Header(" [ Reword Infomation Inactive ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            if(_uiActor.activeSelf)
            {
                _uiActor.SetActive(false);
            }
        }
    }
}
