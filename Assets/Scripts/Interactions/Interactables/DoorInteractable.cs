using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    private Animator doorAnimation;
    private bool isOpen = false;

    [SerializeField] private float interactDistance = 3f; // Maximum distance to interact
    [SerializeField] private Transform doorCenter; // Optional custom door position reference


    private void Awake()
    {
        doorAnimation = GetComponent<Animator>(); // Reference to the Animator component on the door

        // If no custom reference is set, use this object's position
        if (doorCenter == null)
        {
            doorCenter = transform;
        }
    }

    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        // Check if player presses E to interact
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Ensure player is close enough
            float distance = Vector3.Distance(interactor.transform.position, doorCenter.position);

            if (distance <= interactDistance)
            {
                ToggleDoor();
            }
            else
            {
                Debug.Log("Player is too far to interact with the door.");
            }
        }
    }

    private void ToggleDoor()
    {
        if (doorAnimation == null) return;

        if (isOpen) // Animation of the door closing plays if the door is open
        {
            doorAnimation.SetTrigger("Close");
        }
        else // Animation of the door opening plays if the door is closed
        {
            doorAnimation.SetTrigger("Open");
        }

        isOpen = !isOpen;
    }

    public void OnExitedInteraction()
    {
        // Optional: Add logic for when player moves away from door
    }
}

