using PF.PJT.Duet.Controller;
using PF.PJT.Duet.Pawn;
using StudioScor.AbilitySystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet
{
    public class SelectCharacterAtStartSystem : BaseMonoBehaviour
    {
        [Header(" [ Select Character At Start System ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private GameObjectListVariable _activeUIInputVariable;
        [SerializeField] private GameObjectListVariable _inActiveStatusUIVariable;

        [SerializeField] private GameObject _selectCharacterUIActor;
        [SerializeField] private CharacterInformationData[] _startCharacters;
        [SerializeField] private SelectCharacterComponent[] _selectCharacters;

        private IPlayerController _playerController;
        private void Awake()
        {
            foreach (var selectCharacter in _selectCharacters)
            {
                selectCharacter.OnSubmited += SelectCharacter_OnSubmited;
            }

            _selectCharacterUIActor.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_selectCharacters is not null)
            {
                foreach (var selectCharacter in _selectCharacters)
                {
                    if (!selectCharacter)
                        continue;

                    selectCharacter.OnSubmited -= SelectCharacter_OnSubmited;
                }
            }

            if(_playerManager)
            {
                _playerManager.OnChangedPlayerController -= _playerManager_OnChangedPlayerController;
            }

            if (_activeUIInputVariable)
            {
                _activeUIInputVariable.Remove(gameObject);

            }

            if (_inActiveStatusUIVariable)
            {
                _inActiveStatusUIVariable.Remove(gameObject);

            }
        }

        private void Start()
        {
            if (_playerManager.HasPlayerController)
            {
                CheckSelectCharacter();
            }

            _playerManager.OnChangedPlayerController += _playerManager_OnChangedPlayerController;
        }

        private void _playerManager_OnChangedPlayerController(PlayerManager playerManager, IControllerSystem currentController, IControllerSystem prevController = null)
        {
            _playerManager.OnChangedPlayerController -= _playerManager_OnChangedPlayerController;

            CheckSelectCharacter();
        }

        private void CheckSelectCharacter()
        {
            var playerControllerActor = _playerManager.PlayerController.gameObject;
            _playerController = playerControllerActor.GetComponent<IPlayerController>();

            if(_playerController.Characters.Count < 2)
            {
                OnSelectCharacter();
            }
        }

        [ContextMenu(nameof(OnSelectCharacter))]
        public void OnSelectCharacter()
        {
            Log($"{nameof(OnSelectCharacter)}");

            var randCharacter = _startCharacters.RandomElements(_selectCharacters.Length);

            for (int i = 0; i < _selectCharacters.Length; i++)
            {
                var character = randCharacter.ElementAtOrDefault(i);
                
                if(character)
                {
                    var characterData = new CharacterData(randCharacter[i]);

                    _selectCharacters[i].SetCharacterData(characterData);
                }
                else
                {
                    _selectCharacters[i].SetCharacterData(null);
                }
            }

            _activeUIInputVariable.Add(gameObject);
            _inActiveStatusUIVariable.Add(gameObject);

            EventSystem.current.SetSelectedGameObject(_selectCharacters.ElementAt(Mathf.FloorToInt(_selectCharacters.Length * 0.5f)).gameObject);

            _selectCharacterUIActor.SetActive(true);
        }
        public void EndSelectCharacter()
        {
            Log($"{nameof(EndSelectCharacter)}");

            _activeUIInputVariable.Remove(gameObject);
            _inActiveStatusUIVariable.Remove(gameObject);

            _selectCharacterUIActor.SetActive(false);
        }
        private void AddCharacter(CharacterData characterData)
        {
            // create character
            var characterActor = Instantiate(characterData.Actor, _spawnPoint.position, _spawnPoint.rotation);
            var character = characterActor.GetComponent<ICharacter>();

            // grant Ability
            var abilitySystem = character.gameObject.GetAbilitySystem();

            abilitySystem.TryGrantAbility(characterData.AppearSkill);
            abilitySystem.TryGrantAbility(characterData.PassiveSkill);
            abilitySystem.TryGrantAbility(characterData.ActiveSkill_01);

            _playerController.AddCharacter(character);

            if (_playerController.Characters.Count < 2)
            {

            }
            else
            {
                EndSelectCharacter();
            }
        }

        private void SelectCharacter_OnSubmited(SelectCharacterComponent selectCharacterUI)
        {
            if (selectCharacterUI.CharacterData is null)
                return;

            AddCharacter(selectCharacterUI.CharacterData);

            selectCharacterUI.SetCharacterData(null);
        }
    }
}
