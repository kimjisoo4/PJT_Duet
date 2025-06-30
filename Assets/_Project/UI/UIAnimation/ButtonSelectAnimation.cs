using DG.Tweening;
using StudioScor.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PF.PJT.Duet.UISystem
{
    public class ButtonSelectAnimation : BaseMonoBehaviour
    {
        [Header(" [ Button Select Animation ] ")]
        [SerializeField] private GameObject _buttonActor;
        [SerializeField] private TMP_Text _buttonName;
        [SerializeField] private Image _buttonImage;

        [Header(" Color ")]
        [SerializeField] private Color _normalColor = Color.clear;
        [SerializeField] private Color _selectColor = Color.gray;
        [SerializeField] private Color _submitColor = Color.white;

        [Header(" Select Tween ")]
        [SerializeField] private float _fadeInTime = 1f;

        [Header(" Deselect Tween ")]
        [SerializeField] private float _fadeOutTime = 1f;

        [Header(" Submit Tween ")]
        [SerializeField] private float _flashTime = 1f;

        private Sequence _selectSequence;
        private Sequence _deselectSequence;
        private Sequence _submitSequence;

        private ISelectEventListener _buttonSelectEvent;
        private IDeselectEventListener _buttonDeselectEvent;
        private ISubmitEventListener _buttonSubmitEvent;


        private void Awake()
        {
            _buttonSelectEvent = _buttonActor.GetComponent<ISelectEventListener>();
            _buttonDeselectEvent = _buttonActor.GetComponent<IDeselectEventListener>();
            _buttonSubmitEvent = _buttonActor.GetComponent<ISubmitEventListener>();

            _buttonSelectEvent.OnSelected += _buttonSelectEvent_OnSelected;
            _buttonDeselectEvent.OnDeselected += _buttonDeselectEvent_OnDeselected;
            _buttonSubmitEvent.OnSubmited += _buttonSubmitEvent_OnSubmited;
        }
        private void OnDestroy()
        {
            if(_buttonSelectEvent is not null)
            {
                _buttonSelectEvent.OnSelected -= _buttonSelectEvent_OnSelected;
            }
            
            if(_buttonDeselectEvent is not null)
            {
                _buttonDeselectEvent.OnDeselected -= _buttonDeselectEvent_OnDeselected;
            }
        }
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            if (_buttonImage.color != _normalColor)
                _buttonImage.color = _normalColor;
#endif
        }

        private void OnSelect()
        {
            if (_deselectSequence is not null && _deselectSequence.IsPlaying())
                _deselectSequence.Complete();

            if(_selectSequence is null)
            {
                _selectSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);
                _selectSequence.Append(_buttonImage.DOColor(_selectColor, _fadeInTime));
                _selectSequence.OnPlay(() =>
                {
                    _buttonName.fontStyle = FontStyles.Bold;
                });
            }
            else
            {
                _selectSequence.Restart();
            }
        }

        private void OnDeselect()
        {
            if (_selectSequence is not null && _selectSequence.IsPlaying())
                _selectSequence.Complete();

            if (_submitSequence is not null && _submitSequence.IsPlaying())
                _submitSequence.Complete();

            if (_deselectSequence is null)
            {
                _deselectSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);
                _deselectSequence.Append(_buttonImage.DOColor(_normalColor, _fadeOutTime));
                _deselectSequence.OnPlay(() =>
                {
                    _buttonName.fontStyle = FontStyles.Normal;
                });
            }
            else
            {
                _deselectSequence.Restart();
            }
        }
        private void OnSubmit()
        {
            if (_selectSequence is not null && _selectSequence.IsPlaying())
                _selectSequence.Complete();

            if (_submitSequence is null)
            {
                _submitSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);
                _submitSequence.Append(_buttonImage.DOColor(_submitColor, _flashTime));
                _submitSequence.Append(_buttonImage.DOColor(_selectColor, _fadeInTime));
            }
            else
            {
                _submitSequence.Restart();
            }
        }

        private void _buttonDeselectEvent_OnDeselected(IDeselectEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            OnDeselect();
        }

        private void _buttonSelectEvent_OnSelected(ISelectEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            OnSelect();
        }

        private void _buttonSubmitEvent_OnSubmited(ISubmitEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            OnSubmit();
        }
    }
}
