using StudioScor.AbilitySystem;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;

namespace PF.PJT.Duet
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Try Hammer Windmill Ability", story: "[Pawn] Try [WindmillAbility] Ability On [Target]", category: "Action", id: "bbe2b67e73f41fc12f24fe2218f5cb11")]
    public partial class TryHammerWindmillAbilityAction : Action
    {
        public enum EAbilityResult
        {
            None,
            Played,
            Failed,
            Canceled,
            Finished,
        }

        [SerializeReference] public BlackboardVariable<GameObject> Pawn;
        [SerializeReference] public BlackboardVariable<Ability> WindmillAbility;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> _duration = new(3f);

        private IAbilitySystem _abilitySystem;
        private IAbilitySpec _abilitySpec;

        private IControllerSystem _controllerSystem;

        private EAbilityResult _abilityResult;
        private float _remainTime = 0f;

        protected override Status OnStart()
        {
            base.OnStart();

            var actor = Pawn.Value;

            _abilitySystem = actor.GetAbilitySystem();
            _controllerSystem = actor.GetController();

            if (_abilitySystem.TryActivateAbility(WindmillAbility.Value, out _abilitySpec))
            {
                _abilityResult = EAbilityResult.Played;
                _remainTime = _duration.Value;

                _abilitySpec.OnCanceledAbility += _abilitySpec_OnCanceledAbility;
                _abilitySpec.OnFinishedAbility += _abilitySpec_OnFinishedAbility;
            }
            else
            {
                _abilityResult = EAbilityResult.Failed;
            }

            return Status.Running;
        }
        protected override void OnEnd()
        {
            if(_controllerSystem is not null)
            {
                _controllerSystem.SetMoveDirection(Vector3.zero, 0f);
            }

            if (_abilitySpec is not null)
            {
                _abilitySpec.OnCanceledAbility -= _abilitySpec_OnCanceledAbility;
                _abilitySpec.OnFinishedAbility -= _abilitySpec_OnFinishedAbility;

                _abilitySpec = null;
            }
        }
        protected override Status OnUpdate()
        {
            switch (_abilityResult)
            {
                case EAbilityResult.None:
                    break;
                case EAbilityResult.Failed:
                case EAbilityResult.Canceled:
                    return Status.Failure;
                case EAbilityResult.Played:
                    var target = Target.Value;

                    if (_remainTime > 0 && target)
                    {
                        float deltaTime = Time.deltaTime;
                        _remainTime -= deltaTime;

                        Vector3 direction = Pawn.Value.transform.HorizontalDirection(target.transform);

                        _controllerSystem.SetMoveDirection(direction, 1f);
                    }
                    else
                    {
                        _remainTime = 0f;

                        _abilitySpec.ReleaseAbility();
                    }
                    return Status.Running;
                case EAbilityResult.Finished:
                    return Status.Success;
                default:
                    break;
            }

            return Status.Running;
        }
        private void _abilitySpec_OnFinishedAbility(IAbilitySpec abilitySpec)
        {
            _abilityResult = EAbilityResult.Finished;
        }

        private void _abilitySpec_OnCanceledAbility(IAbilitySpec abilitySpec)
        {
            _abilityResult = EAbilityResult.Canceled;
        }
    }
}