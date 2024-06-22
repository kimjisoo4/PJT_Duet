﻿#if SCOR_ENABLE_VISUALSCRIPTING
using Unity.VisualScripting;

namespace StudioScor.Utilities.VisualScripting
{

    public class DamageableMessageListener : MessageListener
    {
        private void Awake()
        {
            if (TryGetComponent(out IDamageableSystem damageable))
            {
                damageable.OnTakeAnyDamage += Damageable_TakeDamage;
                damageable.OnTakePointDamage += Damageable_TakePointDamage;
            }
        }
        private void OnDestroy()
        {
            if (TryGetComponent(out IDamageableSystem damageable))
            {
                damageable.OnTakeAnyDamage -= Damageable_TakeDamage;
                damageable.OnTakePointDamage -= Damageable_TakePointDamage;
            }
        }

        private void Damageable_TakeDamage(IDamageableSystem damageable, DamageInfoData damageInfo)
        {
            EventBus.Trigger(new EventHook(DamageableWithVisualScripting.TAKE_ANY_DAMAGE, damageable), damageInfo);
        }

        private void Damageable_TakePointDamage(IDamageableSystem damageable, DamageInfoData damageInfo)
        {
            EventBus.Trigger(new EventHook(DamageableWithVisualScripting.TAKE_POINT_DAMAGE, damageable), damageInfo);
        }
    }
}

#endif