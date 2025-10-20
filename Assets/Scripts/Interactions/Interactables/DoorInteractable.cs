using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    private Animator doorAnimation;
    private bool isOpen = false;
    [SerializeField] private bool requiresKeycard;
    [SerializeField] private ItemSO keycard;

    private void Awake()
    {
        doorAnimation = GetComponent<Animator>(); // Reference to the Animator component on the door
    }

    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        if (requiresKeycard)
        {
            if (interactor.transform.root.GetComponentInChildren<Inventory>().HasItem(keycard))
            {
                ToggleDoor();
            }
        }
        else
        {
            ToggleDoor();
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

