using StudioScor.Utilities;
using UnityEngine;


namespace PF.PJT.Duet
{
    public class DoorActor : BaseMonoBehaviour, IDoor
    {
        [Header(" [ Door Actor ] ")]
        [SerializeField] private GameObject _door;
        [SerializeField] private bool _isStartedOpen = true;

        private bool _isOpened;
        public bool IsOpened => _isOpened;

        private void Awake()
        {
            if(_isStartedOpen)
            {
                ForceOpen();
            }
            else
            {
                ForceClose();
            }
        }
        public bool TryOpen()
        {
            if (!CanOpen())
                return false;

            ForceOpen();

            return true;
        }
        public bool TryClose()
        {
            if (!CanClose())
                return false;

            ForceClose();

            return true;
        }
        public virtual bool CanOpen()
        {
            return !IsOpened;
        }
        public virtual bool CanClose()
        {
            return IsOpened;
        }
        public virtual void ForceOpen()
        {
            _door.SetActive(false);
        }
        public virtual void ForceClose()
        {
            _door.SetActive(true);
        }
    }

}
