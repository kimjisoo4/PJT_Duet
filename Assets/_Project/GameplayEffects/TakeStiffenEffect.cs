using StudioScor.GameplayEffectSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.Effect
{

    [CreateAssetMenu(menuName = "Project/Duet/GameplayEffect/new Take Stiffen Effect", fileName = "GE_TakeStiffen")]
    public class TakeStiffenEffect : GASGameplayEffect
    {
        private static IObjectPool<Spec> _pool;

        private void Reset()
        {
            _duration = 1f;
            _effectType = EGameplayEffectType.Duration;
        }

        public override IGameplayEffectSpec CreateSpec(IGameplayEffectSystem gameplayEffectSystem, GameObject instigator = null, int level = 0, object data = null)
        {
            if (_pool is null)
            {
                _pool = new ObjectPool<Spec>(Create);
            }

            var spec = _pool.Get();
            spec.SetupSpec(this, gameplayEffectSystem, instigator, level, data);

            return spec;
        }

        private Spec Create()
        {
            Log("Create Spec");

            return new Spec();
        }

        public class Spec : GASGameplayEffectSpec
        {
            public override void SetupSpec(GameplayEffect gameplayEffect, IGameplayEffectSystem gameplayEffectSystem, GameObject instigator, int level = 0, object data = null)
            {
                base.SetupSpec(gameplayEffect, gameplayEffectSystem, instigator, level, data);
            }

            public override void ReleaseSpec()
            {
                base.ReleaseSpec();

                _pool.Release(this);
            }
        }
    }
}
