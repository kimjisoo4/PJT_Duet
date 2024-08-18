#if SCOR_ENABLE_VISUALSCRIPTING
using Unity.VisualScripting;


namespace StudioScor.BodySystem.VisualScripting
{
    [UnitTitle("BodySystem On Granted Body Part")]
    [UnitShortTitle("OnGrantedBodyPart")]
    public class BodySystemOnGrantedBodyPartEventUnit : BodySystemEventUnit
    {
        protected override string HookName => BodySystemWithVisualScripting.BODYSYSTEM_GRANTED_BODY_PART;
    }
}
#endif