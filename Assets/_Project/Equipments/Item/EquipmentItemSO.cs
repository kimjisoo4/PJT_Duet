using UnityEngine;

namespace PF.PJT.Duet
{

    public abstract class EquipmentItemSO : ItemSO, IEquipment, IItem, IDisplayIcon, IDisplayName, IDisplayDescription
    {
        public abstract Sprite Icon { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }


        public Object Context => this;
        bool IEquipment.UseDebug => UseDebug;

        public IEquipmentItemSpec CreateSpec()
        {
            Log(nameof(CreateSpec));

            return GetSpec();
        }

        protected abstract IEquipmentItemSpec GetSpec();

        public override bool CanUseItem(GameObject actor)
        {
            return base.CanUseItem(actor) && actor.HasEquipmnentWearer();
        }

        public override void UseItem(GameObject actor)
        {
            if (actor.TryGetComponent(out IEquipmentWearer wearer))
            {
                var spec = CreateSpec();

                wearer.TryEquipItem(spec);
            }
        }
    }
}
