using StudioScor.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public class BaseToolkitUI : BaseMonoBehaviour
    {
        [Header(" [ UI Toolkit UI Controller ] ")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private string _rootName = "Root";
        [SerializeField] private string _rootDeactivate = "root__deactivate";
        [SerializeField] private string _rootFadeIn = "root__fadein";
        [SerializeField] private string _rootFadeOut = "root__fadeout";

        private VisualElement _root;
        private bool _isInTransition;
        private float _sortingOrder;

        public float SortingOrder => _uiDocument.sortingOrder;
        protected UIDocument UIDocument => _uiDocument;
        protected VisualElement Root => _root;
        public bool IsInTransition => _isInTransition;
        public bool IsActive => gameObject.activeInHierarchy;
        public bool IsPlaying => gameObject.activeInHierarchy && !_isInTransition;

        public event EventHandler OnActivateStarted;
        public event EventHandler OnActivateEnded;
        public event EventHandler OnDeactivateStarted;
        public event EventHandler OnDeactivateEnded;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            if (!_uiDocument)
            {
                _uiDocument = GetComponent<UIDocument>();
            }
#endif
        }

        protected virtual void Awake()
        {
            _sortingOrder = _uiDocument.sortingOrder;
            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            OnActivateStarted = null;
            OnActivateEnded = null;
            OnDeactivateStarted = null;
            OnDeactivateEnded = null;
        }

        protected virtual void OnEnable()
        {
            _root = _uiDocument.rootVisualElement.Q<VisualElement>(_rootName);
            _root.AddToClassList(_rootDeactivate);
        }
        protected virtual void OnDisable()
        {
            _root?.UnregisterCallback<TransitionEndEvent>(OnRootTransitionEnded);

            _root = null;
        }


        [ContextMenu(nameof(Activate), false, 1000000)]
        public void Activate()
        {
            if (gameObject.activeSelf)
                return;

            _isInTransition = true;
            gameObject.SetActive(true);
                
            _root.RegisterCallback<TransitionEndEvent>(OnRootTransitionEnded);

            StartCoroutine(OnActivate());
        }
        private IEnumerator OnActivate()
        {
            yield return null;

            SetEnabled(true);

            OnActivateStart();

            RaiseOnActivateStarted();
        }

        [ContextMenu(nameof(Deactivate), false, 1000000)]
        public void Deactivate()
        {
            if (!gameObject.activeSelf)
                return;;

            _isInTransition = true;

            _root.RegisterCallback<TransitionEndEvent>(OnRootTransitionEnded);
            SetEnabled(false);

            OnDeactivateStart();

            RaiseOnDeactivateStarted();
        }
        private void ActivateEnd()
        {
            OnActivateEnd();

            RaiseOnActivateEnded();
        }
        private void DeactivateEnd()
        {
            gameObject.SetActive(false);
            OnDeactivateEnd();

            RaiseOnDeactivateEnded();
        }

        private void SetEnabled(bool enabled)
        {
            if (_root is null)
                return;

            SetInteraction(enabled);

            if (enabled)
            {
                _root.AddToClassList(_rootFadeIn);
            }
            else
            {
                _root.AddToClassList(_rootFadeOut);
            }
        }

        public void SetInteraction(bool useInteraction)
        {
            if (_root is null)
                return;

            _root.SetEnabled(useInteraction);
        }
        public void SetSortingOrder(float newOrder)
        {
            _uiDocument.sortingOrder = newOrder;
        }
        public void ResetSortingOrder()
        {
            _uiDocument.sortingOrder = _sortingOrder;
        }

        protected virtual void OnActivateStart() { }
        protected virtual void OnActivateEnd() { }
        protected virtual void OnDeactivateStart() { }
        protected virtual void OnDeactivateEnd() { }


        private void OnRootTransitionEnded(TransitionEndEvent endEvent)
        {
            Log($"{endEvent.target} {_root}", SUtility.STRING_COLOR_RED);

            if(endEvent.target is VisualElement root && root == _root)
            {
                _root.UnregisterCallback<TransitionEndEvent>(OnRootTransitionEnded);

                Log(nameof(OnRootTransitionEnded));

                _isInTransition = false;

                if (_root.enabledSelf)
                {
                    ActivateEnd();
                }
                else
                {
                    DeactivateEnd();
                }
            }

        }
        private void RaiseOnActivateStarted()
        {
            Log(nameof(OnActivateStarted));

            OnActivateStarted?.Invoke(this, EventArgs.Empty);
        }
        private void RaiseOnActivateEnded()
        {
            Log(nameof(OnActivateEnded));

            OnActivateEnded?.Invoke(this, EventArgs.Empty);
        }
        private void RaiseOnDeactivateStarted()
        {
            Log(nameof(OnDeactivateStarted));

            OnDeactivateStarted?.Invoke(this, EventArgs.Empty);
        }
        private void RaiseOnDeactivateEnded()
        {
            Log(nameof(OnDeactivateEnded));

            OnDeactivateEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}
