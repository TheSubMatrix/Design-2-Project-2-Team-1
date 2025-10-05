using UnityEngine;

public class PickupInteractable : MonoBehaviour, IInteractable
{
    
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        Destroy(gameObject);
    }

    public void OnExitedInteraction()
    {

    }
}
