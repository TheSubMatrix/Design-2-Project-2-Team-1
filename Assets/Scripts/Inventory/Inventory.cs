using System;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    
    const int InventorySize = 4;
    [SerializeField] InventorySlot[] m_slots;

    void Awake()
    {
        m_slots = Enumerable.Range(0, InventorySize).Select(_ => new InventorySlot()).ToArray();
        EventBus<InventoryData>.Raise(new InventoryData(m_slots.Select(ToSlotData).ToArray()));
    }

    public uint TryAddItem(ItemSO item, uint amount)
    {
        uint amountRemaining = amount;
        foreach (InventorySlot slot in m_slots)
        {
            if (slot.Item != item && slot.Item is not null) continue;
            amountRemaining = slot.Add(item, amount);
            if (amountRemaining == 0) break;
        }
        EventBus<InventoryData>.Raise(new InventoryData(m_slots.Select(ToSlotData).ToArray()));
        return amountRemaining;
    }

    public uint TryRemoveItem(ItemSO item, uint amount)
    {
        uint amountRemaining = amount;
        foreach (InventorySlot slot in m_slots)
        {
            if(slot.Item != item && slot.Item is not null) continue;
            amountRemaining = slot.Remove(item, amount);
            if (amountRemaining == 0) break;
        }
        EventBus<InventoryData>.Raise(new InventoryData(m_slots.Select(ToSlotData).ToArray()));
        return amountRemaining;
    }

    public uint GetItemCount(ItemSO item)
    {
        return m_slots.Where(slot => slot.Item == item || slot.Item is null).Aggregate<InventorySlot, uint>(0, (current, slot) => current + slot.Amount);
    }
    
    static InventoryData.SlotData ToSlotData(InventorySlot slot)
    {
        return new InventoryData.SlotData(slot.Item, slot.Amount);
    }
}
