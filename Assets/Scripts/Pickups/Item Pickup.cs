using UnityEngine;

public class ItemPickup : FloatingPickup
{
    [SerializeField] ItemSO Item;
    protected override void OnPickup(MonoBehaviour interactor)
    {
        Inventory inventory = interactor.transform.root.GetComponentInChildren<Inventory>();
        if(inventory?.TryAddItem(Item, 1) <= 0)
        {
            Destroy(gameObject);
        }
    }
}
