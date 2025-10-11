using UnityEngine;

public struct InventoryData : IEvent
{
    public struct SlotData
    {
        public readonly ItemSO Item;
        public readonly uint Amount;
        public SlotData(ItemSO item, uint amount)
        {
            Item = item;
            Amount = amount;
        }
    }
    public readonly SlotData[] Slots;
    public InventoryData(params SlotData[] slots)
    {
        Slots = slots;
    }
}
