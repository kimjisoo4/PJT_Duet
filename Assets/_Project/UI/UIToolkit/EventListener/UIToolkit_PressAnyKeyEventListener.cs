using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public interface IUIToolkitPressAnyKeyEventListener : IUIToolkitEventListener
    {
        public event UIToolkitEventHandler OnPressed;
        public void PressAnyKey();
    }
    public class UIToolkit_PressAnyKeyEventListener : UIToolkitEventListener, IUIToolkitPressAnyKeyEventListener
    {
        [Header(" [ Press Any Key Event Listener ] ")]
        [SerializeField] private EInputType _inputType = EInputType.Both;

        public event IUIToolkitEventListener.UIToolkitEventHandler OnPressed;

        private void OnDestroy()
        {
            OnPressed = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            switch (_inputType)
            {
                case EInputType.Both:
                    Target.RegisterCallback<KeyDownEvent>(OnKeyDown);
                    Target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                    break;
                case EInputType.Mouse:
                    Target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                    break;
                case EInputType.Keyboard:
                    Target.RegisterCallback<KeyDownEvent>(OnKeyDown);
                    break;
            }
        }
        protected override void OnDisable()
        {
            Target?.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            Target?.UnregisterCallback<PointerDownEvent>(OnPointerDown);

            base.OnDisable();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            PressAnyKey();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            PressAnyKey();
        }

        public void PressAnyKey()
        {
            Log(nameof(PressAnyKey));

            OnPressed?.Invoke(this);
        }
    }
}