using StudioScor.Utilities;

namespace PF.PJT.Duet.GameFlowSystem
{
    public abstract class BaseDecision : BaseScriptableObject
    {
        public abstract bool Decide(object owner);
    }

    public abstract class GameFlowSystemDecision : BaseDecision { }

    
}
