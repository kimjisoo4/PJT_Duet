using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;

namespace PF.PJT.Duet
{
    public abstract class DataUpdater : BaseMonoBehaviour
    {
        public void UpdateData(object data)
        {
            Log($"{nameof(UpdateData)} - {data}");

            OnUpdateData(data);
        }
        public abstract void OnUpdateData(object data);
    }
}
