using StudioScor.Utilities;
using PF.PJT.Duet.Pawn;

namespace PF.PJT.Duet
{
    public abstract class CharacterStateUIModifier : BaseMonoBehaviour
    {
        public abstract void UpdateData(ICharacter character);
    }
}
