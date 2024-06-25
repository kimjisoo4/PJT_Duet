using System.Collections.Generic;
using UnityEngine;


namespace StudioScor.Utilities
{

    [AddComponentMenu("StudioScor/Utilities/SimpleAmount/Simple Amount Component", order: 0)]
    public class SimpleAmountComponent : BaseMonoBehaviour, ISimpleAmount
    {
        [Header(" [ Simple Amount ] ")]
        [SerializeField] private float _currentValue;
        [SerializeField] private float _maxValue;

        private readonly List<ISimpleAmountModifier> modifiers = new();

        private bool needUpdate;
        private float normalizedValue;
        private float prevValue;

        public float PrevValue => prevValue;
        public float CurrentValue => _currentValue;
        public float MaxValue => _maxValue;
        public float NormalizedValue => normalizedValue;

        public void AddModifier(ISimpleAmountModifier modifier)
        {
            if(!modifiers.Contains(modifier))
            {
                modifiers.Add(modifier);
            }
        }
        public void RemoveModifier(ISimpleAmountModifier modifier)
        {
            modifiers.Remove(modifier);
        }

        public void SetMaxValue(float maxValue)
        {
            this._maxValue = maxValue;

            normalizedValue = _currentValue / this._maxValue;

            needUpdate = true;
        }
        public void SetCurrentValue(float currentValue)
        {
            prevValue = this._currentValue;
            this._currentValue = currentValue;

            normalizedValue = this._currentValue / _maxValue;

            needUpdate = true;
        }
        public void SetValue(float currentValue, float maxValue)
        {
            prevValue = this._currentValue;
            this._currentValue = currentValue;
            this._maxValue = maxValue;

            normalizedValue = this._currentValue.SafeDivide(this._maxValue);

            needUpdate = true;
        }

        private void LateUpdate()
        {
            if (!needUpdate)
                return;

            needUpdate = false;

            UpdateAmount();
        }

        public void UpdateAmount()
        {
            foreach (var modifier in modifiers)
            {
                Log($"{nameof(UpdateAmount)} :: {modifier}");

                modifier.UpdateModifier();
            }
        }
    }

}