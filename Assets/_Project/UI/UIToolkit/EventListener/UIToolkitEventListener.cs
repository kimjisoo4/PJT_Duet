using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public interface IUIToolkitEventListener
    {
        public delegate void UIToolkitEventHandler(IUIToolkitEventListener uitookiEventListener);
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public bool enabled { get; set; }
        public UIDocument UIDocument { get; }
        public VisualElement Target { get; }
    }

    [DefaultExecutionOrder(-1)]
    public class UIToolkitEventListener : BaseMonoBehaviour, IUIToolkitEventListener
    {
        [Header(" [ UI Toolkit Event Listener ] ")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private string _targetName;
        private VisualElement _target;
        public UIDocument UIDocument => _uiDocument;
        public VisualElement Target => _target;


        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            if (!_uiDocument)
            {
                _uiDocument = gameObject.GetComponentInParent<UIDocument>();
            }
#endif
        }

        protected virtual void OnEnable()
        {
            _target = _uiDocument.rootVisualElement.Q<VisualElement>(_targetName);
        }
        protected virtual void OnDisable()
        {
            _target = null;
        }
    }
}
