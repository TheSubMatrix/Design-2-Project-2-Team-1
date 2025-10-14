using System;
using System.Collections.Generic;
using AudioSystem;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TerminalInteractable : MonoBehaviour, IInteractable
{
    CinemachineCamera m_previousCamera;
    [SerializeField] CanvasGroup m_mainMenuCanvasGroup;
    [SerializeField] Selectable m_startingSelectable;
    [SerializeField] CanvasGroup m_mailMenuCanvasGroup;
    [SerializeField] Selectable m_mailMenuStartingSelectable;
    [SerializeField] CinemachineCamera m_interactionCamera;
    [SerializeField] InputActionReference m_exitAction;
    [SerializeField] InputActionReference m_moveAction;
    [SerializeField] InputActionReference m_selectAction;
    [SerializeField] SoundData m_interactSound;
    [SerializeField] SoundData m_exitSound;
    
    [SerializeField] List<CanvasGroup> m_mailSubCanvasGroups;
    int m_selectedMailSubCanvasGroupIndex = 0;
    CanvasGroup m_activeCanvasGroup;
    bool m_isInteracting;

    void Awake()
    {
        m_activeCanvasGroup = m_mainMenuCanvasGroup;
    }

    void OnEnable()
    {
        m_exitAction.action.Enable();
        m_exitAction.action.performed += ExitInteractionViaInput;
        m_moveAction.action.Enable();
        m_moveAction.action.performed += OnMoveAction;
        m_selectAction.action.Enable();
        m_selectAction.action.performed += OnSelectAction;
    }
    void OnDisable()
    {
        m_exitAction.action.Disable();
        m_exitAction.action.performed -= ExitInteractionViaInput;
        m_moveAction.action.Disable();
        m_moveAction.action.performed -= OnMoveAction;
        m_selectAction.action.Disable();
        m_selectAction.action.performed -= OnSelectAction;
    }

    void ExitInteractionViaInput(InputAction.CallbackContext context)
    {
        if(!m_isInteracting) return;
        OnExitedInteraction();
    }

    void OnMoveAction(InputAction.CallbackContext context)
    {
        if(!m_isInteracting) return;
        Vector2 input = context.ReadValue<Vector2>();
        
        Selectable currentSelectable = EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>();
        if (currentSelectable == null)
            currentSelectable = m_startingSelectable;
        
        Vector3 direction = new Vector3(input.x, input.y, 0f);
        Selectable nextSelectable = currentSelectable.FindSelectable(direction);

        if (nextSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(nextSelectable.gameObject);
        }
    }
    void OnSelectAction(InputAction.CallbackContext context)
    {
        if(!m_isInteracting) return;
        ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }

    public void OnMailButtonPressed()
    {
        m_mainMenuCanvasGroup.interactable = false;
        m_mainMenuCanvasGroup.alpha = 0;
        m_mailMenuCanvasGroup.alpha = 1;
        m_mailMenuCanvasGroup.interactable = true;
        EventSystem.current.SetSelectedGameObject(m_mailMenuStartingSelectable.gameObject);
        m_mainMenuCanvasGroup.interactable = false;
        m_activeCanvasGroup = m_mailMenuCanvasGroup;
        m_mailMenuCanvasGroup.interactable = true;
    }
    public void OnBackButtonPressed()
    {
        m_mailMenuCanvasGroup.interactable = false;
        m_mailMenuCanvasGroup.alpha = 0;
        m_mainMenuCanvasGroup.alpha = 1;
        m_mainMenuCanvasGroup.interactable = true;
        EventSystem.current.SetSelectedGameObject(m_startingSelectable.gameObject);
        m_mailMenuCanvasGroup.interactable = false;
        m_activeCanvasGroup = m_mainMenuCanvasGroup;
        m_mainMenuCanvasGroup.interactable = true;
    }
    public void OnScrollUpButtonPressed()
    {
        m_mailSubCanvasGroups[m_selectedMailSubCanvasGroupIndex].alpha = 0;
        m_selectedMailSubCanvasGroupIndex = (m_selectedMailSubCanvasGroupIndex + 1) % m_mailSubCanvasGroups.Count;
        m_mailSubCanvasGroups[m_selectedMailSubCanvasGroupIndex].alpha = 1;
    }
    public void OnScrollDownButtonPressed()
    {
        m_mailSubCanvasGroups[m_selectedMailSubCanvasGroupIndex].alpha = 0;
        m_selectedMailSubCanvasGroupIndex = (m_selectedMailSubCanvasGroupIndex - 1 + m_mailSubCanvasGroups.Count) % m_mailSubCanvasGroups.Count;
        m_mailSubCanvasGroups[m_selectedMailSubCanvasGroupIndex].alpha = 1;
    }
    
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        m_previousCamera = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera as CinemachineCamera;
        if(m_interactionCamera is null) return;
        EventBus<CameraBlendData>.Raise(new CameraBlendData(m_interactionCamera));
        EventBus<UpdatePlayerInputState>.Raise(new UpdatePlayerInputState(false));
        EventBus<FadeCanvasGroup>.Raise(new FadeCanvasGroup("Hands", 0.5f,0));
        SoundManager.Instance.CreateSound().WithSoundData(m_interactSound).WithPosition(transform.position).WithRandomPitch().Play();
        EventSystem.current.SetSelectedGameObject(m_startingSelectable.gameObject);
        m_isInteracting = true;
        m_activeCanvasGroup.interactable = true;
    }

    public void OnExitedInteraction()
    {
        EventBus<CameraBlendData>.Raise(new CameraBlendData(m_previousCamera));
        EventBus<UpdatePlayerInputState>.Raise(new UpdatePlayerInputState(true));
        EventBus<FadeCanvasGroup>.Raise(new FadeCanvasGroup("Hands", 0.5f, 1));
        SoundManager.Instance.CreateSound().WithSoundData(m_exitSound).WithPosition(transform.position).WithRandomPitch().Play();
        EventSystem.current.SetSelectedGameObject(null);
        m_isInteracting = false;
        m_mailMenuCanvasGroup.interactable = false;
        m_mainMenuCanvasGroup.interactable = false;
    }
}
