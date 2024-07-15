using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class TryHammerWindmillAbility : PlayerSystemAction
    {
        public enum EAbilityResult
        {
            None,
            Played,
            Failed,
            Canceled,
            Finished,
        }

        [Header(" [ Hammer Windmill Ability ] ")]
        [SerializeField] private Ability _windmillAbility;
        [SerializeField] private SharedFloat _duration = 3f;

        [Header(" Target ")]
        [SerializeField][RequiredField] private SharedTransform _targetKey;

        private IAbilitySystem _abilitySystem;
        private IAbilitySpec _abilitySpec;

        private EAbilityResult _abilityResult;
        private float _remainTime = 0f;

        public override void OnStart()
        {
            base.OnStart();

            _abilitySystem = Pawn.gameObject.GetAbilitySystem();

            var ability = _abilitySystem.TryActivateAbility(_windmillAbility);

            if(ability.isActivate)
            {
                _abilityResult = EAbilityResult.Played;
                _remainTime = _duration.Value;
                _abilitySpec = ability.abilitySpec;

                _abilitySpec.OnCanceledAbility += _abilitySpec_OnCanceledAbility;
                _abilitySpec.OnFinishedAbility += _abilitySpec_OnFinishedAbility;
            }
            else
            {
                _abilityResult = EAbilityResult.Failed;
            }
        }
        public override void OnEnd()
        {
            base.OnEnd();

            ControllerSystem.SetMoveDirection(Vector3.zero, 0f);

            if (_abilitySpec is not null)
            {
                _abilitySpec.OnCanceledAbility -= _abilitySpec_OnCanceledAbility;
                _abilitySpec.OnFinishedAbility -= _abilitySpec_OnFinishedAbility;
             
                _abilitySpec = null;
            }
        }

        public override TaskStatus OnUpdate()
        {
            switch (_abilityResult)
            {
                case EAbilityResult.None:
                    break;
                case EAbilityResult.Failed:
                case EAbilityResult.Canceled:
                    return TaskStatus.Failure;
                case EAbilityResult.Played:
                    var target = _targetKey.Value;

                    if(_remainTime > 0 && target)
                    {
                        float deltaTime = Time.deltaTime;
                        _remainTime -= deltaTime;

                        Vector3 direction = Pawn.transform.HorizontalDirection(target);

                        ControllerSystem.SetMoveDirection(direction, 1f);
                    }
                    else
                    {
                        _remainTime = 0f;

                        _abilitySpec.ReleaseAbility();
                    }
                    return TaskStatus.Running;
                case EAbilityResult.Finished:
                    return TaskStatus.Success;
                default:
                    break;
            }
            return base.OnUpdate();
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

