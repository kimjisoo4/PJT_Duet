using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class Mainmenu_TitleUIState : BaseUIFlowState
    {
        [Header(" [ Title UI State ] ")]
        [SerializeField] private TitleController _titleController;
        [SerializeField] private BaseUIFlowState _nextState;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_titleController)
            {
                _titleController.OnAnyKeyPress -= TitleController_OnAnyKeyPress;
            }
        }
        protected override void EnterState()
        {
            base.EnterState();

            _titleController.Activate();

            _titleController.OnAnyKeyPress += TitleController_OnAnyKeyPress;
        }
        protected override void ExitState()
        {
            base.ExitState();

            if(_titleController)
            {
                _titleController.OnAnyKeyPress -= TitleController_OnAnyKeyPress;
                _titleController.SetInteraction(false);
            }
        }

        private void TitleController_OnAnyKeyPress(TitleController titleController)
        {
            UIFlowController.StateMachine.TrySetState(_nextState);
        }
    }
}
