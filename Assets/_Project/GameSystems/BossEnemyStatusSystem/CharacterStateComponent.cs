using UnityEngine;
using StudioScor.Utilities;
using PF.PJT.Duet.Pawn;

namespace PF.PJT.Duet
{
    public class CharacterStateComponent : BaseMonoBehaviour
    {
        [Header(" [ Character Data Component ] ")]
        [SerializeField] private CharacterStateUIModifier[] _modifiers;

        private ICharacter _character;
        public ICharacter Character => _character;

        public void SetCharacter(ICharacter character)
        {
            _character = character;

            foreach (var modifier in _modifiers)
            {
                modifier.UpdateData(_character);
            }
        }

    }
}
