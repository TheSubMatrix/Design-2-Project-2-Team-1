using UnityEngine;
using UnityEngine.SceneManagement;

public class UIInteraction : MonoBehaviour
{
    public GameObject mainMenuScreen;
    public GameObject helpMenuScreen;
    public GameObject creditsMenuScreen;
    
    // Enables the main menu screen when the player clicks on the "return" button on the help menu screen or credits menu screen
    public void EnableMainMenuScreen()
    {
        helpMenuScreen.SetActive(false);
        creditsMenuScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
    }

    // Enables the help menu screen when the player clicking the "help" button on the main menu screen
    public void EnableHelpMenuScreen()
    {
        helpMenuScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }

    // Enables the main menu screen upon clicking the "credits" button on the help menu screen or credits menu screen
    public void EnableCreditsMenuScreen()
    {
        creditsMenuScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }

    // Loads level one upon clicking the "start game" button
    public void StartGame()
    {
        SceneManager.LoadScene("Level One");
    }    
}
