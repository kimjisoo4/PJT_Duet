using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class PlayerDamageScreenEffectModifier : PlayerDamageEventModifier
    {
        [Header(" [ Player Damage Screen Effect Modifier ] ")]
        [SerializeField] private GameObject _screenEffectActor;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeInTime = 0.5f;
        [SerializeField] private float _fadeOutTime = 0.5f;

        private readonly ITimer _timer = new Timer();
        private bool _isBackward = false;

        private void Awake()
        {
            _screenEffectActor.SetActive(false);
            _canvasGroup.alpha = 0f;
            enabled = false;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _timer.UpdateTimer(deltaTime);

            if(!_isBackward)
            {
                UpdateFadeIn(_timer.NormalizedTime);
            }
            else
            {
                UpdateFadeOut(_timer.NormalizedTime);
            }
        }
        public override void OnHit(DamageInfoData damageInfo)
        {
            OnScreenEffect();
        }
        
        private void OnScreenEffect()
        {
            if (!_screenEffectActor.activeSelf)
                _screenEffectActor.SetActive(true);

            if (!enabled)
                enabled = true;

            OnFadeIn();
        }
        private void EndScreenEffect()
        {
            if (_screenEffectActor.activeSelf)
                _screenEffectActor.SetActive(false);

            enabled = false;
        }
        private void OnFadeIn()
        {
            _isBackward = false;

            if (_fadeInTime > 0f)
            {
                if (_timer.IsPlaying)
                {
                    float noramlizedTime = _timer.NormalizedTime;

                    _timer.EndTimer();

                    _timer.OnTimer(_fadeInTime);
                    _timer.JumpTime(_fadeInTime * noramlizedTime);
                }
                else
                {
                    _timer.OnTimer(_fadeInTime);
                }
            }
            else
            {
                if (_timer.IsPlaying)
                    _timer.EndTimer();

                SetValue(1f);

                OnFadeOut();
            }
        }

        private void UpdateFadeIn(float normalizedTime)
        {
            SetValue(normalizedTime);

            if (_timer.IsFinished)
            {
                OnFadeOut();
            }
        }

        private void OnFadeOut()
        {
            _isBackward = true;

            if (_fadeOutTime > 0f)
            {
                _timer.OnTimer(_fadeOutTime);
            }
            else
            {
                SetValue(0f);
            }
        }
        private void UpdateFadeOut(float normalizedTime)
        {
            SetValue(1f - _timer.NormalizedTime);

            if (_timer.IsFinished)
            {
                EndScreenEffect();
            }
        }

        private void SetValue(float newValue)
        {
            _canvasGroup.alpha = newValue;
        }
    }
}
