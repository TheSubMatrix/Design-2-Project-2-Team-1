using UnityEngine;

[System.Serializable]
public class WalkState : ControllableGroundState
{
    public override float GetMoveSpeed(float baseSpeed, float speedPercent) => baseSpeed * speedPercent;
    public override PlayerMovementState OnJumpPressed(Vector3 currentVelocity, float baseSpeed) => this;
    public override PlayerMovementState OnCrouchPressed(Vector3 currentVelocity, float baseSpeed) => Player.CrouchState;
    public override PlayerMovementState OnSprintPressed() => Player.SprintState;
}