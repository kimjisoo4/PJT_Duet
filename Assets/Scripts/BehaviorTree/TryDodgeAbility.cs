using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class TryDodgeAbility : PlayerSystemAction
    {
        public enum EDirection
        {
            World,
            Local,
            Target,
        }
        public enum EAbilityResult
        {
            None,
            Played,
            Failed,
            Canceled,
            Finished,
        }

        [Header(" [ Dodge Ability ] ")]
        [SerializeField] private Ability _dodgeAbility;

        [SerializeField] private SharedTransform _target;
        [SerializeField] private EDirection _transformSpace = EDirection.Local;
        [SerializeField] private SharedVector3 _direction = Vector3.back;

        private IAbilitySystem _abilitySystem;
        private IAbilitySpec _abilitySpec;
        private EAbilityResult _abilityResult;

        public override void OnStart()
        {
            base.OnStart();

            _abilitySystem = Pawn.gameObject.GetAbilitySystem();


            Vector3 direction = _direction.Value;

            switch (_transformSpace)
            {
                case EDirection.Local:
                    direction = Pawn.transform.TransformDirection(direction);

                    direction.y = 0;
                    direction.Normalize();
                    break;
                case EDirection.Target:
                    var lookRotation = Quaternion.LookRotation(Pawn.transform.HorizontalDirection(_target.Value), Vector3.up);

                    direction = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0) * direction;
                    break;
                default:
                    break;
            }

            ControllerSystem.SetMoveDirection(direction, 1f);

            if(_abilitySystem.TryActivateAbility(_dodgeAbility, out _abilitySpec))
            {
                _abilityResult = EAbilityResult.Played;

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
                    return TaskStatus.Running;
                case EAbilityResult.Finished:
                    return TaskStatus.Success;
                default:
                    break;
            }

            return base.OnUpdate();
        }
        private void _abilitySpec_OnCanceledAbility(IAbilitySpec abilitySpec)
        {
            _abilityResult = EAbilityResult.Canceled;
        }

        private void _abilitySpec_OnFinishedAbility(IAbilitySpec abilitySpec)
        {
            _abilityResult = EAbilityResult.Finished;
        }
    }
}

