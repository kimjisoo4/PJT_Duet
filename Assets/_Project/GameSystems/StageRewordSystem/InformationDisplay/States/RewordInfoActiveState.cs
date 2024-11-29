using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordInfoActiveState : RewordInformationDisplayState
    {
        [Header(" [ Reword Infomation Active ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            if (!_uiActor.activeSelf)
            {
                _uiActor.SetActive(true);
            }
        }
    }
}
