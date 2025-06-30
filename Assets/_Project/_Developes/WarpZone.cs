using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class WarpZone : MonoBehaviour
    {
        [Header(" [ Warp Zone ] ")]
        [SerializeField] private Transform _arrivalPoint;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetActor(out IActor actor))
                return;

            if(actor.transform.TryGetComponent(out ICharacter character))
            {
                character.Teleport(_arrivalPoint.position, _arrivalPoint.rotation);
            }
        }
    }
}
