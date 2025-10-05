using UnityEngine;

public class PickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private float interactDistance = 3f; // Maximum distance to interact
    [SerializeField] private Transform pickupCenter; // Optional custom pickup position reference

    private void Awake()
    {
        // If no custom reference is set, use this object's position
        if (pickupCenter == null)
        {
            pickupCenter = transform;
        }
    }
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        // Check if player presses E to interact
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Ensure player is close enough
            float distance = Vector3.Distance(interactor.transform.position, pickupCenter.position);

            if (distance <= interactDistance)
            {
                Destroy(gameObject); // Destroys pickup
            }
            else
            {
                Debug.Log("Player is too far to interact with the door.");
            }
        }
    }

    public void OnExitedInteraction()
    {

    }
}
