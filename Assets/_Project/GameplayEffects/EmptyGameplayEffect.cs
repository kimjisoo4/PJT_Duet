using StudioScor.GameplayEffectSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.Effect
{
    [CreateAssetMenu(menuName = "Project/Duet/GameplayEffect/new Empty Gameplay Effect", fileName = "GE_")]
    public class EmptyGameplayEffect : GASGameplayEffect
    {
        private static IObjectPool<Spec> _pool;

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
            public override bool CanTakeEffect()
            {
                if(!base.CanTakeEffect())
                {
                    _pool.Release(this);
                    return false;
                }

                return true;
            }

            protected override void OnExitEffect()
            {
                base.OnExitEffect();

                _pool.Release(this);
            }
        }
    }
}
