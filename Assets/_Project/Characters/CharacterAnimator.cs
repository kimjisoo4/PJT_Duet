﻿using StudioScor.Utilities;
using UnityEngine;
using StudioScor.MovementSystem;
using StudioScor.GameplayTagSystem;
namespace PF.PJT.Duet.Pawn
{
    public class CharacterAnimator : BaseMonoBehaviour
    {
        [Header(" [ Character Animator ] ")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float _dampTime = 0.1f;

        [Header(" Gameplay Tag ")]
        [SerializeField] private GameplayTag _stiffenTag;

        private IActor _actor;  
        private IMovementSystem _movementSystem;
        private IGameplayTagSystem _gameplayTagSystem;

        private GameObject Owner => _actor.gameObject;

        private readonly int ANIM_IS_MOVING = Animator.StringToHash("isMoving");
        private readonly int ANIM_MOVE_SPEED = Animator.StringToHash("moveSpeed");
        private readonly int ANIM_IS_STIFFEN = Animator.StringToHash("isStiffen");

        private void Awake()
        {
            _actor = gameObject.GetActor();
            _movementSystem = Owner.GetMovementSystem();
            _gameplayTagSystem = Owner.GetGameplayTagSystem();

            _movementSystem.OnStartedMovement += _movementSystem_OnStartedMovement;
            _movementSystem.OnFinishedMovement += _movementSystem_OnFinishedMovement;

            _gameplayTagSystem.OnGrantedOwnedTag += _gameplayTagSystem_OnGrantedOwnedTag;
            _gameplayTagSystem.OnRemovedOwnedTag += _gameplayTagSystem_OnRemovedOwnedTag;
        }
        private void OnDestroy()
        {
            if (_movementSystem is not null)
            {
                _movementSystem.OnStartedMovement -= _movementSystem_OnStartedMovement;
                _movementSystem.OnFinishedMovement -= _movementSystem_OnFinishedMovement;
            }

            if (_gameplayTagSystem is not null)
            {
                _gameplayTagSystem.OnGrantedOwnedTag -= _gameplayTagSystem_OnGrantedOwnedTag;
                _gameplayTagSystem.OnRemovedOwnedTag -= _gameplayTagSystem_OnRemovedOwnedTag;
            }
        }

        private void _gameplayTagSystem_OnGrantedOwnedTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag)
        {
            if (gameplayTag == _stiffenTag)
            {
                _animator.SetBool(ANIM_IS_STIFFEN, true);
            }
        }
        private void _gameplayTagSystem_OnRemovedOwnedTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag)
        {
            if(gameplayTag == _stiffenTag)
            {
                _animator.SetBool(ANIM_IS_STIFFEN, false);
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

        private void OnEnable()
        {
            _animator.SetBool(ANIM_IS_MOVING, _movementSystem.IsMoving);
            _animator.SetBool(ANIM_IS_STIFFEN, _gameplayTagSystem.ContainOwnedTag(_stiffenTag));
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            float horizontalSpeed = _movementSystem.PrevSpeed;

            _animator.SetFloat(ANIM_MOVE_SPEED, horizontalSpeed, _dampTime, deltaTime);
        }
    }
}