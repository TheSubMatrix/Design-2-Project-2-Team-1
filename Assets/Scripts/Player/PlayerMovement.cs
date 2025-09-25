using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [FormerlySerializedAs("playerCollider")] [Header("Assign in Editor")]
    public CapsuleCollider m_playerCollider;
    [FormerlySerializedAs("orientation")] public Transform m_orientation;
    [FormerlySerializedAs("rb")] public Rigidbody m_rb;
    [FormerlySerializedAs("walkMoveSpeed")] [Header("Movement Options")]
    public float m_walkMoveSpeed;
    [FormerlySerializedAs("sprintMoveSpeed")] public float m_sprintMoveSpeed;
    [FormerlySerializedAs("crouchMoveSpeed")] public float m_crouchMoveSpeed;
    [FormerlySerializedAs("maxGroundAcceleration")] public float m_maxGroundAcceleration;
    [FormerlySerializedAs("maxGroundDeceleration")] public float m_maxGroundDeceleration;
    [FormerlySerializedAs("maxAirAcceleration")] public float m_maxAirAcceleration;
    [FormerlySerializedAs("maxAirDeceleration")] public float m_maxAirDeceleration;
    [FormerlySerializedAs("maxSlideDeceleration")] public float m_maxSlideDeceleration;
    [FormerlySerializedAs("jumpHeight")] public float m_jumpHeight;
    [FormerlySerializedAs("crouchDistance")] public float m_crouchDistance;
    [FormerlySerializedAs("crouchSpeed")] public float m_crouchSpeed;
    [FormerlySerializedAs("maxGroundAngle")] [Header("Controller Options")]
    public float m_maxGroundAngle;
    [FormerlySerializedAs("steepDetectionTolerance")] public float m_steepDetectionTolerance;
    [FormerlySerializedAs("enableGroundSnapping")] public bool m_enableGroundSnapping;
    [FormerlySerializedAs("snapProbeDistance")] public float m_snapProbeDistance;
    
    Rigidbody m_currentConnectedBody;
    Rigidbody m_lastConnectedBody;
    Vector2 m_playerInput;
    Vector3 m_desiredVelocity;
    Vector3 m_modifiedVelocity;
    Vector3 m_currentContactNormal;
    Vector3 m_steepNormal;
    Vector3 m_connectionVelocity;
    Vector3 m_connectionWorldPosition;
    Vector3 m_connectionLocalPosition;
    float m_newX;
    float m_newZ;
    float m_desiredSpeed;
    float m_currentAcceleration;
    float m_currentDeceleration;
    float m_playerHeight;
    float m_crouchedHeight;
    double m_stepsSinceGrounded;
    double m_stepsSinceJump;
    int m_groundContactCount;
    int m_steepContactCount;
    bool m_desiredCrouchState;
    bool m_currentCrouchState;
    bool m_isSliding;
    bool IsGrounded => m_groundContactCount > 0;
    bool m_wantsToJump;

    public void Start()
    {
        m_playerHeight = m_playerCollider.height;
        m_crouchedHeight = m_playerHeight - m_crouchDistance;
    }
    public void Update()
    {
        HandleInputs();
    }
    public void FixedUpdate()
    {
        UpdateStates();
        MovePlayer();
        //if the player is grounded and wants to jump, then jump
        if (m_wantsToJump)
        {
            m_wantsToJump = false;
            if (IsGrounded) Jump();
        }
        //if we want to crouch, and we aren't currently crouched, then crouch
        if(m_desiredCrouchState && !m_currentCrouchState)
        {
            Crouch();
        }
        //if we no longer want to be crouched, and we are currently crouched, then uncrouched
        if(!m_desiredCrouchState && m_currentCrouchState)
        {
            Uncrouch();
        }
        //if we have a certain amount of speed, and we desire to crouch, then slide
        if(m_desiredCrouchState && m_modifiedVelocity.magnitude > m_walkMoveSpeed)
        {
            m_isSliding = true;
            
        }
        //if we want to stop sliding, the player can do so by uncrouching. Automatically stop their slide if their desired move speed is less than or equal to that of the velocity in their desired move direction for smooth transitions between the sliding and crouched state
        if (!m_desiredCrouchState && m_isSliding || Mathf.Clamp(Vector3.Dot(m_modifiedVelocity, m_desiredVelocity), 0, m_desiredSpeed) >= m_modifiedVelocity.magnitude && m_isSliding || m_modifiedVelocity.magnitude <= 0.01 && m_isSliding)
        {
            m_isSliding = false;
        }
        //take the calculated modified velocity and apply it to the rigid body
        m_rb.linearVelocity = m_modifiedVelocity;
        ClearState();
    }
    void Crouch()
    {
        //move the player height towards the height of the desire crouch state
        m_playerCollider.height = Mathf.MoveTowards(m_playerCollider.height, m_crouchedHeight, Time.deltaTime * m_crouchSpeed);
        //change the crouch state if we've reached the desired height
        if (Mathf.Approximately(m_playerCollider.height, m_crouchedHeight))
        {
            m_currentCrouchState = true;
        }
    }
    void Uncrouch()
    {
        //move the player height towards the height of the desire crouch state
        m_playerCollider.height = Mathf.MoveTowards(m_playerCollider.height, m_playerHeight, Time.deltaTime * m_crouchSpeed);
        //change the crouch state if we've reached the desired height
        if (Mathf.Approximately(m_playerCollider.height, m_playerHeight))
        {
            m_currentCrouchState = false;
        }
    }
    void HandleInputs()
    {
        //get the horizontal and vertical movement axis and use them to create a clamped vector 2 to represent the direction we want the player to move
        m_playerInput.x = Input.GetAxis("Horizontal");
        m_playerInput.y = Input.GetAxis("Vertical");
        m_playerInput = Vector2.ClampMagnitude(m_playerInput, 1f);

        //if the sprint key is pressed, and we aren't crouched or sliding, set our desired spreed to the sprint speed
        if (Input.GetButton("Sprint") && !m_desiredCrouchState && !m_isSliding)
        {
            m_desiredSpeed = m_walkMoveSpeed + Mathf.Clamp(Vector3.Dot(new Vector3(m_rb.linearVelocity.x, 0, m_rb.linearVelocity.z).normalized, m_orientation.forward), 0, 1) * (m_sprintMoveSpeed - m_walkMoveSpeed);
        }
        //if we are crouching or sliding, change the desired move speed to crouch move speed so we can smoothly transition between the sliding and crouched state
        else if (m_currentCrouchState || m_isSliding)
        {
            m_desiredSpeed = m_crouchMoveSpeed;
        }
        //otherwise, set our move speed to our walking speed
        else
        {
            m_desiredSpeed = m_walkMoveSpeed;
        }
        if (Input.GetButtonDown("Crouch"))
        {
            m_desiredCrouchState = !m_desiredCrouchState;
        }
        //jump when the jump key is pressed
        m_wantsToJump |= Input.GetButtonDown("Jump");
        //use our inputs to create a velocity representing our desired velocity on the horizontal and vertical axis using the player's orientation
        Vector3 forward = m_orientation.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = m_orientation.right;
        right.y = 0f;
        right.Normalize();
        //Debug.Log(desiredSpeed);
        m_desiredVelocity = (forward * m_playerInput.y + right * m_playerInput.x) * m_desiredSpeed;
    }
    void MovePlayer()
    {
        //change the acceleration and deceleration rate based on whether the character is in the air or grounded
        if (IsGrounded)
        {
            m_currentAcceleration = m_maxGroundAcceleration;
            m_currentDeceleration = m_maxGroundDeceleration;
        }
        else
        {
            m_currentAcceleration = m_maxAirAcceleration;
            m_currentDeceleration = m_maxAirDeceleration;
        }
        //take axis and project them onto the contact plane to use in calculations taking into account the surface normal
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        Vector3 relativeModifiedVelocity = m_modifiedVelocity - m_connectionVelocity;
        //get the modified velocity and desired velocity along each axis we created previously taking into account any moving ground's velocity
        float currentX = Vector3.Dot(relativeModifiedVelocity, xAxis);
        float desiredX = Vector3.Dot(m_desiredVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeModifiedVelocity, zAxis);
        float desiredZ = Vector3.Dot(m_desiredVelocity, zAxis);

        //create a variable to hold what our calculated maximum acceleration per time loop will be
        float maxAccelerationChange = m_currentAcceleration * Time.deltaTime;
        float maxDecelerationChange = m_currentDeceleration * Time.deltaTime;

        if (!m_isSliding)
        {
            //if the speed along the calculated normal X value is lower than the desired speed along the calculated x normal, interpolate using the acceleration value. Otherwise, use the deceleration value
            m_newX = Mathf.MoveTowards(currentX, m_desiredVelocity.x, Mathf.Abs(currentX) < Mathf.Abs(desiredX) ? maxAccelerationChange : maxDecelerationChange);
            //if the speed along the calculated normal Z value is lower than the desired speed along the calculated Z normal, interpolate using the acceleration value. Otherwise, use the deceleration value
            m_newZ = Mathf.MoveTowards(currentZ, m_desiredVelocity.z, Mathf.Abs(currentZ) < Mathf.Abs(desiredZ) ? maxAccelerationChange : maxDecelerationChange);
            //take the modified velocity and add values to it based on calculated surface normal 
        }
        else
        {
            //When sliding, our velocity always moves towards 0 but can still be effected by outside sources like slopes
            m_newX = Mathf.MoveTowards(currentX, 0, m_maxSlideDeceleration);
            m_newZ = Mathf.MoveTowards(currentZ, 0, m_maxSlideDeceleration);
        }

        m_modifiedVelocity += xAxis * (m_newX - currentX) + zAxis * (m_newZ - currentZ);
    }
    void ClearState()
    {
        //reset the states of all variables after they are used so they can be re-acquired
        m_groundContactCount = 0;
        m_steepContactCount = 0;
        m_steepNormal = Vector3.zero;
        m_currentContactNormal = Vector3.zero;
        m_connectionVelocity = Vector3.zero;
        m_lastConnectedBody = m_currentConnectedBody;
        m_currentConnectedBody = null;
    }
    void UpdateStates()
    {
        //add one to the steps since last grounded and since last jumped
        m_stepsSinceJump++;
        m_stepsSinceGrounded++;
        //set the modified velocity to the current velocity so we can modify it and set the rigid body's velocity to the modified value
        m_modifiedVelocity = m_rb.linearVelocity;
        //if there is a body that the player is connected to, then update the connection state if the detected rigidbody is kinematic or has a greater mass than the player's
        if (m_currentConnectedBody)
        {  
            if(m_currentConnectedBody.isKinematic || m_currentConnectedBody.mass >= m_rb.mass)
                UpdateConnectionState();
        }

        if (IsGrounded || SnapToGround() || CheckSteepContacts())
        {
            //when the player hits the ground, set the steps since last grounded to 0 and normalize the contact normal
            m_stepsSinceGrounded = 0;
            if (m_groundContactCount > 1)
            {
                m_currentContactNormal.Normalize();
            }
        }
        else
        {
            //if the player is not grounded, set the contact normal to an upward vector
            m_currentContactNormal = Vector3.up;
        }
    }
    void Jump()
    {
        m_stepsSinceJump = 0;
        //if the player's velocity has some similarity, do the direction of the contact normal 
        if(Vector3.Dot(m_modifiedVelocity, m_currentContactNormal) > 0f)
        {
            m_modifiedVelocity += m_currentContactNormal * Mathf.Max(Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight) - Vector3.Dot(m_modifiedVelocity, m_currentContactNormal));
        }
        //otherwise just jump normally
        else
        {
            m_modifiedVelocity += m_currentContactNormal * Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight);
        }
        m_wantsToJump = false;
    }
    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        //returns the input vector projected on the current contact normal's plane
        return vector - m_currentContactNormal * Vector3.Dot(vector, m_currentContactNormal);
    }
    bool SnapToGround()
    {
        //only try to snap if snapping is enabled
        if (!m_enableGroundSnapping)
        {
            return false;
        }
        //only try to snap when the player has been off the ground for more than one physics step and has not jumped in recent steps
        if(m_stepsSinceGrounded > 1 ||  m_stepsSinceJump <= 2)
        {
            return false;
        }
        //only try to snap if our downward raycast gets a hit
        if(!Physics.Raycast(m_rb.position, Vector3.down, out RaycastHit hit, m_snapProbeDistance))
        {
            return false;
        }
        //only try to snap if our raycast hits a surface within range of the ground angles
        if(hit.normal.y < Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad))
        {
            return false;
        }
        // set the current contact normal to the raycast hit
        m_currentContactNormal = hit.normal;
        //get the current speed which the player is moving
        float speed = m_modifiedVelocity.magnitude;
        //determine how close our desired velocity is to aligning with surface normal
        float dot = Vector3.Dot(m_modifiedVelocity, hit.normal);
        //if the dot product is not already aligned, move the velocity towards aligning with the ground
        if(dot > 0f)
        {
            m_modifiedVelocity = (m_modifiedVelocity-hit.normal).normalized * speed;
        }
        m_currentConnectedBody = hit.rigidbody;
        return true;
    }
    bool CheckSteepContacts()
    {
        //if we see more than one steep contact, then normalize the steep normal to use as virtual ground for player to jump off of
        if (m_steepContactCount <= 1) return false;
        m_steepNormal.Normalize();
        //if the upward normal of the steep contacts is within the ground angle range, create a virtual ground for the player to jump off
        if (!(m_steepNormal.y >= Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad))) return false;
        m_groundContactCount = 1;
        m_currentContactNormal = m_steepNormal;
        return true;
    }
    void UpdateConnectionState()
    {
        if(m_currentConnectedBody == m_lastConnectedBody) 
        {
            //create a new variable to determine the movement between the last connected world position and the current connected saved in local connection position but converted to world space
            Vector3 connectionMovement = m_currentConnectedBody.transform.TransformPoint(m_connectionLocalPosition) - m_connectionWorldPosition;
            //determine our connection velocity by taking the movement determined previously and dividing it by delta time
            m_connectionVelocity = connectionMovement / Time.deltaTime;
        }
        //set the world position variable to the location of the player
        m_connectionWorldPosition = m_rb.position;
        //set the local connection position to the position of the body relative to the connected body
        m_connectionLocalPosition = m_currentConnectedBody.transform.InverseTransformPoint(m_connectionWorldPosition);

    }
    public void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }
    public void OnCollisionExit(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        //look through all collisions and see if any match our ground parameter
        for(int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            //if they do, then set isGrounded to true and add the normals of the collision to calculate the average.
            //Also keep track of the rigidbody of the object collided with so we can move the player if the surface is moving
            if(normal.y >= Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad))
            {
                m_groundContactCount++;
                m_currentContactNormal += normal;
                m_currentConnectedBody = collision.rigidbody;
            }
            //if the upward normal of our steep tolerance is within a certain threshold, then add it to the steep contacts count
            else if (normal.y > m_steepDetectionTolerance)
            {
                m_steepContactCount++;
                m_steepNormal += normal;
                //if there are no ground contacts, set the connected rigidbody in case there are no ground contacts
                if(m_groundContactCount == 0)
                {
                    m_currentConnectedBody = collision.rigidbody;
                }
            }
            
        }
    }
}