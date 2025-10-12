using UnityEngine;

public class SceneTransitionEventDriver : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneTransitionHandler.Instance.TransitionScene(sceneName);
    }
}
