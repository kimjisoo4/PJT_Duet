using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public interface IUIToolkitCancelEventListener : IUIToolkitEventListener
    {
        public event UIToolkitEventHandler OnCanceled;
        public void Cancel();
    }
    public class UIToolkit_CancelEventListener : UIToolkitEventListener, IUIToolkitCancelEventListener
    {
        //[Header(" [ Cancel Event Listener ] ")]
        
        public event IUIToolkitEventListener.UIToolkitEventHandler OnCanceled;

        private void OnDestroy()
        {
            OnCanceled = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Target.RegisterCallback<NavigationCancelEvent>(OnNavigationCanceled);
        }

        protected override void OnDisable()
        {
            Target?.UnregisterCallback<NavigationCancelEvent>(OnNavigationCanceled);
            
            base.OnDisable();
        }

        public void Cancel()
        {
            Log(nameof(Cancel));

            OnCanceled?.Invoke(this);
        }

        private void OnNavigationCanceled(NavigationCancelEvent evt)
        {
            Cancel();
        }
    }
}