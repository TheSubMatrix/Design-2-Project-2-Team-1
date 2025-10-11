using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] ItemSO Item;
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        Inventory inventory = interactor.transform.root.GetComponentInChildren<Inventory>();
        inventory?.TryAddItem(Item, 1);
    }
}
