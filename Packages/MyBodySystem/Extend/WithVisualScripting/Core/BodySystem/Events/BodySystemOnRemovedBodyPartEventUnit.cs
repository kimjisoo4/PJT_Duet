#if SCOR_ENABLE_VISUALSCRIPTING
using Unity.VisualScripting;

namespace StudioScor.BodySystem.VisualScripting
{
    [UnitTitle("BodySystem On Removed Body Part")]
    [UnitShortTitle("OnRemovedBodyPart")]
    public class BodySystemOnRemovedBodyPartEventUnit : BodySystemEventUnit
    {
        protected override string HookName => BodySystemWithVisualScripting.BODYSYSTEM_REMOVED_BODY_PART;
    }
}

#endif