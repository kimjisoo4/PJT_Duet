using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace PF.PJT.Duet
{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Try Combo Ability", story: "[Self] Try [ComboAbility] Ability On [Target] ", category: "Action/Duet", id: "68c1c72aa320e90d4d01effbfce09e80")]
    public partial class TryComboAbilityAction : Action
    {
        public enum EAbilityResult
        {
            None,
            Played,
            Failed,
            Canceled,
            Finished,
        }

        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<Ability> ComboAbility;
        [SerializeReference] public BlackboardVariable<int> MaxComboCount = new(3);
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> Distance = new(5f);

        private IAbilitySystem _abilitySystem;
        private IAbilitySpec _abilitySpec;
        private EAbilityResult _abilityResult = EAbilityResult.None;
        private int _currentComboCount = 0;

        protected override Status OnStart()
        {
            var self = Self.Value;

            if (!self)
                return Status.Failure;

            _abilitySystem = self.GetAbilitySystem();

            if (_abilitySystem.TryActivateAbility(ComboAbility.Value, out _abilitySpec))
            {
                _abilityResult = EAbilityResult.Played;
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

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            switch (_abilityResult)
            {
                case EAbilityResult.None:
                case EAbilityResult.Failed:
                case EAbilityResult.Canceled:
                    return Status.Failure;
                case EAbilityResult.Played:
                    if (_currentComboCount < MaxComboCount.Value)
                    {
                        var target = Target.Value;

                        if (target)
                        {
                            float distance = Self.Value.transform.HorizontalDistance(target.transform);

                            if (distance <= Distance.Value)
                            {
                                if (_abilitySpec.TryActiveAbility())
                                {
                                    _currentComboCount++;
                                }
                            }
                        }
                    }
                    return Status.Running;
                case EAbilityResult.Finished:
                    return Status.Success;
                default:
                    return Status.Failure;

            }
        }

        protected override void OnEnd()
        {
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
    }


}
