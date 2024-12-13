using System.Collections.Generic;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    public interface ICreateCharacterSystem
    {
        public delegate void CreateCharacterSystemEventHandler(ICreateCharacterSystem createCharacterSystem);

        public IReadOnlyList<CharacterData> CharacterDatas { get; }

        public void Init();
        public void CreateCharacterDatas(int count);
        public void ClearCharacterDatas();

        public event CreateCharacterSystemEventHandler OnCreatedCharacterDatas;
        public event CreateCharacterSystemEventHandler OnClearedCharacterDatas;
    }
}
