using PF.PJT.Duet.Controller.Enemy;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class DetectPlayerTriggerArea : BaseMonoBehaviour
    {
        [Header(" [ Detect Player Trigger Area ] ")]
        [SerializeField] private EnemyController[] _enemyControllers;

        private ITriggerArea _triggerArea;

        private void Awake()
        {
            if(gameObject.TryGetComponent(out _triggerArea))
            {
                _triggerArea.OnEnteredTrigger += _triggerArea_OnEnteredTrigger;
            }
        }
        private void OnDestroy()
        {
            if(_triggerArea is not null)
            {
                _triggerArea.OnEnteredTrigger -= _triggerArea_OnEnteredTrigger;
            }
        }

        private void _triggerArea_OnEnteredTrigger(ITriggerArea triggerArea, Collider collider)
        {
            foreach (var enemyController in _enemyControllers)
            {
                enemyController.SetTargetKey(collider.transform);
            }

            gameObject.SetActive(false);
        }
    }
}
