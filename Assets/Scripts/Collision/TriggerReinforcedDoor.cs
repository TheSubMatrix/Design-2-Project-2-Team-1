using UnityEngine;

public class TriggerReinforcedDoor : MonoBehaviour
{
    private Animator reinforcedDoorAnimator;

    private void Awake()
    {
        reinforcedDoorAnimator = GetComponent<Animator>(); // Reference to the Animator component on the reinforced door
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if collider belongs to the player and animation of the reinforced door closing plays if player touches the trigger
        if (other.CompareTag("Player"))
        {
            reinforcedDoorAnimator.SetTrigger("Close");
        }
    }
}
