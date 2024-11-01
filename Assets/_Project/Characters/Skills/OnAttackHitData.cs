using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public class OnAttackHitData
    {
        public OnAttackHitData() { }

        public static OnAttackHitData CreateAttackHitData(GameObject attacker, GameObject hitter, Vector3 attackDirection, Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal, float damage, float resultDamage)
        {
            if(_pool is null)
            {
                _pool = new ObjectPool<OnAttackHitData>(Create);
            }

            var data = _pool.Get();

            data._attacker = attacker;
            data._hitter = hitter;
            data._attackDirection = attackDirection;
            data._hitCollider = hitCollider;
            data._hitPoint = hitPoint;
            data._hitNormal = hitNormal;
            data._damage = damage;
            data._resultDamage = resultDamage;

            return data;
        }

        private static IObjectPool<OnAttackHitData> _pool;

        private static OnAttackHitData Create()
        {
            return new OnAttackHitData();
        }

        private GameObject _attacker;
        private GameObject _hitter;
        private Collider _hitCollider;
        private Vector3 _attackDirection;
        private Vector3 _hitPoint;
        private Vector3 _hitNormal;
        private float _damage;
        private float _resultDamage;

        public GameObject Attacker => _attacker;
        public GameObject Hitter => _hitter;
        public Vector3 AttackDirection => _attackDirection;
        public Collider HitCollider => _hitCollider;
        public Vector3 HitPoint => _hitPoint;
        public Vector3 HitNormal => _hitNormal;
        public float Damage => _damage;
        public float ResultDamage => _resultDamage;

        public void Release()
        {
            _pool.Release(this);
        }

        public override string ToString()
        {
            return $"HitData [ Attacker({_attacker}) Hitter({_hitter}) Damage({_damage:f2} ResultDaamge({_resultDamage:f2})) ]";
        }
    }
}
