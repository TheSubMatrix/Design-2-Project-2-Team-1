using UnityEngine;

[System.Serializable]
public abstract class ControllableGroundState : GroundedMovementState
{
    [Header("Ground Control Parameters")]
    [SerializeField] protected float m_maxGroundAcceleration;
    [SerializeField] protected float m_maxGroundDeceleration;

    public override void SetPhysicsParameters(PlayerPhysicsContext context)
    {
        base.SetPhysicsParameters(context);
        context.MaxGroundAcceleration = m_maxGroundAcceleration;
        context.MaxGroundDeceleration = m_maxGroundDeceleration;
    }
    
    public override void CalculateMovement(ref Vector3 modifiedVelocity, Vector2 inputDirection, 
        Transform orientation, float baseSpeed, float speedPercent, PlayerPhysicsContext context, float deltaTime)
    {
        // Calculate the desired velocity from input
        float desiredSpeed = GetMoveSpeed(baseSpeed, speedPercent);
        
        Vector3 forward = orientation.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = orientation.right;
        right.y = 0f;
        right.Normalize();
        
        Vector3 desiredVelocity = (forward * inputDirection.y + right * inputDirection.x) * desiredSpeed;
        
        // Apply ground/air movement physics
        bool isGrounded = context.IsGrounded;
        float currentAcceleration = isGrounded ? context.MaxGroundAcceleration : context.MaxAirAcceleration;
        float currentDeceleration = isGrounded ? context.MaxGroundDeceleration : context.MaxAirDeceleration;
        
        Vector3 xAxis = PlayerPhysicsContext.ProjectOnContactPlane(right, context.ContactNormal).normalized;
        Vector3 zAxis = PlayerPhysicsContext.ProjectOnContactPlane(forward, context.ContactNormal).normalized;

        Vector3 relativeVelocity = modifiedVelocity - context.ConnectionVelocity;
        
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float desiredX = Vector3.Dot(desiredVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);
        float desiredZ = Vector3.Dot(desiredVelocity, zAxis);

        float maxAccelerationChange = currentAcceleration * deltaTime;
        float maxDecelerationChange = currentDeceleration * deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredX, 
            Mathf.Abs(currentX) < Mathf.Abs(desiredX) ? maxAccelerationChange : maxDecelerationChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredZ, 
            Mathf.Abs(currentZ) < Mathf.Abs(desiredZ) ? maxAccelerationChange : maxDecelerationChange);
        
        modifiedVelocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    
    public void ProcessJump(PlayerPhysicsContext context, ref Vector3 modifiedVelocity)
    {
        if (!context.IsGrounded) return;
        context.ResetStepsSinceJump();
        Vector3 contactNormal = context.ContactNormal;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * context.JumpHeight);
        float currentUpwardSpeed = Vector3.Dot(modifiedVelocity, contactNormal);
        modifiedVelocity += currentUpwardSpeed > 0f ? contactNormal * Mathf.Max(jumpSpeed - currentUpwardSpeed, 0) : contactNormal * jumpSpeed;
    }
}