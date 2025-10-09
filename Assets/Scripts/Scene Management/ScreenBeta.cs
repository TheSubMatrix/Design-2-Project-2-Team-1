using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenBeta : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Level Two");
        }
    }
}
