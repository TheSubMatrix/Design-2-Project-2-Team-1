using UnityEngine;

public class PlayerPhysicsContext
{
    // References (set each frame)
    public Rigidbody Rb { get; set; }
    public CapsuleCollider PlayerCollider { get; set; }
    
    // Configuration (set each frame)
    public float MaxGroundAngle { get; set; }
    public float SteepDetectionTolerance { get; set; }
    public bool EnableGroundSnapping { get; set; }
    public float SnapProbeDistance { get; set; }
    public float MaxGroundAcceleration { get; set; }
    public float MaxGroundDeceleration { get; set; }
    public float MaxAirAcceleration { get; set; }
    public float MaxAirDeceleration { get; set; }
    public float JumpHeight { get; set; }
    
    // Frame state (accumulated during collision detection)
    public Vector3 ContactNormal { get; private set; }
    public int GroundContactCount { get; private set; }
    public int SteepContactCount { get; private set; }
    public int CeilingContactCount { get; set; }
    public Vector3 SteepNormal { get; private set; }
    
    // Connection state (persistent across frames within context)
    public Vector3 ConnectionVelocity { get; set; }
    public Rigidbody CurrentConnectedBody { get; set; }
    public Rigidbody LastConnectedBody { get; set; }
    public Vector3 ConnectionWorldPosition { get; set; }
    public Vector3 ConnectionLocalPosition { get; set; }
    
    // Step counters (persistent)
    public double StepsSinceGrounded { get; set; }
    public double StepsSinceJump { get; set; }
    
    // Computed properties
    public bool IsGrounded => GroundContactCount > 0;
    public Vector3 RbPosition => Rb?.position ?? Vector3.zero;
    public Vector3 RbVelocity => Rb?.linearVelocity ?? Vector3.zero;
    
    public void Reset()
    {
        ContactNormal = Vector3.zero;
        GroundContactCount = 0;
        SteepContactCount = 0;
        CeilingContactCount = 0;
        SteepNormal = Vector3.zero;
        ConnectionVelocity = Vector3.zero;
        LastConnectedBody = CurrentConnectedBody;
        CurrentConnectedBody = null;
    }
    
    public void IncrementStepCounters()
    {
        StepsSinceJump++;
        StepsSinceGrounded++;
    }

    public void ResetStepsSinceGrounded() => StepsSinceGrounded = 0;
    public void ResetStepsSinceJump() => StepsSinceJump = 0;
    
    public void NormalizeContactNormal()
    {
        if (ContactNormal != Vector3.zero) ContactNormal = ContactNormal.normalized;
    }
    
    public void ResetContactNormalToUp() => ContactNormal = Vector3.up;
    
    public void SetContactNormal(Vector3 normal) 
    {
        ContactNormal = normal != Vector3.zero ? normal.normalized : Vector3.up;
    }
    
    public void SetGroundContactCount(int count) => GroundContactCount = count;
    
    public void EvaluateCollision(Collision collision)
    {
        float minGroundDotProduct = Mathf.Cos(MaxGroundAngle * Mathf.Deg2Rad);
        
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                GroundContactCount++;
                ContactNormal += normal;
                CurrentConnectedBody = collision.rigidbody;
            }
            else if (normal.y > SteepDetectionTolerance)
            {
                SteepContactCount++;
                SteepNormal += normal;
                if (GroundContactCount == 0)
                    CurrentConnectedBody = collision.rigidbody;
            }
            else if (normal.y < -SteepDetectionTolerance)
            {
                CeilingContactCount++;
            }
        }
    }
    public static Vector3 ProjectOnContactPlane(Vector3 vector, Vector3 normal)
    {
        return vector - normal * Vector3.Dot(vector, normal);
    }
}