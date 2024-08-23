using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public struct FDamageResult
    {
        [SerializeField] private float _damage;
        [SerializeField] private Vector3 _position;
        [SerializeField] private DamageType _damageType;

        public readonly float Damage => _damage;
        public readonly Vector3 Position => _position;
        public readonly DamageType DamageType => _damageType;

        public FDamageResult(float damage, Vector3 position, DamageType damageType)
        {
            _damage = damage;
            _position = position;
            _damageType = damageType;
        }
    }


    [CreateAssetMenu(menuName = "Project/Duet/new Damage Result Event", fileName = "Event_DamageResult")]
    public class DamageResultEvent : GenericGameEvent<FDamageResult>
    {
    }
}
