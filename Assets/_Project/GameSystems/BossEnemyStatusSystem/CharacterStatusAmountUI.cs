using PF.PJT.Duet.Pawn;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class CharacterStatusAmountUI : CharacterStateUIModifier
    {
        [Header(" [ Character Status Amount UI ] ")]
        [SerializeField] private SimpleAmountComponent _amount;
        [SerializeField] private StatusTag _statusTag;

        private Status _status;
        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_amount)
            {
                _amount = GetComponentInChildren<SimpleAmountComponent>();
            }
#endif
        }
        private void OnDestroy()
        {
            if(_status is not null)
            {
                _status.OnChangedMaxValue -= Status_OnChangedMaxValue;
                _status.OnChangedValue -= Status_OnChangedValue;

                _status = null;
            }
        }

        public override void UpdateData(ICharacter character)
        {
            if(_status is not null)
            {
                _status.OnChangedMaxValue -= Status_OnChangedMaxValue;
                _status.OnChangedValue -= Status_OnChangedValue;
                _status = null;
            }

            if(character is not null)
            {
                var statusSystem = character.gameObject.GetStatusSystem();
                _status = statusSystem.GetStatus(_statusTag);

                _amount.SetValue(_status.CurrentValue, _status.MaxValue);

                _status.OnChangedMaxValue += Status_OnChangedMaxValue;
                _status.OnChangedValue += Status_OnChangedValue;
            }
        }

        private void Status_OnChangedMaxValue(Status status, float currentValue, float prevValue)
        {
            _amount.SetMaxValue(currentValue);
        }

        private void Status_OnChangedValue(Status status, float currentValue, float prevValue)
        {
            _amount.SetCurrentValue(currentValue);
        }
    }
}
