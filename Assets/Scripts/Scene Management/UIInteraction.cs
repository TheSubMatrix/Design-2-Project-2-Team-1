using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class UIInteraction : MonoBehaviour
{
    [FormerlySerializedAs("mainMenuScreen")] public GameObject m_mainMenuScreen;
    [FormerlySerializedAs("creditsMenuScreen")] public GameObject m_creditsMenuScreen;
    
    // Enables the main menu screen when the player clicks on the "return" button on the help menu screen or credits menu screen
    public void EnableMainMenuScreen()
    {
        m_creditsMenuScreen.SetActive(false);
        m_mainMenuScreen.SetActive(true);
    }

    // Enables the help menu screen when the player clicking the "help" button on the main menu screen
    public void EnableHelpMenuScreen()
    {
        SceneTransitionHandler.Instance.TransitionScene("Help");
    }

    // Enables the main menu screen upon clicking the "credits" button on the help menu screen or credits menu screen
    public void EnableCreditsMenuScreen()
    {
        m_creditsMenuScreen.SetActive(true);
        m_mainMenuScreen.SetActive(false);
    }

    // Loads level one upon clicking the "start game" button
    public void StartGame()
    {
        SceneTransitionHandler.Instance.TransitionScene("Level One Intro Cutscene");
    }    

    // Quits the game upon clicking the "quit" button
    public void QuitGame()
    {
        SceneTransitionHandler.Instance.QuitGame();
    }
}
