using StudioScor.GameplayEffectSystem;
using StudioScor.StatSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.Effect
{
    [CreateAssetMenu(menuName = "Project/Duet/GameplayEffect/new Stat Buff Effect", fileName = "GE_StatBuff_")]
    public class StatBuffEffect : GASGameplayEffect
    {
        [Header(" [ Stat Buff Effect ] ")]
        [SerializeField] private StatTag _statTag;
        [SerializeField] private EStatModifierType _modifierType;
        [SerializeField] private float _value = 1f;
        [SerializeField] private int _order;

        private static ObjectPool<Spec> _pool;

        public override IGameplayEffectSpec CreateSpec(IGameplayEffectSystem gameplayEffectSystem, GameObject instigator = null, int level = 0, object data = null)
        {
            if(_pool is null)
            {
                _pool = new ObjectPool<Spec>(Create);
            }

            var spec = _pool.Get();

            spec.SetupSpec(this, gameplayEffectSystem, instigator, level, data);

            return spec;
        }

        private static Spec Create()
        {
            return new Spec();
        }

        public class Spec : GASGameplayEffectSpec
        {
            protected new StatBuffEffect _gameplayEffect;
            private Stat _stat;

            private StatModifier _statModifier;

            public override void SetupSpec(GameplayEffect gameplayEffect, IGameplayEffectSystem gameplayEffectSystem, GameObject instigator, int level = 0, object data = null)
            {
                base.SetupSpec(gameplayEffect, gameplayEffectSystem, instigator, level, data);

                _gameplayEffect = gameplayEffect as StatBuffEffect;

                gameObject.TryGetStat(_gameplayEffect._statTag, out _stat);
            }
            public override bool CanTakeEffect()
            {
                return base.CanTakeEffect() && _stat is not null;
            }

            public override void ReleaseSpec()
            {
                base.ReleaseSpec();

                _pool.Release(this);
            }

            protected override void OnEnterEffect()
            {
                base.OnEnterEffect();

                if(_statModifier is null)
                {
                    _statModifier = new StatModifier();
                }

                _statModifier.Setup(_gameplayEffect._value, _gameplayEffect._modifierType, _gameplayEffect._order);

                _stat.AddModifier(_statModifier);
            }

            protected override void OnExitEffect()
            {
                base.OnExitEffect();

                if(_stat is not null)
                {
                    _stat.RemoveModifier(_statModifier);
                }
            }
        }
    }
}
