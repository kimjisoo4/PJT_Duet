using StudioScor.GameplayEffectSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.Effect
{
    [CreateAssetMenu(menuName = "Project/Duet/GameplayEffect/new Take Knockback Effect", fileName = "GE_TakeKnockback")]
    public class TakeKnockbackEffect : GASGameplayEffect
    {
        [Header(" [ Take KnockBack Effect ] ")]
        [SerializeField] private float _knockbackDistance = 0.5f;
        [SerializeField] private float _knockbackDuration = 0.2f;
        [SerializeField][Range(-180f, 180f)] private float _knockbackAngle = 0f;

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
            protected new TakeKnockbackEffect _gameplayEffect;
            private IKnockbackable _knockbackable;

            public override void SetupSpec(GameplayEffect gameplayEffect, IGameplayEffectSystem gameplayEffectSystem, GameObject instigator, int level = 0, object data = null)
            {
                base.SetupSpec(gameplayEffect, gameplayEffectSystem, instigator, level, data);

                _gameplayEffect = gameplayEffect as TakeKnockbackEffect;
                _knockbackable = gameObject.GetComponent<IKnockbackable>();
            }

            public override bool CanTakeEffect()
            {
                if(!base.CanTakeEffect())
                {
                    _pool.Release(this);

                    return false;
                }

                return true;
            }
            protected override void OnEnterEffect()
            {
                base.OnEnterEffect();

                if(_knockbackable is not null)
                {
                    Vector3 direction = Quaternion.Euler(0, Instigator.transform.eulerAngles.y + _gameplayEffect._knockbackAngle, 0) * Vector3.forward;
                    float distance = _gameplayEffect._knockbackDistance;
                    float duration = _gameplayEffect._knockbackDuration;

                    _knockbackable.TakeKnockBack(direction, distance, duration);
                }
            }
            protected override void OnExitEffect()
            {
                base.OnExitEffect();

                _pool.Release(this);
            }
        }
    }
}
