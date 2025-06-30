using UnityEngine;
using UnityEngine.UIElements;
namespace PF.PJT.Duet.UISystem
{
    public class PressAnyKeyUI : BaseToolkitUI
    {
        public delegate void PressAnyKeyUIStateEventHandler(PressAnyKeyUI pressAnyKeyUIController);

        [Header(" [ Press Any Key UI Controller ] ")]
        [SerializeField] private GameObject _pressAnyKeyButtonActor;
        [SerializeField] private string _pressAnyKeyName = "PressAnyKey";

        [Header(" Class ")]
        [SerializeField] private string _pressAnyKey_FadeOut = "pressanykey-label__fadeout";
        [SerializeField] private string _pressAnyKey_Press = "pressanykey-label__press";

        private IUIToolkitPressAnyKeyEventListener _pressAnyKeyEvent;
        private VisualElement _pressAnyKey;

        public event PressAnyKeyUIStateEventHandler OnAnyKeyPressed;

        protected override void Awake()
        {
            base.Awake();

            _pressAnyKeyEvent = _pressAnyKeyButtonActor.GetComponent<IUIToolkitPressAnyKeyEventListener>();

        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            RemoveAllBinding();

            OnAnyKeyPressed = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _pressAnyKey = Root.Q<VisualElement>(_pressAnyKeyName);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            _pressAnyKey = null;

            _pressAnyKey?.UnregisterCallback<TransitionEndEvent>(OnPressAnyKeyTransitionEnd);
        }
        private void RemoveAllBinding()
        {
            if (_pressAnyKeyEvent is not null)
                _pressAnyKeyEvent.OnPressed -= PressAnyKey_OnPressed;

        }
        protected override void OnActivateEnd()
        {
            base.OnActivateEnd();

            _pressAnyKeyEvent.OnPressed += PressAnyKey_OnPressed;

            _pressAnyKey.RegisterCallback<TransitionEndEvent>(OnPressAnyKeyTransitionEnd);
            _pressAnyKey.ToggleInClassList(_pressAnyKey_FadeOut);

            _pressAnyKeyEvent.Target.Focus();
        }

        protected override void OnDeactivateStart()
        {
            base.OnDeactivateStart();

            RemoveAllBinding();

            _pressAnyKey.UnregisterCallback<TransitionEndEvent>(OnPressAnyKeyTransitionEnd);
        }

        private void OnPressAnyKeyTransitionEnd(TransitionEndEvent evt)
        {
            Log(nameof(OnPressAnyKeyTransitionEnd));

            if (!_pressAnyKey.ClassListContains(_pressAnyKey_Press))
            {
                Log($"Not has '{_pressAnyKey_Press}' class");
                _pressAnyKey.ToggleInClassList(_pressAnyKey_FadeOut);
            }
            else
            {
                Log($"Has '{_pressAnyKey_Press}' class");
            }
        }

        private void PressAnyKey_OnPressed(IUIToolkitEventListener uitookiEventListener)
        {
            _pressAnyKey.AddToClassList(_pressAnyKey_Press);

            Invoke_OnAnykeyPressed();
        }

        private void Invoke_OnAnykeyPressed()
        {
            Log(nameof(OnAnyKeyPressed));

            OnAnyKeyPressed?.Invoke(this);
        }
    }
}
