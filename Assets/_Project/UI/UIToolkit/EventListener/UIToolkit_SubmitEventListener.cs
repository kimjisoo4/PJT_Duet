using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{


    public interface IUIToolkitSubmitEventListener : IUIToolkitEventListener
    {
        public event UIToolkitEventHandler OnSubmited;
        public void Submit();
    }

    public class UIToolkit_SubmitEventListener : UIToolkitEventListener, IUIToolkitSubmitEventListener
    {
        [Header(" [ Submit Event Listener ] ")]
        [SerializeField] private EInputType _inputType = EInputType.Both;

        public event IUIToolkitEventListener.UIToolkitEventHandler OnSubmited;

        private void OnDestroy()
        {
            OnSubmited = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
         
            switch (_inputType)
            {
                case EInputType.Both:
                    Target.RegisterCallback<ClickEvent>(OnClicked);
                    Target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmited);
                    break;
                case EInputType.Mouse:
                    Target.RegisterCallback<ClickEvent>(OnClicked);
                    break;
                case EInputType.Keyboard:
                    Target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmited);
                    break;
                default:
                    break;
            }
        }
        protected override void OnDisable()
        {
            Target?.UnregisterCallback<ClickEvent>(OnClicked);
            Target?.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmited);

            base.OnDisable();
        }

        public void Submit()
        {
            Log(nameof(Submit));

            OnSubmited?.Invoke(this);
        }

        private void OnNavigationSubmited(NavigationSubmitEvent evt)
        {
            Submit();
        }
        private void OnClicked(ClickEvent evt)
        {
            Submit();
        }
    }
}