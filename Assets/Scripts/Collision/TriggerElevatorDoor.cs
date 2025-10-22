using UnityEngine;

public class TriggerElevatorDoor : MonoBehaviour
{
    private Animator elevatorDoorAnimator;

    private void Awake()
    {
        elevatorDoorAnimator = GetComponent<Animator>(); // Reference to the Animator component on the reinforced door
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if collider belongs to the player and animation of the reinforced door closing plays if player touches the trigger
        if (other.CompareTag("Player"))
        {
            elevatorDoorAnimator.SetTrigger("Open");
        }
    }
}
