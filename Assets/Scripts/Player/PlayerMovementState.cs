using UnityEngine;

[System.Serializable]
public abstract class PlayerMovementState
{
    [SerializeField, Range(0, 100)] protected float m_stateHeightPercent = 100;
    protected PlayerMovement Player;
    
    public void Initialize(PlayerMovement player) => Player = player;
    
    public virtual void Enter() { }
    public virtual void Exit() { }
    
    public abstract float GetMoveSpeed(float baseSpeed, float speedPercent);
    
    public virtual void UpdateCapsuleHeight(CapsuleCollider collider, float playerHeight, 
        float heightChangeSpeed, int ceilingContactCount, float deltaTime)
    {
        float targetHeight = playerHeight * (m_stateHeightPercent / 100f);

        // Don't stand up if there's a ceiling
        if (targetHeight > collider.height && ceilingContactCount > 0)
            return;

        if (!Mathf.Approximately(collider.height, targetHeight))
        {
            collider.height = Mathf.MoveTowards(
                collider.height, 
                targetHeight, 
                heightChangeSpeed * deltaTime
            );
        }
    }
    
    public abstract void SetPhysicsParameters(PlayerPhysicsContext context);
    public abstract void UpdatePhysics(PlayerPhysicsContext context);
    public abstract void CalculateMovement(ref Vector3 modifiedVelocity, Vector2 inputDirection, 
        Transform orientation, float baseSpeed, float speedPercent, PlayerPhysicsContext context, float deltaTime);
    public virtual void FixedUpdate(PlayerPhysicsContext context) { }
    
    // Input handlers - states return the next state to transition to
    public virtual PlayerMovementState OnJumpPressed(Vector3 currentVelocity, float baseSpeed) => this;
    public virtual PlayerMovementState OnCrouchPressed(Vector3 currentVelocity, float baseSpeed) => this;
    public virtual PlayerMovementState OnSprintPressed() => this;
    public virtual PlayerMovementState OnSprintReleased() => this;
}