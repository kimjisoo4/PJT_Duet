using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class BossEnemyStatusSystem : BaseMonoBehaviour
    {
        [Header(" [ Boss Enemy Status System ] ")]
        [SerializeField] private GameObject _statusUIActor;
        [SerializeField] private CharacterListVariable _bossCharacters;
        [SerializeField] private CharacterStateComponent _bossCharacterData;
        [SerializeField] private Transform _container;

        private readonly Dictionary<ICharacter, CharacterStateComponent> _bossCharacterDatas = new();
        private readonly Queue<CharacterStateComponent> _characterDataComponentPool = new();

        private void Awake()
        {
            _bossCharacters.OnAdded += _bossCharacters_OnAdded;
            _bossCharacters.OnRemoved += _bossCharacters_OnRemoved;

            _statusUIActor.SetActive(false);
        }
        private void OnDestroy()
        {
            if(_bossCharacters)
            {
                _bossCharacters.OnAdded -= _bossCharacters_OnAdded;
                _bossCharacters.OnRemoved -= _bossCharacters_OnRemoved;
            }
        }

        private void _bossCharacters_OnAdded(ListVariableObject<ICharacter> variable, ICharacter value)
        {
            CharacterStateComponent bossChracterData;

            if(_characterDataComponentPool.Count == 0)
            {
                bossChracterData = Instantiate(_bossCharacterData, _container);
            }
            else
            {
                bossChracterData = _characterDataComponentPool.Dequeue();
            }

            _bossCharacterDatas.Add(value, bossChracterData);
            bossChracterData.SetCharacter(value);
            bossChracterData.gameObject.SetActive(true);

            if (_bossCharacters.Values.Count == 1)
            {
                _statusUIActor.SetActive(true);
            }
        }

        private void _bossCharacters_OnRemoved(ListVariableObject<ICharacter> variable, ICharacter value)
        {
            if(_bossCharacterDatas.TryGetValue(value, out var bossChracterData))
            {
                _bossCharacterDatas.Remove(value);
                _characterDataComponentPool.Enqueue(bossChracterData);

                bossChracterData.SetCharacter(null);
                bossChracterData.gameObject.SetActive(false);

                if(_bossCharacters.Values.Count == 0)
                {
                    _statusUIActor.SetActive(false);
                }
            }
        }

        
    }
}
