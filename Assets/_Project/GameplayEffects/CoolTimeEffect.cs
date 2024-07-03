using StudioScor.GameplayEffectSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.Effect
{
    [CreateAssetMenu(menuName = "Project/Duet/GameplayEffect/new CoolTime Effect", fileName = "GE_CoolTime_")]
    public class CoolTimeEffect : GASGameplayEffect
    {
        private static IObjectPool<Spec> _pool;

        private void Reset()
        {
            _effectType = EGameplayEffectType.Duration;
            _duration = 5f;
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
            public float NormalizedTime { get; private set; }

            public override bool CanTakeEffect()
            {
                if (!base.CanTakeEffect())
                {
                    _pool.Release(this);
                    return false;
                }

                return true;
            }

            protected override void OnEnterEffect()
            {
                base.OnEnterEffect();

                NormalizedTime = 0f;
            }
            protected override void OnExitEffect()
            {
                base.OnExitEffect();

                NormalizedTime = 1f;

                _pool.Release(this);
            }
            protected override void OnUpdateEffect(float deltaTime)
            {
                base.OnUpdateEffect(deltaTime);

                NormalizedTime = 1f - (_remainTime / _gameplayEffect.Duration);
            }
        }
    }
}
