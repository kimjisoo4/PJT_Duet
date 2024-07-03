using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class FloatingDamageTextComponent : BaseMonoBehaviour
    {
        [Header(" [ Floating Damage Text Component ] ")]
        [SerializeField] private FloatingTextComponent _floatingText;
        [SerializeField] private LookAtCamera _lookAtCamera;
        [SerializeField] private Timer _timer;

        private void Awake()
        {
            _lookAtCamera.TryEnterState();
            _timer.OnFinishedTimer += _timer_OnFinishedTimer;
        }

        private void OnEnable()
        {
            _timer.OnTimer();
        }
        private void OnDisable()
        {
            _timer.EndTimer();
        }
        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _timer.UpdateTimer(deltaTime);
        }
        private void _timer_OnFinishedTimer(ITimer timer)
        {
            _floatingText.Release();

            gameObject.SetActive(false);
        }
    }
}
