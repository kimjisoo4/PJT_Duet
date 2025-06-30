using UnityEngine;
using UnityEngine.UIElements;
namespace PF.PJT.Duet.UISystem
{
    public class TitleUI : BaseToolkitUI
    {
        public delegate void TitleUIEventHandler(TitleUI titleUI);

        [Header(" [ Title UI ] ")]
        [SerializeField] private string _titleName = "Title";
        
        [Header(" Class ")]
        [SerializeField] private string _titleUp = "title__up";
        [SerializeField] private string _titleUpEndElementName = "title-label";

        private VisualElement _title;
        private VisualElement _titleUpEndElement;

        public event TitleUIEventHandler OnTranslateFinished;


        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnTranslateFinished = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _title = Root.Q<VisualElement>(_titleName);
            _titleUpEndElement = Root.Q<VisualElement>(_titleUpEndElementName);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            _title?.UnregisterCallback<TransitionEndEvent>(OnTitleTransitionEnded);

            _title = null;
            _titleUpEndElement = null;
        }

        protected override void OnActivateEnd()
        {
            base.OnActivateEnd();

            _title.RegisterCallback<TransitionEndEvent>(OnTitleTransitionEnded);

            _title.AddToClassList(_titleUp);
        }
        protected override void OnDeactivateStart()
        {
            base.OnDeactivateStart();

            _title?.UnregisterCallback<TransitionEndEvent>(OnTitleTransitionEnded);
        }

        private void EndTranslate()
        {
            Log(nameof(EndTranslate));

            _title?.UnregisterCallback<TransitionEndEvent>(OnTitleTransitionEnded);

            Invoke_OnTranslateFinished();
        }

        private void OnTitleTransitionEnded(TransitionEndEvent endEvent)
        {
            Log(nameof(OnTitleTransitionEnded));

            if (endEvent.target is VisualElement target && target == _titleUpEndElement)
            {
                EndTranslate();
            }
        }

        private void Invoke_OnTranslateFinished()
        {
            Log(nameof(OnTranslateFinished));

            OnTranslateFinished?.Invoke(this);
        }
    }
}
