using System.Collections.Generic;
using UnityEngine;

namespace StudioScor.BodySystem
{
    public static class BodySystemUtility
    {
        public static IBodySystem GetBodySystem(this Component component)
        {
            return component.gameObject.GetBodySystem();
        }
        public static bool TryGetBodySystem(this Component component, out IBodySystem bodySystem)
        {
            return component.gameObject.TryGetBodySystem(out bodySystem);
        }
        public static IBodySystem GetBodySystem(this GameObject gameObject)
        {
            return gameObject.GetComponent<IBodySystem>();
        }
        public static bool TryGetBodySystem(this GameObject gameObject, out IBodySystem bodySystem)
        {
            return gameObject.TryGetComponent(out bodySystem);
        }

        public static bool HasBodyPart(this IBodySystem bodySystem, BodyTag tag)
        {
            return bodySystem.BodyParts.ContainsKey(tag);
        }
        public static bool TryGetBodyPart(this IBodySystem bodySystem, BodyTag tag, out IBodyPart bodyPart)
        {
            return bodySystem.BodyParts.TryGetValue(tag, out bodyPart);
        }
        public static IBodyPart GetBodyPart(this IBodySystem bodySystem, BodyTag tag)
        {
            if(bodySystem.BodyParts.ContainsKey(tag))
            {
                return bodySystem.BodyParts[tag];
            }

            return null;
        }

        public static bool CanEquipment(this IBodySystem bodySystem, BodyTag tag, out IBodyPart bodyPart)
        {
            return bodySystem.TryGetBodyPart(tag, out bodyPart) && bodyPart.CanEquiped;
        }

        public static bool CanEquipment(this IBodySystem bodySystem, IEnumerable<BodyTag> tags, out IBodyPart bodyPart)
        {
            bool canEquipt = false;
            bodyPart = null;

            foreach (var tag in tags)
            {
                if(bodySystem.TryGetBodyPart(tag, out bodyPart) && bodyPart.CanEquiped)
                {
                    canEquipt = true;

                    break;
                }
            }

            return canEquipt;
        }
    }

}
