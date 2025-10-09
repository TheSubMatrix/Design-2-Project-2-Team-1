using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenBeta : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneTransitionHandler.Instance.TransitionScene("Level Two");
        }
    }
}
