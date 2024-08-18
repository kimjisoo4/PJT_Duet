using StudioScor.AbilitySystem;
using StudioScor.GameplayTagSystem;
using StudioScor.StatSystem;
using StudioScor.StatusSystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/State/new Attribute Ability", fileName = "GA_PawnAbiltiy_Attribute")]
    public class AttributeAbility : GASAbility
    {
        [Header(" [ Stat State Post Ability ] ")]
        [SerializeField] private StatTag _statTag;
        [SerializeField] private StatusTag _statusTag;
        [SerializeField][Range(0f, 1f)] private float _defaultRateValue = 1f;

        [Header(" Reset Status ")]
        [SerializeField] private GameplayTag _resetTriggerTag;

        [Header(" State Tag ")]
        [SerializeField] private GameplayTag _fulledTag;
        [SerializeField] private GameplayTag _consumedTag;
        [SerializeField] private GameplayTag _emptiedTag;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec
        {
            protected new readonly AttributeAbility _ability;
            private readonly IStatSystem _statSystem;
            private readonly IStatusSystem _statusSystem;
            private readonly Stat _stat;
            private readonly Status _status;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AttributeAbility;

                _statSystem = gameObject.GetStatSystem();
                _statusSystem = gameObject.GetStatusSystem();

                _stat = _statSystem.GetOrCreateValue(_ability._statTag);
                _status = _statusSystem.GetOrCreateStatus(_ability._statusTag, _stat.Value);
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                AddOwnedTag(_status.CurrentState); 

                GameplayTagSystem.OnTriggeredTag += GameplayTagSystem_OnTriggeredTag;
                _status.OnChangedState += _status_OnChangedState;
                _stat.OnChangedValue += _stat_OnChangedValue;

                ForceActiveAbility();
            }

            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if(GameplayTagSystem is not null)
                {
                    GameplayTagSystem.OnTriggeredTag -= GameplayTagSystem_OnTriggeredTag;
                }

                if (_status is not null)
                {
                    _status.OnChangedState -= _status_OnChangedState;

                    RemoveOwnedTag(_status.CurrentState);
                }

                if (_stat is not null)
                {
                    _stat.OnChangedValue -= _stat_OnChangedValue;
                }
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _status.SetValue(_stat.Value, _ability._defaultRateValue, true);
            }

            private void AddOwnedTag(EStatusState state)
            {
                switch (state)
                {
                    case EStatusState.None:
                        break;
                    case EStatusState.Fulled:
                        GameplayTagSystem.AddOwnedTag(_ability._fulledTag);
                        break;
                    case EStatusState.Emptied:
                        GameplayTagSystem.AddOwnedTag(_ability._emptiedTag);
                        break;
                    case EStatusState.Consumed:
                        GameplayTagSystem.AddOwnedTag(_ability._consumedTag);
                        break;
                    default:
                        break;
                }
            }
            private void RemoveOwnedTag(EStatusState state)
            {
                switch (state)
                {
                    case EStatusState.None:
                        break;
                    case EStatusState.Fulled:
                        GameplayTagSystem.RemoveOwnedTag(_ability._fulledTag);
                        break;
                    case EStatusState.Emptied:
                        GameplayTagSystem.RemoveOwnedTag(_ability._emptiedTag);
                        break;
                    case EStatusState.Consumed:
                        GameplayTagSystem.RemoveOwnedTag(_ability._consumedTag);
                        break;
                    default:
                        break;
                }
            }
            private void GameplayTagSystem_OnTriggeredTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag, object data = null)
            {
                if (gameplayTag != _ability._resetTriggerTag)
                    return;

                _status.SetValue(_stat.Value, _ability._defaultRateValue, true);

            }

            private void _stat_OnChangedValue(Stat stat, float currentValue, float prevValue)
            {
                _status.SetMaxValue(currentValue, false, true);
            }

            private void _status_OnChangedState(Status status, EStatusState currentState, EStatusState prevState)
            {
                RemoveOwnedTag(prevState);
                AddOwnedTag(currentState);
            }
        }
    }
}
