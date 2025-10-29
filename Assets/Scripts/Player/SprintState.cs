using UnityEngine;

[System.Serializable]
public class SprintState : ControllableGroundState
{
    [SerializeField] float m_sprintSpeedMultiplier = 1.5f;
    [SerializeField] float m_minSlideSpeed = 6f;
    [SerializeField, Range(0f, 1f)] float m_forwardSprintThreshold = 0.6f;
    [SerializeField, Range(0f, 1f)] float m_minSprintSpeedPercent = 0.8f;
    
    public override float GetMoveSpeed(float baseSpeed, float speedPercent)
    {
        return (baseSpeed * m_sprintSpeedMultiplier) * speedPercent;
    }
    
    public override void CalculateMovement(ref Vector3 modifiedVelocity, Vector2 inputDirection, 
        Transform orientation, float baseSpeed, float speedPercent, PlayerPhysicsContext context, float deltaTime)
    {
        // Check if player is moving forward enough to maintain sprint
        float forwardInput = inputDirection.y;
        
        if (forwardInput < m_forwardSprintThreshold)
        {
            // Not moving forward enough, drop to walk speed
            // But stay in SprintState (player must release sprint button to exit)
            float walkSpeed = baseSpeed * speedPercent;
            inputDirection = inputDirection.normalized;
            
            Vector3 forward = orientation.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = orientation.right;
            right.y = 0f;
            right.Normalize();
            
            Vector3 desiredVelocity = (forward * inputDirection.y + right * inputDirection.x) * walkSpeed;
            
            // Apply movement with walk speed
            ApplyMovementPhysics(ref modifiedVelocity, desiredVelocity, context, deltaTime);
        }
        else
        {
            // Moving forward enough, use sprint speed
            base.CalculateMovement(ref modifiedVelocity, inputDirection, orientation, baseSpeed, speedPercent, context, deltaTime);
        }
    }
    
    void ApplyMovementPhysics(ref Vector3 modifiedVelocity, Vector3 desiredVelocity, PlayerPhysicsContext context, float deltaTime)
    {
        bool isGrounded = context.IsGrounded;
        float currentAcceleration = isGrounded ? context.MaxGroundAcceleration : context.MaxAirAcceleration;
        float currentDeceleration = isGrounded ? context.MaxGroundDeceleration : context.MaxAirDeceleration;
        
        Vector3 xAxis = PlayerPhysicsContext.ProjectOnContactPlane(Vector3.right, context.ContactNormal).normalized;
        Vector3 zAxis = PlayerPhysicsContext.ProjectOnContactPlane(Vector3.forward, context.ContactNormal).normalized;

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
    
    public override PlayerMovementState OnJumpPressed(Vector3 currentVelocity, float baseSpeed)
    {
        Vector3 horizontalVel = new(currentVelocity.x, 0f, currentVelocity.z);
        float sprintSpeed = baseSpeed * m_sprintSpeedMultiplier;
        return horizontalVel.magnitude >= sprintSpeed * m_minSprintSpeedPercent ? this : Player.WalkState;
    }

    public override PlayerMovementState OnCrouchPressed(Vector3 currentVelocity, float baseSpeed)
    {
        if (!Player.PhysicsContext.IsGrounded) return Player.CrouchState;
        Vector3 horizontalVel = new(currentVelocity.x, 0f, currentVelocity.z);
        return horizontalVel.magnitude >= m_minSlideSpeed ? Player.SlideState : Player.CrouchState;
    }

    public override PlayerMovementState OnSprintReleased() => Player.WalkState;
}