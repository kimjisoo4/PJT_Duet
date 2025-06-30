using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class HUDVisibilityRelay : BaseMonoBehaviour
    {
        [Header(" [ HUD Visibility Relay With Player Skill State ] ")]
        [SerializeField] private HUDVisibilityController _hudVisibility;
        [SerializeField] private GameObject _targetActor;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_hudVisibility)
            {
                _hudVisibility = SUtility.FindAssetByType<HUDVisibilityController>();
            }
#endif
        }

        private void OnEnable()
        {
            SyncVisibility();
            
            _hudVisibility.OnVisibilityChanged += _hudVisibility_OnVisibilityChanged;
        }
        private void OnDisable()
        {
            _hudVisibility.OnVisibilityChanged -= _hudVisibility_OnVisibilityChanged;
        }

        private void SyncVisibility()
        {
            _targetActor.SetActive(_hudVisibility.IsVisible);
        }
        private void _hudVisibility_OnVisibilityChanged(bool isVisible)
        {
            SyncVisibility();
        }
    }
}
