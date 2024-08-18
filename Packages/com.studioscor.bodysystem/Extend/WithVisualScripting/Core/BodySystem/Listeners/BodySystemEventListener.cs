#if SCOR_ENABLE_VISUALSCRIPTING
using Unity.VisualScripting;


namespace StudioScor.BodySystem.VisualScripting
{
    public class BodySystemEventListener : MessageListener
    {
        private ChangedBodyPartValue _ChangedBodyPartValue;
        private void Awake()
        {
            var bodySystem = GetComponent<BodySystemComponent>();

            bodySystem.OnGrantedBodyPart += BodySystem_OnGrantedBodyPart;
            bodySystem.OnRemovedBodyPart += BodySystem_OnRemovedBodyPart;
        }
        private void OnDestroy()
        {
            if (TryGetComponent(out BodySystemComponent bodySystem))
            {
                bodySystem.OnGrantedBodyPart -= BodySystem_OnGrantedBodyPart;
                bodySystem.OnRemovedBodyPart -= BodySystem_OnRemovedBodyPart;
            }
        }
        private void BodySystem_OnRemovedBodyPart(BodySystemComponent bodySystem, BodyTag bodytag, IBodyPart transform)
        {
            if (_ChangedBodyPartValue is null)
                _ChangedBodyPartValue = new();

            _ChangedBodyPartValue.BodyTag = bodytag;
            _ChangedBodyPartValue.BodyPart = transform;

            EventBus.Trigger(new EventHook(BodySystemWithVisualScripting.BODYSYSTEM_REMOVED_BODY_PART, bodySystem), _ChangedBodyPartValue);

            _ChangedBodyPartValue.BodyTag = null;
            _ChangedBodyPartValue.BodyPart = null;
        }

        private void BodySystem_OnGrantedBodyPart(BodySystemComponent bodySystem, BodyTag bodytag, IBodyPart transform)
        {
            if (_ChangedBodyPartValue is null)
                _ChangedBodyPartValue = new();

            _ChangedBodyPartValue.BodyTag = bodytag;
            _ChangedBodyPartValue.BodyPart = transform;

            EventBus.Trigger(new EventHook(BodySystemWithVisualScripting.BODYSYSTEM_GRANTED_BODY_PART, bodySystem), _ChangedBodyPartValue);

            _ChangedBodyPartValue.BodyTag = null;
            _ChangedBodyPartValue.BodyPart = null;
        }
    }
}
#endif