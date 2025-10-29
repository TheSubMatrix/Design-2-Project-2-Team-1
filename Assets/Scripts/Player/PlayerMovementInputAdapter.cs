using System;
using UnityEngine;

[Serializable]
internal class PlayerMovementInputAdapter
{
    PlayerInputManager m_inputManager;
    PlayerMovement m_player;
    bool m_wantsToJump;
    bool m_wantsToCrouch;
    bool m_isSprintHeld;
    public Vector2 Input => m_inputManager != null ? m_inputManager.MoveInput : Vector2.zero;
    public void Initialize(PlayerMovement player, PlayerInputManager inputManager)
    {
        m_player = player;
        m_inputManager = inputManager;

        if (m_inputManager == null) return;
        m_inputManager.JumpPressed += OnJumpPressed;
        m_inputManager.CrouchPressed += OnCrouchPressed;
        m_inputManager.SprintPressed += OnSprintPressed;
        m_inputManager.SprintReleased += OnSprintReleased;
    }

    public void Cleanup()
    {
        if (m_inputManager == null) return;
        m_inputManager.JumpPressed -= OnJumpPressed;
        m_inputManager.CrouchPressed -= OnCrouchPressed;
        m_inputManager.SprintPressed -= OnSprintPressed;
        m_inputManager.SprintReleased -= OnSprintReleased;
    }

    public void ProcessBufferedInputs(PlayerPhysicsContext context, ref Vector3 modifiedVelocity, float baseSpeed)
    {
        if (m_wantsToJump)
        {
            m_wantsToJump = false;
            ProcessJump(context, ref modifiedVelocity, baseSpeed);
        }
        if (m_wantsToCrouch)
        {
            m_wantsToCrouch = false;
            ProcessCrouch(modifiedVelocity, baseSpeed);
        }
        if (!m_isSprintHeld || m_player.CurrentState is SprintState) return;
        TryReenterSprint();
    }

    void ProcessJump(PlayerPhysicsContext context, ref Vector3 modifiedVelocity, float baseSpeed)
    {
        if (m_player.CurrentState is not ControllableGroundState controllable) return;
        PlayerMovementState nextState = m_player.CurrentState.OnJumpPressed(modifiedVelocity, baseSpeed);
        controllable.ProcessJump(context, ref modifiedVelocity);
        if (nextState == m_player.CurrentState) return;
        m_player.TransitionToState(nextState);
    }

    void ProcessCrouch(Vector3 velocity, float baseSpeed)
    {
        PlayerMovementState nextState = m_player.CurrentState.OnCrouchPressed(velocity, baseSpeed);
        if (nextState == null || nextState == m_player.CurrentState) return;
        m_player.TransitionToState(nextState);
    }

    void TryReenterSprint()
    {
        if (m_player.CurrentState is not (WalkState or CrouchState)) return;
        PlayerMovementState nextState = m_player.CurrentState.OnSprintPressed();
        if (nextState == m_player.CurrentState) return;
        m_player.TransitionToState(nextState);
    }

    void OnJumpPressed()
    {
        m_wantsToJump = true;
    }

    void OnCrouchPressed()
    {
        m_wantsToCrouch = true;
    }

    void OnSprintPressed()
    {
        m_isSprintHeld = true;
        PlayerMovementState nextState = m_player.CurrentState.OnSprintPressed();
        if (nextState == m_player.CurrentState) return;
        m_player.TransitionToState(nextState);
    }

    void OnSprintReleased()
    {
        m_isSprintHeld = false;
        PlayerMovementState nextState = m_player.CurrentState.OnSprintReleased();
        if (nextState == m_player.CurrentState) return;
        m_player.TransitionToState(nextState);
    }

    public void HandleInputDisabled()
    {
        m_wantsToJump = false;
        m_wantsToCrouch = false;
        m_isSprintHeld = false;
        if (m_player.CurrentState != m_player.SprintState && m_player.CurrentState != m_player.SlideState) return;
        m_player.TransitionToState(m_player.WalkState);
    }
}