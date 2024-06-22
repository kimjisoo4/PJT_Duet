using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;
using StudioScor.MovementSystem;
using StudioScor.RotationSystem;
using StudioScor.AbilitySystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
}
namespace PF.PJT.Duet.Pawn
{
    public interface IKnockbackable
    {
        public delegate void TakeKnockbackEventHandler(IKnockbackable knockbackable, Vector3 direction, float distance, float duration);
        public void TakeKnockBack(Vector3 direction, float distance, float duration);

        public event TakeKnockbackEventHandler OnTakeKnockback;
    }
    public interface ICharacter
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }

        public GameObject Model { get; }

        public bool TryLeave();
        public void Appear();

        public void Teleport(Vector3 position, Quaternion rotation);
        public void SetInputAttack(bool pressed);
        public void SetInputDash(bool pressed);
    }

    public class Character : BaseMonoBehaviour, ICharacter, IKnockbackable
    {
        [Header(" [ Character ] ")]
        [SerializeField] private GameObject _model;

        [SerializeField] private Ability _attackAbility;
        [SerializeField] private Ability _dashAbility;

        [SerializeField] private GameplayTag _inputAttackTag;
        [SerializeField] private GameplayTag _inputDashTag;

        [Header(" [ Change Character ] ")]
        [SerializeField] private Ability _appearAbility;
        [SerializeField] private Ability _leaveAbility;

        private IAbilitySystem _abilitySystem;
        private IPawnSystem _pawnSystem;
        private IGameplayEffectSystem _gameplayEffectSystem;
        private IGameplayTagSystem _gameplayTagSystem;
        private IMovementSystem _movementSystem;
        private IRotationSystem _rotationSystem;
        private IGroundChecker _groundChecker;
        private IDilationSystem _dilationSystem;

        public GameObject Model => _model;
        public event IKnockbackable.TakeKnockbackEventHandler OnTakeKnockback;

        private AbilityInputBuffer _inputBuffer = new();


        private void Awake()
        {
            InitCharacter();
        }
        private void Start()
        {
            _abilitySystem.TryGrantAbility(_attackAbility, 0);
            _abilitySystem.TryGrantAbility(_dashAbility, 0);
            _abilitySystem.TryGrantAbility(_appearAbility, 0);

        }
        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _inputBuffer.UpdateBuffer(deltaTime);

            _abilitySystem.Tick(deltaTime * _dilationSystem.Speed);
            _gameplayEffectSystem.Tick(deltaTime * _dilationSystem.Speed);

            _movementSystem.UpdateMovement(deltaTime);
        }
        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            _groundChecker.CheckGrounded();

            _movementSystem.SetGrounded(_groundChecker.IsGrounded);
            _movementSystem.SetGroundState(_groundChecker.Point, _groundChecker.Normal, _groundChecker.Distance);

            _abilitySystem.FixedTick(deltaTime * _dilationSystem.Speed);
        }
        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            _rotationSystem.UpdateRotation(deltaTime);
        }

        private void InitCharacter()
        {
            _pawnSystem = gameObject.GetPawnSystem();
            _gameplayTagSystem = gameObject.GetGameplayTagSystem();
            _abilitySystem = gameObject.GetAbilitySystem();
            _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
            _movementSystem = gameObject.GetMovementSystem();
            _rotationSystem = gameObject.GetRotationSystem();
            _groundChecker = gameObject.GetGroundChecker();
            _dilationSystem = gameObject.GetDilationSystem();

            _inputBuffer.SetAbilitySystem(_abilitySystem);
        }

        public bool TryLeave()
        {
            if (_abilitySystem.TryActivateAbility(_leaveAbility).isActivate)
            {
                Log("Leave");

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Appear()
        {
            Log("Appear");

            _abilitySystem.CancelAbility(_leaveAbility);

            _abilitySystem.TryActivateAbility(_appearAbility);
        }

        public void SetInputAttack(bool pressed)
        {
            if (pressed)
            {
                _gameplayTagSystem.AddOwnedTag(_inputAttackTag);

                if (!_abilitySystem.TryActivateAbility(_attackAbility).isActivate)
                {
                    _inputBuffer.SetBuffer(_attackAbility);
                }
            }
            else
            {
                _gameplayTagSystem.RemoveOwnedTag(_inputAttackTag);

                _abilitySystem.ReleasedAbility(_attackAbility);
            }
        }

        public void SetInputDash(bool pressed)
        {
            if(pressed)
            {
                _gameplayTagSystem.AddOwnedTag(_inputDashTag);

                if (!_abilitySystem.TryActivateAbility(_dashAbility).isActivate)
                {
                    _inputBuffer.SetBuffer(_dashAbility);
                }
            }
            else
            {
                _abilitySystem.ReleasedAbility(_dashAbility);

                _gameplayTagSystem.RemoveOwnedTag(_inputDashTag);
            }
        }

        public void TakeKnockBack(Vector3 direction, float distance, float duration)
        {
            OnTakeKnockback?.Invoke(this, direction, distance, duration);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _movementSystem.Teleport(position);
            _rotationSystem.SetRotation(rotation);
        }
    }
}
