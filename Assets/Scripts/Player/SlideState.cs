using UnityEngine;

[System.Serializable]
public class SlideState : ControllableGroundState
{
    [SerializeField] float m_slideStopThreshold = 3f;
    [SerializeField] float m_maxSlideDeceleration = 1f;
    
    public override float GetMoveSpeed(float baseSpeed, float speedPercent) => 0;
    
    public override void CalculateMovement(ref Vector3 modifiedVelocity, Vector2 inputDirection, Transform orientation, float baseSpeed, float speedPercent, PlayerPhysicsContext context, float deltaTime)
    {
        Vector3 xAxis = PlayerPhysicsContext.ProjectOnContactPlane(Vector3.right, context.ContactNormal).normalized;
        Vector3 zAxis = PlayerPhysicsContext.ProjectOnContactPlane(Vector3.forward, context.ContactNormal).normalized;

        Vector3 relativeVelocity = modifiedVelocity - context.ConnectionVelocity;
        
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);
        
        float slideDeceleration = m_maxSlideDeceleration * deltaTime;
        float newX = Mathf.MoveTowards(currentX, 0, slideDeceleration);
        float newZ = Mathf.MoveTowards(currentZ, 0, slideDeceleration);
        
        modifiedVelocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    public override void FixedUpdate(PlayerPhysicsContext context)
    {
        Vector3 horizontalVel = new(context.Rb.linearVelocity.x, 0f, context.Rb.linearVelocity.z);
        if (!(horizontalVel.magnitude <= m_slideStopThreshold)) return;
        Player.TransitionToState(Player.CrouchState);
    }
    
    public override PlayerMovementState OnJumpPressed(Vector3 currentVelocity, float baseSpeed) => Player.CrouchState;
    public override PlayerMovementState OnCrouchPressed(Vector3 currentVelocity, float baseSpeed) => Player.WalkState;
    public override PlayerMovementState OnSprintPressed() => this;
    public override PlayerMovementState OnSprintReleased() => this;
}