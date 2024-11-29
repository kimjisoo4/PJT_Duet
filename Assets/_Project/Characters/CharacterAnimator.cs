using StudioScor.GameplayTagSystem;
using StudioScor.MovementSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn
{
    public class CharacterAnimator : BaseMonoBehaviour
    {
        [Header(" [ Character Animator ] ")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float _dampTime = 0.1f;

        [Header(" Gameplay Tag ")]
        [SerializeField] private GameplayTag _stiffenTag;
        [SerializeField] private GameplayTag _groggyTag;

        private IActor _actor;  
        private IMovementSystem _movementSystem;
        private IGameplayTagSystem _gameplayTagSystem;

        private GameObject Owner => _actor.gameObject;

        private readonly int ANIM_IS_MOVING = Animator.StringToHash("isMoving");
        private readonly int ANIM_IS_FALLING = Animator.StringToHash("isFalling");
        private readonly int ANIM_MOVE_SPEED = Animator.StringToHash("moveSpeed");
        private readonly int ANIM_SPEED_X = Animator.StringToHash("speedX");
        private readonly int ANIM_SPEED_Z = Animator.StringToHash("speedZ");

        private readonly int ANIM_IS_STIFFEN = Animator.StringToHash("isStiffen");
        private readonly int ANIM_IS_GROGGY = Animator.StringToHash("isGroggy");

        private void Awake()
        {
            _actor = gameObject.GetActor();
            _movementSystem = Owner.GetMovementSystem();
            _gameplayTagSystem = Owner.GetGameplayTagSystem();

            _movementSystem.OnStartedMovement += _movementSystem_OnStartedMovement;
            _movementSystem.OnFinishedMovement += _movementSystem_OnFinishedMovement;

            _movementSystem.OnJumped += _movementSystem_OnJumped;
            _movementSystem.OnLanded += _movementSystem_OnLanded;

            _gameplayTagSystem.OnGrantedOwnedTag += _gameplayTagSystem_OnGrantedOwnedTag;
            _gameplayTagSystem.OnRemovedOwnedTag += _gameplayTagSystem_OnRemovedOwnedTag;
        }

        private void OnDestroy()
        {
            if (_movementSystem is not null)
            {
                _movementSystem.OnStartedMovement -= _movementSystem_OnStartedMovement;
                _movementSystem.OnFinishedMovement -= _movementSystem_OnFinishedMovement;

                _movementSystem.OnJumped -= _movementSystem_OnJumped;
                _movementSystem.OnLanded -= _movementSystem_OnLanded;
            }

            if (_gameplayTagSystem is not null)
            {
                _gameplayTagSystem.OnGrantedOwnedTag -= _gameplayTagSystem_OnGrantedOwnedTag;
                _gameplayTagSystem.OnRemovedOwnedTag -= _gameplayTagSystem_OnRemovedOwnedTag;
            }
        }

        private void OnEnable()
        {
            _animator.SetBool(ANIM_IS_MOVING, _movementSystem.IsMoving);
            _animator.SetBool(ANIM_IS_STIFFEN, _gameplayTagSystem.ContainOwnedTag(_stiffenTag));
            _animator.SetBool(ANIM_IS_GROGGY, _gameplayTagSystem.ContainOwnedTag(_groggyTag));
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            float horizontalSpeed = _movementSystem.PrevSpeed;

            Vector3 velocity = _movementSystem.PrevVelocityXZ;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);

            _animator.SetFloat(ANIM_SPEED_X, localVelocity.x, _dampTime, deltaTime);
            _animator.SetFloat(ANIM_SPEED_Z, localVelocity.z, _dampTime, deltaTime);
            _animator.SetFloat(ANIM_MOVE_SPEED, horizontalSpeed, _dampTime, deltaTime);
        }


        private void _gameplayTagSystem_OnGrantedOwnedTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag)
        {
            if (_stiffenTag == gameplayTag)
            {
                _animator.SetBool(ANIM_IS_STIFFEN, true);
            }
            else if (_groggyTag == gameplayTag)
            {
                _animator.SetBool(ANIM_IS_GROGGY, true);
            }
        }
        private void _gameplayTagSystem_OnRemovedOwnedTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag)
        {
            if(_stiffenTag == gameplayTag)
            {
                _animator.SetBool(ANIM_IS_STIFFEN, false);
            }
            else if (_groggyTag == gameplayTag)
            {
                _animator.SetBool(ANIM_IS_GROGGY, false);
            }
        }

        private void _movementSystem_OnStartedMovement(IMovementSystem movementSystem)
        {
            _animator.SetBool(ANIM_IS_MOVING, true);
        }
        private void _movementSystem_OnFinishedMovement(IMovementSystem movementSystem)
        {
            _animator.SetBool(ANIM_IS_MOVING, false);
        }

        private void _movementSystem_OnJumped(IMovementSystem movementSystem)
        {
            _animator.SetBool(ANIM_IS_FALLING, true);
        }
        private void _movementSystem_OnLanded(IMovementSystem movementSystem)
        {
            _animator.SetBool(ANIM_IS_FALLING, false);
        }
    }
}
