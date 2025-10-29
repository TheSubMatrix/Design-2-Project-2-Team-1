using UnityEngine;

[System.Serializable]
public abstract class GroundedMovementState : PlayerMovementState
{
    [Header("State Physics Parameters")]
    [SerializeField] protected float m_maxAirAcceleration;
    [SerializeField] protected float m_maxAirDeceleration;
    [SerializeField] protected float m_jumpHeight;
    
    [Header("State Grounding Parameters")]
    [SerializeField] protected float m_maxGroundAngle = 90f;
    [SerializeField] protected float m_steepDetectionTolerance = 0.02f;
    [SerializeField] protected bool m_enableGroundSnapping;
    [SerializeField, ConditionalVisibility(nameof(m_enableGroundSnapping), true)] 
    protected float m_snapProbeDistance;
    
    public override void SetPhysicsParameters(PlayerPhysicsContext context)
    {
        // Set physics values
        context.MaxAirAcceleration = m_maxAirAcceleration;
        context.MaxAirDeceleration = m_maxAirDeceleration;
        context.JumpHeight = m_jumpHeight;
        
        // Set grounding values
        context.MaxGroundAngle = m_maxGroundAngle;
        context.SteepDetectionTolerance = m_steepDetectionTolerance;
        context.EnableGroundSnapping = m_enableGroundSnapping;
        context.SnapProbeDistance = m_snapProbeDistance;
    }
    
    public override void UpdatePhysics(PlayerPhysicsContext context)
    {
        context.IncrementStepCounters();
        UpdateConnectedBodyState(context);

        if (context.IsGrounded || TrySnapToGround(context) || TrySteepContactAsGround(context))
        {
            context.ResetStepsSinceGrounded();
            if (context.GroundContactCount > 1)
                context.NormalizeContactNormal();
        }
        else
        {
            context.ResetContactNormalToUp();
        }
    }

    protected void UpdateConnectedBodyState(PlayerPhysicsContext context)
    {
        if (context.CurrentConnectedBody is null ||
            (!context.CurrentConnectedBody.isKinematic &&
             !(context.CurrentConnectedBody.mass >= context.Rb.mass))) return;
             
        if (context.CurrentConnectedBody == context.LastConnectedBody)
        {
            Vector3 newWorldPos = context.CurrentConnectedBody.transform.TransformPoint(context.ConnectionLocalPosition);
            context.ConnectionVelocity = (newWorldPos - context.ConnectionWorldPosition) / Time.deltaTime;
        }

        context.ConnectionWorldPosition = context.Rb.position;
        context.ConnectionLocalPosition = context.CurrentConnectedBody.transform.InverseTransformPoint(context.Rb.position);
    }

    protected bool TrySnapToGround(PlayerPhysicsContext context)
    {
        if (!context.EnableGroundSnapping || 
            context.StepsSinceGrounded > 1 || 
            context.StepsSinceJump <= 2)
            return false;
        
        if (!Physics.Raycast(context.RbPosition, Vector3.down, out RaycastHit hit, 
                            context.SnapProbeDistance))
            return false;
        
        float minGroundDotProduct = Mathf.Cos(context.MaxGroundAngle * Mathf.Deg2Rad);
        if (hit.normal.y < minGroundDotProduct)
            return false;
        context.SetGroundContactCount(1); 
        context.SetContactNormal(hit.normal);
        
        Vector3 currentVelocity = context.RbVelocity;
        float upwardSpeed = Vector3.Dot(currentVelocity, hit.normal);
        
        if (upwardSpeed > 0f)
        {
            Vector3 groundVelocity = currentVelocity - hit.normal * upwardSpeed;
            context.Rb.linearVelocity = groundVelocity; 
        }
        
        context.CurrentConnectedBody = hit.rigidbody;
        return true;
    }

    protected bool TrySteepContactAsGround(PlayerPhysicsContext context)
    {
        if (context.SteepContactCount <= 1) return false;
        
        Vector3 steepNormal = context.SteepNormal.normalized;
        float minGroundDotProduct = Mathf.Cos(context.MaxGroundAngle * Mathf.Deg2Rad);

        if (steepNormal.y < minGroundDotProduct) return false;
        
        context.SetGroundContactCount(1);
        context.SetContactNormal(steepNormal);
        return true;
    }
}