using StudioScor.GameplayEffectSystem;
using StudioScor.StatSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.Effect
{
    [CreateAssetMenu(menuName = "Project/Duet/GameplayEffect/new Take Damage Effect", fileName = "GE_TakeDamage")]
    public class TakeDamageEffect : GASGameplayEffect
    {
        public struct FElement
        {
            public Vector3 HitPoint;
            public Vector3 HitNormal;
            public Collider HitCollider;
            public Vector3 Direction;
            public GameObject DamageCauser;
            public GameObject Instigator;

            public FElement(Vector3 hitPoint, Vector3 hitNormal, Collider hitCollider, Vector3 direction, GameObject damageCauser, GameObject instigator)
            {
                HitPoint = hitPoint;
                HitNormal = hitNormal;
                HitCollider = hitCollider;
                Direction = direction;
                DamageCauser = damageCauser;
                Instigator = instigator;
            }
        }

        [Header(" Take Damage Effect ")]
        [SerializeField] private DamageType _damageType;
        [SerializeField] private StatTag _baseStat;
        [SerializeField] private float _damageRatio = 1f;

        private static IObjectPool<Spec> _pool;

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

        private Spec Create()
        {
            Log("Create Spec");

            return new Spec();
        }
        

        public class Spec : GASGameplayEffectSpec
        {
            private new TakeDamageEffect _gameplayEffect;

            private IDamageableSystem _damageableSystem;
            private IStatSystem _statSystem;

            public override void SetupSpec(GameplayEffect gameplayEffect, IGameplayEffectSystem gameplayEffectSystem, GameObject instigator, int level = 0, object data = null)
            {
                base.SetupSpec(gameplayEffect, gameplayEffectSystem, instigator, level, data);
                
                _gameplayEffect = gameplayEffect as TakeDamageEffect;
                _damageableSystem = gameObject.GetComponent<IDamageableSystem>();
                _statSystem = gameObject.GetStatSystem();
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

                if (_statSystem is not null)
                {
                    var baseStat = _statSystem.GetOrCreateValue(_gameplayEffect._baseStat);

                    float damage = baseStat.Value * _gameplayEffect._damageRatio;
                    FElement damageData = (FElement)Data;

                    _damageableSystem.ApplyPointDamage(damage, 
                                                       _gameplayEffect._damageType, 
                                                       damageData.HitPoint, 
                                                       damageData.HitNormal, 
                                                       damageData.HitCollider, 
                                                       damageData.Direction, 
                                                       damageData.DamageCauser, 
                                                       damageData.Instigator);
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
