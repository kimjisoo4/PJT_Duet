using StudioScor.AbilitySystem;
using StudioScor.GameplayTagSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Effect/new Trigger Shader Float Property Ability", fileName = "GA_PawnAbiltiy_TriggerShaderFloat_")]
    public class TriggerShaderFloatPropertyAbility : GASAbility
    {
        [Header(" [ Trigger Shader Float Property  Ability ] ")]
        [SerializeField] private GameplayTag _onTriggerTag;
        [SerializeField] private string _propertyName = "_Alpha";
        [SerializeField] private float _fadeInTime = 0.5f;
        [SerializeField] private float _fadeOutTime = 0.5f;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly TriggerShaderFloatPropertyAbility _ability;
            private readonly ICharacter _character;
            private readonly int _propertyID;
            private readonly Timer _timer = new Timer();

            private bool _isBackward;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as TriggerShaderFloatPropertyAbility;

                _character = gameObject.GetComponent<ICharacter>();

                _propertyID = Shader.PropertyToID(_ability._propertyName);
            }
            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                GameplayTagSystem.OnTriggeredTag += GameplayTagSystem_OnTriggeredTag;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if (GameplayTagSystem is not null)
                {
                    GameplayTagSystem.OnTriggeredTag -= GameplayTagSystem_OnTriggeredTag;
                }
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                if (_ability._fadeInTime > 0f)
                {
                    _isBackward = false;
                    _timer.OnTimer(_ability._fadeInTime);
                }
                else
                {
                    _isBackward = true;

                    SetPropertyValue(1f);
                    _timer.OnTimer(_ability._fadeOutTime);
                }
            }

            protected override void OnReTriggerAbility()
            {
                base.OnReTriggerAbility();

                _isBackward = false;

                if (_ability._fadeInTime > 0f)
                {
                    if (_timer.IsPlaying)
                    {
                        float normalizedTime = _timer.NormalizedTime;

                        _timer.EndTimer();

                        _timer.OnTimer(_ability._fadeInTime);
                        _timer.JumpTime(normalizedTime * _ability._fadeInTime);
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
                    _isBackward = true;

                    _timer.OnTimer(_ability._fadeOutTime);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _timer.EndTimer();
                SetPropertyValue(0f);
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                if (!IsPlaying)
                    return;

                UpdateProperty(deltaTime);
            }

            private void UpdateProperty(float deltaTime)
            {
                _timer.UpdateTimer(deltaTime);
                
                if (!_isBackward)
                {
                    // 정방향
                    SetPropertyValue(_timer.NormalizedTime);

                    if (_timer.IsFinished)
                    {
                        _isBackward = true;

                        _timer.OnTimer(_ability._fadeOutTime);
                    }
                }
                else
                {
                    // 역방향
                    SetPropertyValue(1f - _timer.NormalizedTime);

                    if (_timer.IsFinished)
                    {
                        _isBackward = false;
                        TryFinishAbility();
                    }
                }
            }

            private void SetPropertyValue(float value)
            {
                foreach (var material in _character.Materials)
                {
                    material.SetFloat(_propertyID, value);
                }
            }
            private void GameplayTagSystem_OnTriggeredTag(StudioScor.GameplayTagSystem.IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag, object data = null)
            {
                if (_ability._onTriggerTag != gameplayTag)
                    return;

                TryActiveAbility();
            }
        }
    }
}
