using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.MovementSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.StatSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PF.PJT.Duet.Pawn.ICharacter;

namespace PF.PJT.Duet.Pawn
{
    public enum ESkillSlot
    {
        None,
        Auto,       // Auto Target Slot
        Attack,     // LMB
        Dash,       // Space
        Appear,     // Tap (enabled)
        Leave,      // Tap (disabled)
        Skill_01,   // RMB
        Skill_02,   // Shift LMB
        Skill_03,   // Shift RMB
    }

    public interface IAddForceable
    {
        public delegate void AddForceEventHandler(IAddForceable addForceable, Vector3 force);

        public void AddForce(Vector3 force);

        public event AddForceEventHandler OnAddForce;
    }

    public interface IKnockbackable
    {
        public delegate void TakeKnockbackEventHandler(IKnockbackable knockbackable, Vector3 direction, float distance, float duration);
        public void TakeKnockback(Vector3 direction, float distance, float duration);

        public event TakeKnockbackEventHandler OnTakeKnockback;
    }

    public interface ICharacter
    {
        public delegate void CharacterStateEventHandler(ICharacter character);
        public delegate void ChangeSkillSlotEventHandler(ICharacter character, ESkillSlot skillSlot);
        public delegate void CharacterColliderHitEventHandler(ICharacter character, ControllerColliderHit hit);

        public CharacterInformationData CharacterInformationData { get; }
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public GameObject Model { get; }
        public bool IsDead { get; }
        public IReadOnlyList<Material> Materials { get; }
        public IReadOnlyDictionary<ESkillSlot, Ability> SlotSkills { get; }

        public void GrantSkill(ESkillSlot slot, Ability skill);
        public void ResetCharacter();
        public void OnSpawn();
        public void OnDispawn();
        public void OnDie();
        public void EndDie();


        public bool CanLeave();
        public bool CanAppear();
        public void Leave();
        public void Appear(bool useAppearSkill);

        public void Teleport(Vector3 position, Quaternion rotation);
        public void SetInputAttack(bool pressed);
        public void SetInputDash(bool pressed);
        public void SetInputSkill(int index, bool pressed);


        public event CharacterStateEventHandler OnReseted;
        public event CharacterStateEventHandler OnSpawned;
        public event CharacterStateEventHandler OnDead;
        public event CharacterStateEventHandler OnDespawned;
        public event ChangeSkillSlotEventHandler OnChangedSkillSlot;

        public event CharacterColliderHitEventHandler OnCharacterColliderHit;
    }

    public class Character : BaseMonoBehaviour, ICharacter, IKnockbackable, IAddForceable
    {
        [Header(" [ Character ] ")]
        [SerializeField] private CharacterInformationData _characterInformationData;
        [SerializeField] private PooledObject _pooledObject;
        [SerializeField] private GameObject _model;
        [SerializeField] private bool _autoSpawn = true;

        [Header(" Inputs ")]
        [SerializeField] private GameplayTag _inputAttackTag;
        [SerializeField] private GameplayTag _inputDashTag;
        [SerializeField] private GameplayTag[] _inputSkillTags;

        [Header(" Default Abilities ")]
        [SerializeField] private CharacterSkill _attackSkill;
        [SerializeField] private CharacterSkill _mainSkill;
        [SerializeField] private CharacterSkill _dashSkill; 
        [SerializeField] private CharacterSkill[] _defaultSkills;

        [Header(" Change Character ")]
        [SerializeField] private Ability _appearAbility;
        [SerializeField] private Ability _leaveAbility;

        [Header(" Enemy AI ")]
        [SerializeField] private PoolContainer[] _enemyControllers;

        [Header(" GameEvents ")]
        [SerializeField] private ToggleableUnityEvent _onReseted;
        [SerializeField] private ToggleableUnityEvent _onSpawned;
        [SerializeField] private ToggleableUnityEvent _onDespawned;
        [SerializeField] private ToggleableUnityEvent _onDead;

        private IAbilitySystem _abilitySystem;
        private IPawnSystem _pawnSystem;
        private IStatSystem _statSystem;
        private IStatusSystem _statusSystem;
        private IGameplayEffectSystem _gameplayEffectSystem;
        private IGameplayTagSystem _gameplayTagSystem;
        private IMovementSystem _movementSystem;
        private IRotationSystem _rotationSystem;
        private IDilationSystem _dilationSystem;

        private bool _isDead = false;

        private List<Material> _materials;
        private readonly Dictionary<ESkillSlot, Ability> _slotSkills = new();
        private readonly List<Ability> _grantedSkills = new();

        public bool IsDead => _isDead;
        public CharacterInformationData CharacterInformationData => _characterInformationData;
        public GameObject Model => _model;

        public IReadOnlyDictionary<ESkillSlot, Ability> SlotSkills => _slotSkills;
        public IReadOnlyList<Ability> GrantedSkills => _grantedSkills;


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
        public event IAddForceable.AddForceEventHandler OnAddForce;

        public event CharacterStateEventHandler OnReseted;
        public event CharacterStateEventHandler OnSpawned;
        public event CharacterStateEventHandler OnDead;
        public event CharacterStateEventHandler OnDespawned;
        public event ChangeSkillSlotEventHandler OnChangedSkillSlot;

        public event CharacterColliderHitEventHandler OnCharacterColliderHit;

        private readonly AbilityInputBuffer _inputBuffer = new();

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_pooledObject)
            {
                _pooledObject = GetComponentInChildren<PooledObject>();
            }
#endif
        }

        private void Awake()
        {
            InitCharacter();

            GrantDefaultSkill();
        }

        private void Start()
        {
            if(_autoSpawn)
                OnSpawn();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _inputBuffer.UpdateBuffer(deltaTime);

            _abilitySystem.Tick(deltaTime * _dilationSystem.Speed);
            _gameplayEffectSystem.Tick(deltaTime * _dilationSystem.Speed);

        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            _abilitySystem.FixedTick(deltaTime * _dilationSystem.Speed);
            _movementSystem.UpdateMovement(deltaTime * _dilationSystem.Speed);
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
            _dilationSystem = gameObject.GetDilationSystem();
            _statSystem = gameObject.GetStatSystem();
            _statusSystem = gameObject.GetStatusSystem();

            _inputBuffer.SetAbilitySystem(_abilitySystem);

            foreach (var controller in _enemyControllers)
            {
                controller.Initialization();
            }
        }

        private void GrantDefaultSkill()
        {
            GrantSkill(ESkillSlot.Attack, _attackSkill);
            GrantSkill(ESkillSlot.Skill_01, _mainSkill);
            GrantSkill(ESkillSlot.Dash, _dashSkill);
            GrantSkill(ESkillSlot.Appear, _appearAbility);
            GrantSkill(ESkillSlot.Leave, _leaveAbility);

            foreach (var ability in _defaultSkills)
            {
                GrantSkill(ESkillSlot.Auto, ability);
            }
        }

        public void ResetCharacter()
        {
            foreach (var skill in _slotSkills.Values)
            {
                _abilitySystem.RemoveAbility(skill);
            }
            foreach (var skill in _grantedSkills)
            {
                _abilitySystem.RemoveAbility(skill);
            }

            _slotSkills.Clear();
            _grantedSkills.Clear();

            Invoke_OnReseted();

            GrantDefaultSkill();

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
                        controllerSystem.Possess(_pawnSystem);
                    }
                }
            }

            Invoke_OnSpawend();
        }
        public void OnDie()
        {
            if (_isDead)
                return;
            
            Log(nameof(OnDie));

            _isDead = true;

            _pawnSystem.UnPossess();

        }
        public void EndDie()
        {
            if (!_isDead)
                return;

            Log(nameof(EndDie));

            Invoke_OnDead();
        }
       

        public void OnDispawn()
        {
            _pooledObject.Release();

            Invoke_OnDispawned();
        }

        public void GrantSkill(ESkillSlot slot, Ability skill)
        {
            if (!skill)
                return;

            Log($"{nameof(GrantSkill)} - Input : {slot} || Skill : {skill}");

            if (slot == ESkillSlot.Auto)
            {
                var characterSkill = skill as CharacterSkill;

                switch (characterSkill.SkillType)
                {
                    case ESkillType.None:
                        slot = ESkillSlot.None;
                        break;
                    case ESkillType.Attack:
                        slot = ESkillSlot.Attack;
                        break;
                    case ESkillType.Dash:
                        slot = ESkillSlot.Dash;
                        break;
                    case ESkillType.Skill:
                        if (!_slotSkills.ContainsKey(ESkillSlot.Skill_01))
                        {
                            slot = ESkillSlot.Skill_01;
                        }
                        else if(!_slotSkills.ContainsKey(ESkillSlot.Skill_02))
                        {
                            slot = ESkillSlot.Skill_02;
                        }
                        else if (!_slotSkills.ContainsKey(ESkillSlot.Skill_03))
                        {
                            slot = ESkillSlot.Skill_03;
                        }
                        else
                        {
                            Debug.Log(" Need Override Target ");
                            slot = ESkillSlot.None;
                        }
                        break;
                    case ESkillType.Appear:
                        slot = ESkillSlot.Appear;
                        break;
                    default:
                        break;
                }
            }

            if (slot != ESkillSlot.None)
            {
                if (_slotSkills.TryGetValue(slot, out Ability prevSkill))
                {
                    if (prevSkill == skill)
                        return;

                    _abilitySystem.RemoveAbility(prevSkill);
                    _abilitySystem.TryGrantAbility(skill, 0);

                    _slotSkills[slot] = skill;
                }
                else
                {
                    _abilitySystem.TryGrantAbility(skill, 0);
                    _slotSkills.Add(slot, skill);
                }
            }
            else
            {
                _abilitySystem.TryGrantAbility(skill, 0);
                _grantedSkills.Add(skill);
            }

            Invoke_OnChangedSkillSlot(slot);
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
            Log(nameof(Leave));

            _abilitySystem.TryActivateAbility(_slotSkills[ESkillSlot.Leave]);
        }
        public void Appear(bool useAppearSkill)
        {
            Log(nameof(Appear));

            if(_slotSkills.TryGetValue(ESkillSlot.Leave, out Ability leaveSkill))
                _abilitySystem.CancelAbility(leaveSkill);

            if(useAppearSkill && _slotSkills.TryGetValue(ESkillSlot.Appear, out Ability appearSkill))
                _abilitySystem.TryActivateAbility(appearSkill);
        }

        public void SetInputAttack(bool pressed)
        {
            if (pressed)
            {
                _gameplayTagSystem.AddOwnedTag(_inputAttackTag);

                if (_slotSkills.TryGetValue(ESkillSlot.Attack, out Ability attackSkill))
                {
                    if (!_abilitySystem.TryActivateAbility(attackSkill))
                    {
                        _inputBuffer.SetBuffer(attackSkill);
                    }
                }
            }
            else
            {
                _gameplayTagSystem.RemoveOwnedTag(_inputAttackTag);

                if (_slotSkills.TryGetValue(ESkillSlot.Attack, out Ability attackSkill))
                {
                    if (_abilitySystem.IsPlayingAbility(attackSkill))
                    {
                        _abilitySystem.ReleasedAbility(attackSkill);
                    }
                    else
                    {
                        _inputBuffer.ReleaseBuffer(attackSkill);
                    }
                }
            }
        }
        public void SetInputDash(bool pressed)
        {
            if (pressed)
            {
                _gameplayTagSystem.AddOwnedTag(_inputDashTag);

                if (_slotSkills.TryGetValue(ESkillSlot.Dash, out Ability dashAbility))
                {
                    if (!_abilitySystem.TryActivateAbility(dashAbility))
                    {
                        _inputBuffer.SetBuffer(dashAbility);
                    }

                }
            }
            else
            {
                _gameplayTagSystem.RemoveOwnedTag(_inputDashTag);

                if (_slotSkills.TryGetValue(ESkillSlot.Dash, out Ability dashAbility))
                {
                    if (_abilitySystem.IsPlayingAbility(dashAbility))
                    {
                        _abilitySystem.ReleasedAbility(dashAbility);
                    }
                    else
                    {
                        _inputBuffer.ReleaseBuffer(dashAbility);
                    }
                }
            }
        }
        public void SetInputSkill(int index, bool pressed)
        {
            ESkillSlot slot;

            switch (index)
            {
                case 0:
                    slot = ESkillSlot.Skill_01;
                    break;
                case 1:
                    slot = ESkillSlot.Skill_02;
                    break;
                case 2:
                    slot = ESkillSlot.Skill_03;
                    break;
                default:
                    slot = ESkillSlot.None;
                    return;
            }

            if(pressed)
            {
                _gameplayTagSystem.AddOwnedTag(_inputSkillTags[index]);

                if(_slotSkills.TryGetValue(slot, out Ability skillAbility))
                {
                    if (!_abilitySystem.TryActivateAbility(skillAbility))
                    {
                        _inputBuffer.SetBuffer(skillAbility);
                    }
                }
            }
            else
            {
                _gameplayTagSystem.RemoveOwnedTag(_inputSkillTags[index]);

                if (_slotSkills.TryGetValue(slot, out Ability skillAbility))
                {
                    if (_abilitySystem.IsPlayingAbility(skillAbility))
                    {
                        _abilitySystem.ReleasedAbility(skillAbility);
                    }
                    else
                    {
                        _inputBuffer.ReleaseBuffer(skillAbility);
                    }
                }
            }
        }



        public void TakeKnockback(Vector3 direction, float distance, float duration)
        {
            Log($"{nameof(TakeKnockback)}");

            OnTakeKnockback?.Invoke(this, direction, distance, duration);
        }

        public void AddForce(Vector3 force)
        {
            Log($"{nameof(AddForce)}");

            OnAddForce?.Invoke(this, force);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _movementSystem.Teleport(position, true);
            _rotationSystem.SetRotation(rotation, true);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Invoke_OnCharacterColliderHit(hit);
        }

        private void Invoke_OnReseted()
        {
            Log($"{nameof(OnReseted)}");

            _onReseted.Invoke();
            OnReseted?.Invoke(this);
        }
        private void Invoke_OnSpawend()
        {
            Log($"{nameof(OnSpawned)}");

            _onSpawned.Invoke();
            OnSpawned?.Invoke(this);
        }
        private void Invoke_OnDead()
        {
            Log($"{nameof(OnDie)}");

            _onDead.Invoke();
            OnDead?.Invoke(this);
        }
        private void Invoke_OnDispawned()
        {
            Log($"{nameof(_onDespawned)}");

            _onDespawned.Invoke();
            OnDespawned?.Invoke(this);
        }
        private void Invoke_OnChangedSkillSlot(ESkillSlot skillslot)
        {
            Log($"{nameof(OnChangedSkillSlot)} - SkillSlot : {skillslot}");

            OnChangedSkillSlot?.Invoke(this, skillslot);
        }

        private void Invoke_OnCharacterColliderHit(ControllerColliderHit hit)
        {
            Log($"{nameof(Invoke_OnCharacterColliderHit)}");

            OnCharacterColliderHit?.Invoke(this, hit);
        }
    }
}
