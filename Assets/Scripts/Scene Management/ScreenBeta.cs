using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenBeta : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                SceneTransitionHandler.Instance.TransitionScene("Level Two");
            }
            
            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                SceneTransitionHandler.Instance.TransitionScene("Level Three");
            }
        }
    }
}
