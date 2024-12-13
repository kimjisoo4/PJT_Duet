using StudioScor.Utilities;
using StudioScor.Utilities.FadeSystem;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class FadeSystemStateObserver : MonoBehaviour
    {
        [Header(" [ Fade System State Observer ] ")]
        [SerializeField] private FadeSystemComponent _fadeSystem;

        [Header(" Variables ")]
        [SerializeField] private GameObjectListVariable _activeUIInputVariable;
        [SerializeField] private GameObjectListVariable _inactiveStatusVariable;
        private void Start()
        {
            if(_fadeSystem.State != EFadeState.FadeIn || _fadeSystem.IsFading )
            {
                AddToVariable();
            }

            _fadeSystem.OnStartedFadeOut += _fadeSystem_OnStartedFadeOut;
            _fadeSystem.OnFinishedFadeIn += _fadeSystem_OnFinishedFadeIn;
        }
        private void OnDestroy()
        {
            RemoveToVariable();

            if (_fadeSystem)
            {
                _fadeSystem.OnFinishedFadeIn -= _fadeSystem_OnFinishedFadeIn;
            }
        }
        private void AddToVariable()
        {
            _activeUIInputVariable.Add(gameObject);
            _inactiveStatusVariable.Add(gameObject);
        }
        private void RemoveToVariable()
        {
            _activeUIInputVariable.Remove(gameObject);
            _inactiveStatusVariable.Remove(gameObject);
        }

        private void _fadeSystem_OnStartedFadeOut(IFadeSystem fadeSystem)
        {
            AddToVariable();
        }

        private void _fadeSystem_OnFinishedFadeIn(IFadeSystem fadeSystem)
        {
            RemoveToVariable();
        }
    }
}
