using StudioScor.GameplayEffectSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.Effect
{
    [CreateAssetMenu(menuName = "Project/Duet/GameplayEffect/new Take Radial Knockback Effect", fileName = "GE_TakeRadialKnockback")]
    public class TakeRadialKnockbackEffect : GASGameplayEffect
    {
        public class Element
        {
            private static IObjectPool<Element> _pool;

            public Vector3 Center { get; set; }

            public static Element Get(Vector3 center)
            {
                if (_pool is null)
                {
                    _pool = new ObjectPool<Element>(Create);
                }

                var element = _pool.Get();
                element.Center = center;

                return element;
            }

            private static Element Create()
            {
                return new Element();
            }
            public void Release()
            {
                _pool.Release(this);
            }
        }

        [Header(" [ Radial Knockback Effect ] ")]
        [SerializeField] private float _knockbackDistance = 5f;
        [SerializeField] private float _knockbackDuration = 1f;


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
            protected new TakeRadialKnockbackEffect _gameplayEffect;
            private IKnockbackable _knockbackable;

            public override void SetupSpec(GameplayEffect gameplayEffect, IGameplayEffectSystem gameplayEffectSystem, GameObject instigator, int level = 0, object data = null)
            {
                base.SetupSpec(gameplayEffect, gameplayEffectSystem, instigator, level, data);

                _gameplayEffect = gameplayEffect as TakeRadialKnockbackEffect;
                _knockbackable = gameObject.GetComponent<IKnockbackable>();
            }
            public override void ReleaseSpec()
            {
                base.ReleaseSpec();

                _pool.Release(this);
            }
            protected override void OnEnterEffect()
            {
                base.OnEnterEffect();

                if(_knockbackable is not null)
                {
                    var data = (Element)Data;

                    Vector3 direction = data.Center.HorizontalDirection(GameplayEffectSystem.transform);

                    _knockbackable.TakeKnockback(direction, _gameplayEffect._knockbackDistance, _gameplayEffect._knockbackDuration);
                }
            }
        }
    }
}
