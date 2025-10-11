using UnityEngine;


[System.Serializable]
internal class InventorySlot
{
    const uint MaxAmount = 999;
    public bool IsEmpty => Item is null;
    [field:SerializeField] public ItemSO Item { get; private set; }
    [field:SerializeField] public uint Amount { get; private set; }

    public InventorySlot(ItemSO item = null, uint amount = 0)
    {
        Item = item;
        Amount = amount;
    }

    public void Clear()
    {
        Item = null;
        Amount = 0;
        
    }

    public uint Add(ItemSO item, uint amountToAdd)
    {
        if(item is null || amountToAdd is 0 || Item is not null && Item != item) return amountToAdd;
        Item ??= item;
        if (amountToAdd > MaxAmount)
        {
            Amount = MaxAmount;
            return amountToAdd - MaxAmount;
        }
        if (Amount + amountToAdd > MaxAmount)
        {
            uint diff = Amount + amountToAdd - MaxAmount;
            Amount = MaxAmount;
            return diff;
        }
        Amount += amountToAdd;
        return 0;
    }

    public uint Remove(ItemSO item, uint amountToRemove)
    {
        if(item is null || amountToRemove is 0 || Item is not null && Item != item) return amountToRemove;
        if (amountToRemove >= Amount)
        {
            Amount = 0;
            Item = null;
            return amountToRemove - Amount;
        }
        Amount -= amountToRemove;
        return 0;
    }
}
