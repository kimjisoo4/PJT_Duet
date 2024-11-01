using StudioScor.PlayerSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class PlayerStatusSystem : BaseMonoBehaviour
    {
        [Header(" [ Player Status System ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private GameObject _statusUIActor;
        [SerializeField] private StatusUIToSimpleAmount[] _statusBars;

        [Header(" Events ")]
        [SerializeField] private GameObjectListVariable _inActoveVariable;

        private void Awake()
        {
            if(_inActoveVariable)
            {
                _inActoveVariable.OnAdded += _inActoveVariable_OnAdded;
                _inActoveVariable.OnRemoved += _inActoveVariable_OnRemoved;
            }
        }
        private void Start()
        {
            if(_playerManager.HasPlayerPawn)
            {
                SetCharacter(_playerManager.PlayerPawn.gameObject);
            }

            _playerManager.OnChangedPlayerPawn += _playerManager_OnChangedPlayerPawn;
        }
        private void OnDestroy()
        {
            if(_playerManager)
            {
                _playerManager.OnChangedPlayerPawn -= _playerManager_OnChangedPlayerPawn;
            }

            if(_inActoveVariable)
            {
                _inActoveVariable.OnAdded -= _inActoveVariable_OnAdded;
                _inActoveVariable.OnRemoved -= _inActoveVariable_OnRemoved;
            }
        }

        private void _inActoveVariable_OnAdded(ListVariableObject<GameObject> variable, GameObject value)
        {
            if (_inActoveVariable.Values.Count == 0)
                return;

            if (!_statusUIActor.activeSelf)
                return;

            _statusUIActor.SetActive(false);
        }
        private void _inActoveVariable_OnRemoved(ListVariableObject<GameObject> variable, GameObject value)
        {
            if (_inActoveVariable.Values.Count != 0)
                return;

            if (_statusUIActor.activeSelf)
                return;

            _statusUIActor.SetActive(true);
        }

        
        public void SetCharacter(GameObject character)
        {
            for(int i = 0; i < _statusBars.Length; i++)
            {
                var bar = _statusBars[i];
            
                bar.SetTarget(character);
            }
        }

        private void _playerManager_OnChangedPlayerPawn(PlayerManager playerManager, IPawnSystem currentPawn, IPawnSystem prevPawn = null)
        {
            var target = currentPawn is null ? null : currentPawn.gameObject;

            SetCharacter(target);
        }
    }
}
