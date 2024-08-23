using StudioScor.AbilitySystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.MovementSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.StatSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace PF.PJT.Duet.Pawn
{
    public interface IKnockbackable
    {
        public delegate void TakeKnockbackEventHandler(IKnockbackable knockbackable, Vector3 direction, float distance, float duration);
        public void TakeKnockback(Vector3 direction, float distance, float duration);

        public event TakeKnockbackEventHandler OnTakeKnockback;
    }

    public interface ICharacter
    {
        public delegate void CharacterStateEventHandler(ICharacter character);

        public CharacterInformationData CharacterInformationData { get; }
        public GameObject gameObject { get; }
        public Transform transform { get; }

        public GameObject Model { get; }

        public IReadOnlyList<Material> Materials { get; }

        public void ResetCharacter();
        public void OnSpawn();
        public void OnDie();

        public bool IsDead { get; }

        public bool CanLeave();
        public bool CanAppear();
        public void Leave();
        public void Appear();

        public void Teleport(Vector3 position, Quaternion rotation);
        public void SetInputAttack(bool pressed);
        public void SetInputSkill(bool pressed);
        public void SetInputDash(bool pressed);

        public event CharacterStateEventHandler OnSpawned;
        public event CharacterStateEventHandler OnDead;
    }

    public class Character : BaseMonoBehaviour, ICharacter, IKnockbackable
    {
        [Header(" [ Character ] ")]
        [SerializeField] private CharacterInformationData _characterInformationData;
        [SerializeField] private GameObject _model;

        [SerializeField] private Ability _attackAbility;
        [SerializeField] private Ability _skillAbility;
        [SerializeField] private Ability _dashAbility;

        [Header(" Inputs ")]
        [SerializeField] private GameplayTag _inputAttackTag;
        [SerializeField] private GameplayTag _inputSkillTag;
        [SerializeField] private GameplayTag _inputDashTag;

        [Header(" Change Character ")]
        [SerializeField] private Ability _appearAbility;
        [SerializeField] private Ability _leaveAbility;

        [Header(" Gameplay Tag Trigger ")]
        [SerializeField] private GameplayTag _resetTriggerTag;

        [Header(" Enemy AI ")]
        [SerializeField] private SimplePoolContainer[] _enemyControllers;

        [Header(" GameEvents ")]
        [SerializeField] private CharacterGameEvent _onSpawend;
        [SerializeField] private CharacterGameEvent _onDead;

        private IAbilitySystem _abilitySystem;
        private IPawnSystem _pawnSystem;
        private IStatSystem _statSystem;
        private IStatusSystem _statusSystem;
        private IGameplayEffectSystem _gameplayEffectSystem;
        private IGameplayTagSystem _gameplayTagSystem;
        private IMovementSystem _movementSystem;
        private IRotationSystem _rotationSystem;
        private IGroundChecker _groundChecker;
        private IDilationSystem _dilationSystem;

        private bool _isDead = false;
        private List<Material> _materials;
        public bool IsDead => _isDead;

        public CharacterInformationData CharacterInformationData => _characterInformationData;
        public GameObject Model => _model;

        public IReadOnlyList<Material> Materials
        {
            get
            {
                if(_materials is null)
                {
                    _materials = new();

                    var renderers = Model.GetComponentsInChildren<Renderer>();
                    List<Material> materials = new();

                    foreach (var renderer in renderers)
                    {
                        materials.Clear();

                        renderer.GetMaterials(materials);
                        _materials.AddRange(materials);
                    }
                }

                return _materials;
            }
        }

        public event IKnockbackable.TakeKnockbackEventHandler OnTakeKnockback;
        public event ICharacter.CharacterStateEventHandler OnSpawned;
        public event ICharacter.CharacterStateEventHandler OnDead;

        private AbilityInputBuffer _inputBuffer = new();

        private void Awake()
        {
            InitCharacter();

            GrantDefaultSkill();
        }
        private void Start()
        {
            OnSpawn();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _inputBuffer.UpdateBuffer(deltaTime);

            _abilitySystem.Tick(deltaTime * _dilationSystem.Speed);
            _gameplayEffectSystem.Tick(deltaTime * _dilationSystem.Speed);

            _movementSystem.UpdateMovement(deltaTime * _dilationSystem.Speed);
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

            _rotationSystem.UpdateRotation(deltaTime * _dilationSystem.Speed);
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
            _statSystem = gameObject.GetStatSystem();
            _statusSystem = gameObject.GetStatusSystem();

            _inputBuffer.SetAbilitySystem(_abilitySystem);
        }

        private void GrantDefaultSkill()
        {
            _abilitySystem.TryGrantAbility(_attackAbility, 0);
            _abilitySystem.TryGrantAbility(_skillAbility, 0);
            _abilitySystem.TryGrantAbility(_dashAbility, 0);
            _abilitySystem.TryGrantAbility(_appearAbility, 0);
            _abilitySystem.TryGrantAbility(_leaveAbility, 0);
        }
        public void ResetCharacter()
        {
            _gameplayTagSystem.TriggerTag(_resetTriggerTag);

            _isDead = false;
        }

        public void OnSpawn()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if(!_pawnSystem.IsPossessed)
            {
                if (_enemyControllers is not null && _enemyControllers.Count() > 0)
                {
                    var controller = _enemyControllers[UnityEngine.Random.Range(0, _enemyControllers.Length)];
                    var controllerActor = controller.Get();

                    if (controllerActor.TryGetControllerSystem(out IControllerSystem controllerSystem))
                    {
                        controllerSystem.OnPossess(_pawnSystem);
                    }
                }
            }

            Invoke_OnSpawend();
        }

        public void OnDie()
        {
            if (_isDead)
                return;

            _isDead = true;

            _pawnSystem.UnPossess();

            Invoke_OnDead();
        }

        public bool CanLeave()
        {
            return !_isDead;
        }
        public bool CanAppear()
        {
            return !_isDead;
        }
        public void Leave()
        {
            Log("Leave");

            _abilitySystem.TryActivateAbility(_leaveAbility);
        }

        public void Appear()
        {
            Log("Appear");

            if(_leaveAbility)
                _abilitySystem.CancelAbility(_leaveAbility);

            _abilitySystem.TryActivateAbility(_appearAbility);
        }

        public void SetInputAttack(bool pressed)
        {
            if (pressed)
            {
                _gameplayTagSystem.AddOwnedTag(_inputAttackTag);

                if (_attackAbility)
                {
                    if (!_abilitySystem.TryActivateAbility(_attackAbility).isActivate)
                    {
                        _inputBuffer.SetBuffer(_attackAbility);
                    }
                }
            }
            else
            {
                _gameplayTagSystem.RemoveOwnedTag(_inputAttackTag);

                if (_attackAbility)
                {
                    if (_abilitySystem.IsPlayingAbility(_attackAbility))
                    {
                        _abilitySystem.ReleasedAbility(_attackAbility);
                    }
                    else
                    {
                        _inputBuffer.ReleaseBuffer(_attackAbility);
                    }
                }
            }
        }

        public void SetInputSkill(bool pressed)
        {
            if(pressed)
            {
                _gameplayTagSystem.AddOwnedTag(_inputSkillTag);

                if(_skillAbility)
                {
                    if (!_abilitySystem.TryActivateAbility(_skillAbility).isActivate)
                    {
                        _inputBuffer.SetBuffer(_skillAbility);
                    }
                }
            }
            else
            {
                _gameplayTagSystem.RemoveOwnedTag(_inputSkillTag);

                if (_skillAbility)
                {
                    if (_abilitySystem.IsPlayingAbility(_skillAbility))
                    {
                        _abilitySystem.ReleasedAbility(_skillAbility);
                    }
                    else
                    {
                        _inputBuffer.ReleaseBuffer(_skillAbility);
                    }
                }
            }
        }

        public void SetInputDash(bool pressed)
        {
            if (pressed)
            {
                _gameplayTagSystem.AddOwnedTag(_inputDashTag);

                if (_dashAbility)
                {
                    if (!_abilitySystem.TryActivateAbility(_dashAbility).isActivate)
                    {
                        _inputBuffer.SetBuffer(_dashAbility);
                    }

                }
            }
            else
            {
                _gameplayTagSystem.RemoveOwnedTag(_inputDashTag);

                if (_dashAbility)
                {
                    if (_abilitySystem.IsPlayingAbility(_dashAbility))
                    {
                        _abilitySystem.ReleasedAbility(_dashAbility);
                    }
                    else
                    {
                        _inputBuffer.ReleaseBuffer(_dashAbility);
                    }
                }
            }
        }

        public void TakeKnockback(Vector3 direction, float distance, float duration)
        {
            OnTakeKnockback?.Invoke(this, direction, distance, duration);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _movementSystem.Teleport(position, true);
            _rotationSystem.SetRotation(rotation, true);
        }

        private void Invoke_OnSpawend()
        {
            Log($"{nameof(OnSpawned)}");

            if (_onSpawend)
                _onSpawend.Invoke(this);

            OnSpawned?.Invoke(this);
        }
        private void Invoke_OnDead()
        {
            Log($"{nameof(OnDead)}");

            if (_onDead)
                _onDead.Invoke(this);

            OnDead?.Invoke(this);
        }
    }
}
