using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class TryComboAbility : PlayerSystemAction
    {
        public enum EAbilityResult
        {
            None,
            Played,
            Failed,
            Canceled,
            Finished,
        }

        [Header(" [ Combo Ability ]")]
        [SerializeField] private Ability _comboAbility;
        [SerializeField] private int _maxComboCount = 3;

        [Header(" Target ")]
        [SerializeField][RequiredField] private SharedTransform _targetKey;
        [SerializeField] private SharedFloat _distance = 5f;
        
        private IAbilitySystem _abilitySystem;
        private IAbilitySpec _abilitySpec;
        private EAbilityResult _abilityResult = EAbilityResult.None;
        private int _currentComboCount = 0;

        public override void OnStart()
        {
            base.OnStart();

            _abilitySystem = Pawn.gameObject.GetAbilitySystem();

            var ability = _abilitySystem.TryActivateAbility(_comboAbility);

            if(ability.isActivate)
            {
                _abilityResult = EAbilityResult.Played;
                _abilitySpec = ability.abilitySpec;
                _currentComboCount = 1;

                _abilitySpec.OnFinishedAbility += AbilitySpec_OnFinishedAbility;
                _abilitySpec.OnCanceledAbility += AbilitySpec_OnCanceledAbility;
            }
            else
            {
                _abilityResult = EAbilityResult.Failed;
                _abilitySpec = null;
                _currentComboCount = -1;
            }
        }
        public override void OnEnd()
        {
            base.OnEnd();

            _abilityResult = EAbilityResult.None;

            if (_abilitySpec is not null)
            {
                _abilitySpec.OnFinishedAbility -= AbilitySpec_OnFinishedAbility;
                _abilitySpec.OnCanceledAbility -= AbilitySpec_OnCanceledAbility;

                _abilitySpec = null;
            }
        }
        private void AbilitySpec_OnFinishedAbility(IAbilitySpec abilitySpec)
        {
            _abilityResult = EAbilityResult.Finished;
        }
        private void AbilitySpec_OnCanceledAbility(IAbilitySpec abilitySpec)
        {
            _abilityResult = EAbilityResult.Canceled;
        }

        public override TaskStatus OnUpdate()
        {
            switch (_abilityResult)
            {
                case EAbilityResult.None:
                case EAbilityResult.Failed:
                case EAbilityResult.Canceled:
                    return TaskStatus.Failure;
                case EAbilityResult.Played:
                    if(_currentComboCount < _maxComboCount)
                    {
                        var target = _targetKey.Value;

                        if (target)
                        {
                            float distance = Pawn.transform.HorizontalDistance(target);

                            if (distance <= _distance.Value)
                            {
                                if (_abilitySpec.TryActiveAbility())
                                {
                                    _currentComboCount++;
                                }
                            }
                        }
                    }
                    return TaskStatus.Running;
                case EAbilityResult.Finished:
                    return TaskStatus.Success;
                default:
                    return TaskStatus.Failure;
            }
        }
    }
}

