using StudioScor.Utilities;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace PF.PJT.Duet
{
    public class FloatingDamageTextSystem : BaseMonoBehaviour
    {
        [Serializable]
        public struct FFloatingText
        {
            [SerializeField] private DamageType _damageType;
            [SerializeField] private FloatingTextContainer _floatingText;

            public DamageType DamageType => _damageType;
            public FloatingTextContainer FloatingText => _floatingText;
        }

        [Header(" [ Floating Damage Text ] ")]
        [SerializeField] private Canvas _floatingCanvas;
        [SerializeField] private DamageResultEvent _onTakeDamage;

        [Header(" Text Containers ")]
        [SerializeField] private FloatingTextContainer _defaultFloatingDamage;
        [SerializeField] private FFloatingText[] _floatingDamages;

        [Header(" Spawn ")]
        [SerializeField][Min(0f)] private float _randPosition = 0.5f;

        private readonly Dictionary<DamageType, FloatingTextContainer> _floatingDamageDic = new();
        private void Awake()
        {
            _onTakeDamage.OnTriggerGenericEvent += _onTakeDamage_OnTriggerGenericEvent;

            _defaultFloatingDamage.SetupPool(_floatingCanvas);

            foreach (var floatingText in _floatingDamages)
            {
                _floatingDamageDic.Add(floatingText.DamageType, floatingText.FloatingText);
                floatingText.FloatingText.SetupPool(_floatingCanvas);
            }
        }

        private void OnDestroy()
        {
            if(_onTakeDamage)
            {
                _onTakeDamage.OnTriggerGenericEvent -= _onTakeDamage_OnTriggerGenericEvent;
            }

            if(_defaultFloatingDamage)
            {
                _defaultFloatingDamage.Clear();
            }

            foreach (var floiatingText in _floatingDamages)
            {
                if(floiatingText.FloatingText)
                {
                    floiatingText.FloatingText.Clear();
                }
            }
        }

        private void _onTakeDamage_OnTriggerGenericEvent(FDamageResult damageResult)
        {
            if (damageResult.Damage <= 0f)
                return;

            if (!_floatingDamageDic.TryGetValue(damageResult.DamageType, out FloatingTextContainer flatingText))
            {
                flatingText = _defaultFloatingDamage;
            }

            Vector3 position = damageResult.Position;

            if(_randPosition > 0)
            {
                position += UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0f, _randPosition);
            }

            flatingText.SpawnFloatingDamage(position, damageResult.Damage);
        }
    }
}
