using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Create Character System")]
    public class CreateCharacterSystem : BaseMonoBehaviour, ICreateCharacterSystem
    {
        [Header(" [ Create Character System ] ")]
        [SerializeField] private List<CharacterInformationData> _characterInformationDatas;

        [Header(" Events ")]
        [SerializeField] private ToggleableUnityEvent _onCreatedCharacterDatas;
        [SerializeField] private ToggleableUnityEvent _onClearedCharacterDatas;

        private bool _wasInit;
        private readonly List<CharacterData> _characterDatas = new();

        public event ICreateCharacterSystem.CreateCharacterSystemEventHandler OnCreatedCharacterDatas;
        public event ICreateCharacterSystem.CreateCharacterSystemEventHandler OnClearedCharacterDatas;

        public IReadOnlyList<CharacterData> CharacterDatas => _characterDatas;


        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            _characterDatas.Clear();
        }

        public void CreateCharacterDatas(int count)
        {
            _characterDatas.Clear();

            var randDatas = _characterInformationDatas.RandomElements(count);

            for (int i = 0; i < randDatas.Length; i++)
            {
                var characterInformationData = randDatas[i];
                var data = new CharacterData(characterInformationData);

                _characterDatas.Add(data);
            }

            Invoke_OnCreatedCharacterDatas();
        }
        public void ClearCharacterDatas()
        {
            _characterDatas.Clear();

            Invoke_OnClearedCharacterDatas();
        }


        private void Invoke_OnCreatedCharacterDatas()
        {
            Log(nameof(OnCreatedCharacterDatas));

            _onCreatedCharacterDatas.Invoke();
            OnCreatedCharacterDatas?.Invoke(this);
        }
        private void Invoke_OnClearedCharacterDatas()
        {
            Log(nameof(OnClearedCharacterDatas));

            _onClearedCharacterDatas.Invoke();
            OnClearedCharacterDatas?.Invoke(this);
        }
    }
}
