using StudioScor.Utilities;
using System;
using UnityEngine;

namespace StudioScor.MovementSystem
{

    [Serializable]
    public class ReachMovementActionTask : Task, ISubTask
    {
        [Header(" [ Reach Movement Ability Task ] ")]
#if SCOR_ENABLE_SERIALIZEREFERENCE
        [SerializeReference, SerializeReferenceDropdown]
#endif
        private IDirectionVariable _direction = new LocalDirectionVariable(Vector3.forward);

#if SCOR_ENABLE_SERIALIZEREFERENCE
        [SerializeReference, SerializeReferenceDropdown]
#endif
        private IFloatVariable _distance = new DefaultFloatVariable(5f);

        [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private bool _updatableDirection = false;
        [SerializeField] private bool _usePhysics = false;

        private Vector3 _moveDirection;
        private float _moveDistance;
        private bool _updatable;
        private bool _physics;
        private AnimationCurve _animationCurve;

        private IMovementSystem _movementSystem;
        private readonly ReachValueToTime _reachValueToTime = new();
        private ReachMovementActionTask _original;

        public override ITask Clone()
        {
            var clone = new ReachMovementActionTask();

            clone._original = this;
            clone._direction = _direction.Clone();
            clone._distance = _distance.Clone();

            return clone;
        }
        protected override void SetupTask()
        {
            base.SetupTask();

            _movementSystem = Owner.GetMovementSystem();

            _direction.Setup(Owner);
            _distance.Setup(Owner);
        }
        protected override void EnterTask()
        {
            base.EnterTask();

            _moveDirection = _direction.GetValue();
            _moveDistance = _distance.GetValue();
            
            _animationCurve = _original is null ? _curve : _original._curve;
            _updatable = _original is null ? _updatableDirection : _original._updatableDirection;
            _physics = _original is null ? _usePhysics : _original._physics;

            _reachValueToTime.OnMovement(_moveDistance, _animationCurve);
        }

        protected override void ExitTask()
        {
            base.ExitTask();

            _reachValueToTime.EndMovement();
        }
        public void UpdateSubTask(float deltaTime, float normalizedTime)
        {
            if (!IsPlaying)
                return;

            if (_physics)
                return;

            UpdateMovement(normalizedTime);
        }

        public void FixedUpdateSubTask(float deltaTime, float normalizedTime)
        {
            if (!IsPlaying)
                return;

            if (!_physics)
                return;

            UpdateMovement(normalizedTime);
        }

        private void UpdateMovement(float normalizedTime)
        {
            if (_updatable)
            {
                _moveDirection = _direction.GetValue();
            }

            float speed = _reachValueToTime.UpdateMovement(normalizedTime);

            _movementSystem.MovePosition(_moveDirection * speed);
        }
    }

}
