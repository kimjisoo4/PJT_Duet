using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Effect/new Toggle Shader Float Property Ability", fileName = "GA_PawnAbiltiy_ToggleShaderFloat_")]
    public class ToggleShaderFloatPropertyAbility : GASAbility
    {
        [Header(" [ Toggle Shader Float Parameter Ability ] ")]
        [SerializeField] private string _propertyName = "_Alpha";
        [SerializeField] private float _fadeInTime = 0.5f;
        [SerializeField] private float _fadeOutTime = 0.5f;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly ToggleShaderFloatPropertyAbility _ability;
            private readonly ICharacter _character;
            private readonly int _propertyID;
            private readonly Timer _timer = new Timer();

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as ToggleShaderFloatPropertyAbility;

                _character = gameObject.GetComponent<ICharacter>();

                _propertyID = Shader.PropertyToID(_ability._propertyName);
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                if (_ability._fadeInTime > 0f)
                {
                    if (_timer.IsPlaying)
                    {
                        float normalizedTime = _timer.NormalizedTime;
                        _timer.EndTimer();

                        _timer.OnTimer(_ability._fadeInTime);
                        _timer.JumpTime((1f - normalizedTime) * _ability._fadeInTime);
                    }
                    else
                    {
                        _timer.OnTimer(_ability._fadeInTime);
                    }
                }
                else
                {
                    if (_timer.IsPlaying)
                        _timer.EndTimer();

                    SetPropertyValue(1f);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                if (_ability._fadeOutTime > 0f)
                {
                    if (_timer.IsPlaying)
                    {
                        float normalizedTime = _timer.NormalizedTime;
                        _timer.EndTimer();

                        _timer.OnTimer(_ability._fadeOutTime);
                        _timer.JumpTime((1f - normalizedTime) * _ability._fadeOutTime);
                    }
                    else
                    {
                        _timer.OnTimer(_ability._fadeOutTime);
                    }
                }
                else
                {
                    if (_timer.IsPlaying)
                        _timer.EndTimer();

                    SetPropertyValue(0f);
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                UpdateProperty(deltaTime);
            }

            private void UpdateProperty(float deltaTime)
            {
                if (!_timer.IsPlaying)
                    return;

                _timer.UpdateTimer(deltaTime);

                float value = IsPlaying ? _timer.NormalizedTime : 1 - _timer.NormalizedTime;

                SetPropertyValue(value);
            }

            private void SetPropertyValue(float value)
            {
                foreach (var material in _character.Materials)
                {
                    material.SetFloat(_propertyID, value);
                }
            }
        }
    }
}
