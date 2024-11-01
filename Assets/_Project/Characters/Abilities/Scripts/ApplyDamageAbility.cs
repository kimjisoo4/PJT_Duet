using StudioScor.AbilitySystem;
using StudioScor.GameplayTagSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
   
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Effect/new Apply Damage Ability", fileName = "GA_PawnAbiltiy_ApplyDamage")]
    public class ApplyDamageAbility : GASAbility
    {
        [Header(" Apply Damage Ability ")]
        [SerializeField] private StatusTag _hpTag;
        [SerializeField] private DamageResultEvent _damageResultEvent;
        [SerializeField] private GameplayTagSO _onHitTriggerTag;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            protected new readonly ApplyDamageAbility _ability;
            private readonly IStatusSystem _statusSystem;
            private readonly IDamageableSystem _damageableSystem;
            private readonly Status _hpStatus;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as ApplyDamageAbility;
                _damageableSystem = gameObject.GetComponent<IDamageableSystem>();
                _statusSystem = gameObject.GetStatusSystem();

                _hpStatus = _statusSystem.GetOrCreateStatus(_ability._hpTag);
            }
            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();
                _damageableSystem.OnTakePointDamage += _damageableSystem_OnTakePointDamage;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if(_damageableSystem is not null)
                {
                    _damageableSystem.OnTakePointDamage -= _damageableSystem_OnTakePointDamage;
                }
                
            }

            private void _damageableSystem_OnTakePointDamage(IDamageableSystem damageable, DamageInfoData damageInfo)
            {
                if (!IsPlaying)
                    return;

                float damage = damageInfo.Damage;

                _hpStatus.SubtractCurrentValue(damage);

                damageInfo.AppliedDamage = damage;

                _ability._damageResultEvent.Invoke(new FDamageResult(damage, transform.position, damageInfo.Type));

                GameplayTagSystem.TriggerTag(_ability._onHitTriggerTag, damageInfo);
            }
        }
    }
}
