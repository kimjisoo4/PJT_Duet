#if SCOR_ENABLE_VISUALSCRIPTING
using StudioScor.Utilities.VisualScripting;
using System;
using Unity.VisualScripting;


namespace StudioScor.BodySystem.VisualScripting
{

    [UnitSubtitle("BodySystem Event")]
    [UnitCategory("Events\\StudioScor\\BodySystem")]
    public abstract class BodySystemEventUnit : CustomEventUnit<BodySystemComponent, ChangedBodyPartValue>
    {
        public override Type MessageListenerType => typeof(BodySystemEventListener);

        [DoNotSerialize]
        [PortLabel("BodyTag")]
        public ValueOutput BodyTag;

        [DoNotSerialize]
        [PortLabel("BodyPart")]
        public ValueOutput BodyPart;

        protected override void Definition()
        {
            base.Definition();

            BodyTag = ValueOutput<BodyTag>(nameof(BodyTag));
            BodyPart = ValueOutput<BodyPartComponent>(nameof(BodyPart));
        }

        protected override void AssignArguments(Flow flow, ChangedBodyPartValue changedBodyPartValue)
        {
            base.AssignArguments(flow, changedBodyPartValue);

            flow.SetValue(BodyTag, changedBodyPartValue.BodyTag);
            flow.SetValue(BodyPart, changedBodyPartValue.BodyPart);
        }
    }
}
#endif