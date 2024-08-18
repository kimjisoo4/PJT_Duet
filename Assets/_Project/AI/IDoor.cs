namespace PF.PJT.Duet
{
    public interface IDoor
    {
        public bool IsOpened { get; }

        public bool CanOpen();
        public bool CanClose();
        public bool TryOpen();
        public bool TryClose();
        public void ForceOpen();
        public void ForceClose();
    }

}
