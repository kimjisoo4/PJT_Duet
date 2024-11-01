using Cinemachine;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{

    public class PlayerDamageCameraShakeModifier : PlayerDamageEventModifier
    {
        [Header(" [ Player Damage Camera Shake Modifier ] ")]
        [SerializeField] private CinemachineImpulseSource _cinemachineImpulse;
        [SerializeField][Min(0f)] private Vector2 _shakeInRange = new Vector2(0f, 100f);
        [SerializeField][Min(0f)] private float _maxShakeStrength = 0.5f;

        public override void OnHit(DamageInfoData damageInfo)
        {
            float strength = Mathf.InverseLerp(_shakeInRange.x, _shakeInRange.y, damageInfo.AppliedDamage);
            strength = Mathf.Min(strength, _maxShakeStrength);

            Vector3 velocity = damageInfo.AttackDirection * strength;

            _cinemachineImpulse.GenerateImpulse(velocity);
        }
    }
}
