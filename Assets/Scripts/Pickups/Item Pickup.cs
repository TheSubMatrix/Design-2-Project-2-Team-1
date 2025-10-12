using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] ItemSO Item;
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        Inventory inventory = interactor.transform.root.GetComponentInChildren<Inventory>();
        if(inventory?.TryAddItem(Item, 1) > 0)
        {
            Destroy(gameObject);
        }
    }
}
