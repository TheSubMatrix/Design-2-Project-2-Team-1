using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GoToSceneCheat : MonoBehaviour
{
    [SerializeField] InputActionReference m_mainMenuCheatAction;
    [SerializeField] InputActionReference m_levelOneCheatAction;
    [SerializeField] InputActionReference m_levelTwoCheatAction;
    [SerializeField] InputActionReference m_levelThreeCheatAction;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void OnEnable()
    {
        m_mainMenuCheatAction.action.Enable();
        m_mainMenuCheatAction.action.started += OnMainMenuCheat;
        m_levelOneCheatAction.action.Enable();
        m_levelOneCheatAction.action.started += OnLevelOneCheat;
        m_levelTwoCheatAction.action.Enable();
        m_levelTwoCheatAction.action.started += OnLevelTwoCheat;
        m_levelThreeCheatAction.action.Enable();
        m_levelThreeCheatAction.action.started += OnLevelThreeCheat;
    }
    void OnDisable()
    {
        m_mainMenuCheatAction.action.Disable();
        m_mainMenuCheatAction.action.started -= OnMainMenuCheat;
        m_levelOneCheatAction.action.Disable();
        m_levelOneCheatAction.action.started -= OnLevelOneCheat;
        m_levelTwoCheatAction.action.Disable();
        m_levelTwoCheatAction.action.started -= OnLevelTwoCheat;
        m_levelThreeCheatAction.action.Disable();
        m_levelThreeCheatAction.action.started -= OnLevelThreeCheat;
    }

    void OnMainMenuCheat(InputAction.CallbackContext context)
    {
        SceneTransitionHandler.Instance.TransitionScene("Main Menu");
    }
    void OnLevelOneCheat(InputAction.CallbackContext context)
    {
        SceneTransitionHandler.Instance.TransitionScene("Level One");
    }
    void OnLevelTwoCheat(InputAction.CallbackContext context)
    {
        SceneTransitionHandler.Instance.TransitionScene("Level Two");
    }
    void OnLevelThreeCheat(InputAction.CallbackContext context)
    {
        SceneTransitionHandler.Instance.TransitionScene("Level Three");
    }
}
