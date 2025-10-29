using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] InputActionReference m_moveAction;
    [SerializeField] InputActionReference m_jumpAction;
    [SerializeField] InputActionReference m_sprintAction;
    [SerializeField] InputActionReference m_crouchAction;
    [SerializeField] InputActionReference m_lookAction;

    bool m_shouldReadInput = true;
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public event Action JumpPressed;
    public event Action CrouchPressed;
    public event Action SprintPressed;
    public event Action SprintReleased;

    EventBinding<UpdatePlayerInputState> m_updateInputStateEvent;

    void OnEnable()
    {
        m_updateInputStateEvent = new EventBinding<UpdatePlayerInputState>(HandleInputReadChange);
        EventBus<UpdatePlayerInputState>.Register(m_updateInputStateEvent);
        
        if (m_moveAction != null)
        {
            m_moveAction.action.Enable();
            m_moveAction.action.canceled += OnMove;
            m_moveAction.action.performed += OnMove;
        }
        if (m_jumpAction != null)
        {
            m_jumpAction.action.Enable();
            m_jumpAction.action.performed += OnJumpPerformed;
        }
        if (m_sprintAction != null)
        {
            m_sprintAction.action.performed += OnSprintPerformed;
            m_sprintAction.action.canceled += OnSprintCanceled;
            m_sprintAction.action.Enable();
        }
        if (m_crouchAction != null)
        {
            m_crouchAction.action.Enable();
            m_crouchAction.action.performed += OnCrouchPerformed;
        }
        if (m_lookAction != null)
        {
            m_lookAction.action.Enable();
        }
    }

    void OnDisable()
    {
        EventBus<UpdatePlayerInputState>.Deregister(m_updateInputStateEvent);
        
        if (m_moveAction != null)
        {
            m_moveAction.action.Disable();
            m_moveAction.action.performed -= OnMove;
            m_moveAction.action.canceled -= OnMove;
        }
        if (m_jumpAction != null)
        {
            m_jumpAction.action.performed -= OnJumpPerformed;
            m_jumpAction.action.Disable();
        }
        if (m_sprintAction != null)
        {
            m_sprintAction.action.Disable();
            m_sprintAction.action.performed -= OnSprintPerformed;
            m_sprintAction.action.canceled -= OnSprintCanceled;
        }
        if (m_crouchAction != null)
        {
            m_crouchAction.action.performed -= OnCrouchPerformed;
            m_crouchAction.action.Disable();
        }
        if (m_lookAction != null)
        {
            m_lookAction.action.Disable();
        }
    }

    void Update()
    {
        // Read look input every frame for smooth camera movement
        if (m_lookAction is not null && m_shouldReadInput)
        {
            LookInput = m_lookAction.action.ReadValue<Vector2>();
        }
        else
        {
            LookInput = Vector2.zero;
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = m_shouldReadInput ? context.ReadValue<Vector2>() : Vector2.zero;
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (m_shouldReadInput) JumpPressed?.Invoke();
    }

    void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        if (m_shouldReadInput) CrouchPressed?.Invoke();
    }

    void OnSprintPerformed(InputAction.CallbackContext context)
    {
        if (m_shouldReadInput) SprintPressed?.Invoke();
    }

    void OnSprintCanceled(InputAction.CallbackContext context)
    {
        if (m_shouldReadInput) SprintReleased?.Invoke();
    }

    void HandleInputReadChange(UpdatePlayerInputState state)
    {
        m_shouldReadInput = state.DesiredInputState;

        if (m_shouldReadInput) return;
        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
    }
}