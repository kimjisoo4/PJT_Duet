using System.Collections.Generic;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    public interface ICreateCharacterDisplay
    {
        public delegate void CreateCharacterDisplayEventHandler(ICreateCharacterDisplay createCharacterDisplay);

        public void Init();
        public void Activate();
        public void Inactivate();
        public void SetCharacterDatas(IEnumerable<CharacterData> chracterDatas);


        public event CreateCharacterDisplayEventHandler OnActivated;
        public event CreateCharacterDisplayEventHandler OnFinishedBlendIn;
        public event CreateCharacterDisplayEventHandler OnStartedBlendOut;
        public event CreateCharacterDisplayEventHandler OnInactivated;
    }
}
