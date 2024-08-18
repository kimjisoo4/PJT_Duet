﻿using UnityEngine;
using StudioScor.Utilities;
namespace StudioScor.MovementSystem
{
    [AddComponentMenu("StudioScor/MovementSystem/Modifiers/RootMotion Modifier", order: 30)]
    public class RootMotionModifierComponent : MovementModifierComponent
    {
        [Header(" [ Root Motion Modifier ] ")]
        [SerializeField] private Animator _Animator;
        private Vector3 _RootPosition = Vector3.zero;

        protected override void Reset()
        {
            base.Reset();

            gameObject.TryGetComponentInParentOrChildren(out _Animator);
        }

        protected override void Awake()
        {
            if(!_Animator)
            {
                if(!gameObject.TryGetComponentInParentOrChildren(out _Animator))
                {
                    LogError("Animator Is NULL!!");
                }
            }
        }

        protected override void UpdateMovement(float deltaTime)
        {
            MovementSystem.MovePosition(_RootPosition);
        }

        private void OnAnimatorMove()
        {
            _RootPosition = _Animator.deltaPosition;
        }
    }

}