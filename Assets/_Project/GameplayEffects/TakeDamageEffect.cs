using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.StatSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.Effect
{
    [CreateAssetMenu(menuName = "Project/Duet/GameplayEffect/new Take Damage Effect", fileName = "GE_TakeDamage")]
    public class TakeDamageEffect : GASGameplayEffect, IDisplayDamage
    {
        public class Element
        {
            private Vector3 _hitPoint;
            private Vector3 _hitNormal;
            private Collider _hitCollider;
            private Vector3 _direction;
            private GameObject _damageCauser;
            private GameObject _instigator;

            public Vector3 HitPoint => _hitPoint;
            public Vector3 HitNormal => _hitNormal;
            public Collider HitCollider => _hitCollider;
            public Vector3 Direction => _direction;
            public GameObject DamageCauser => _damageCauser;
            public GameObject Instigator => _instigator;

            private static IObjectPool<Element> _pool;

            public Element()
            { }

            public static Element Get(Vector3 hitPoint, Vector3 hitNormal, Collider hitCollider, Vector3 direction, GameObject damageCauser, GameObject instigator)
            {
                if(_pool is null)
                {
                    _pool = new ObjectPool<Element>(Create);
                }

                var element = _pool.Get();

                element._hitPoint = hitPoint;
                element._hitNormal = hitNormal;
                element._hitCollider = hitCollider;
                element._direction = direction;
                element._damageCauser = damageCauser;
                element._instigator = instigator;

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

        [Header(" [ Take Damage Effect ] ")]
        [SerializeField] private DamageType _damageType;
        [SerializeField] private StatTag _baseStat;
        [SerializeField] private float _damageRatio = 1f;
        [SerializeField] private bool _isAdditionalDamage = false;

        [Header(" Gameplay Tag Trigger Event ")]
        [SerializeField] private GameplayTag _onAttackHitTag;

        private static IObjectPool<Spec> _pool;
        public StatTag BaseStat => _baseStat;
        public float DamageRatio => _damageRatio;
        public DamageType DamageType => _damageType;
        public string Damage => $"<b>{_damageRatio * 100:F0}</b>";

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
            private IStatSystem _instigatorStatSystem;
            public override void SetupSpec(GameplayEffect gameplayEffect, IGameplayEffectSystem gameplayEffectSystem, GameObject instigator, int level = 0, object data = null)
            {
                base.SetupSpec(gameplayEffect, gameplayEffectSystem, instigator, level, data);
                
                _gameplayEffect = gameplayEffect as TakeDamageEffect;
                _damageableSystem = gameObject.GetComponent<IDamageableSystem>();
                _statSystem = gameObject.GetStatSystem();
                _instigatorStatSystem = instigator.GetStatSystem();
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

                if (_instigatorStatSystem is not null)
                {
                    var baseStat = _instigatorStatSystem.GetOrCreateValue(_gameplayEffect._baseStat);

                    float damage = baseStat.Value * _gameplayEffect._damageRatio;
                    Element damageData = (Element)Data;

                    var appliedDamage = _damageableSystem.ApplyPointDamage(damage, 
                                                       _gameplayEffect._damageType, 
                                                       damageData.HitPoint, 
                                                       damageData.HitNormal, 
                                                       damageData.HitCollider, 
                                                       damageData.Direction, 
                                                       damageData.DamageCauser, 
                                                       damageData.Instigator);


                    
                    if(!_gameplayEffect._isAdditionalDamage && Instigator && Instigator.TryGetGameplayTagSystem(out IGameplayTagSystem instigatorGameplayTagSystem))
                    {
                        var data = OnAttackHitData.CreateAttackHitData(Instigator, gameObject, damageData.Direction, damageData.HitCollider, damageData.HitPoint, damageData.HitNormal, damage, appliedDamage);

                        instigatorGameplayTagSystem.TriggerTag(_gameplayEffect._onAttackHitTag, data);

                        data.Release();
                    }
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
