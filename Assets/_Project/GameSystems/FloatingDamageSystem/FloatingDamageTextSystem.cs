using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.System
{
    public class FloatingDamageTextSystem : BaseMonoBehaviour
    {
        [Header(" [ Floating Damage Text ] ")]
        [SerializeField] private DamageResultEvent _onTakeDamage;
        [SerializeField] private Canvas _floatingCanvas;

        [SerializeField] private FloatingTextContainer _floatingDamage;

        private void Awake()
        {
            _onTakeDamage.OnTriggerGenericEvent += _onTakeDamage_OnTriggerGenericEvent;

            _floatingDamage.SetupPool(_floatingCanvas);
        }
        private void OnDestroy()
        {
            _onTakeDamage.OnTriggerGenericEvent -= _onTakeDamage_OnTriggerGenericEvent;
        }

        private void _onTakeDamage_OnTriggerGenericEvent(FDamageResult damageResult)
        {
            _floatingDamage.SpawnFloatingDamage(damageResult.Position, damageResult.Damage);
        }
    }
}
