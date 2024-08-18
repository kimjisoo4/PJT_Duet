using UnityEngine;

namespace PF.PJT.Duet
{
    public interface IDisplayItem
    {
        public Sprite Icon { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
