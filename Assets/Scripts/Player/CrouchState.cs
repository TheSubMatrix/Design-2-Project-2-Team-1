using UnityEngine;

[System.Serializable]
public class CrouchState : ControllableGroundState
{
    [SerializeField] float m_crouchSpeedMultiplier = 0.6f;
    public override float GetMoveSpeed(float baseSpeed, float speedPercent) => baseSpeed * m_crouchSpeedMultiplier * speedPercent;
    public override PlayerMovementState OnJumpPressed(Vector3 currentVelocity, float baseSpeed) => Player.WalkState;
    public override PlayerMovementState OnCrouchPressed(Vector3 currentVelocity, float baseSpeed) => Player.WalkState;
    public override PlayerMovementState OnSprintPressed() => Player.SprintState;
    
}