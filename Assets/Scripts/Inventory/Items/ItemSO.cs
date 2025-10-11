using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/Inventory/Item"), Serializable]
public class ItemSO : ScriptableObject, IEquatable<ItemSO>
{
    [field: SerializeField] public string ItemName { get; private set; }
    [field: SerializeField] public Sprite ItemIcon {get; private set;}

    public bool Equals(ItemSO other)
    {
        if (other is null) return false;
        return ItemName == other.ItemName;
    }
}
