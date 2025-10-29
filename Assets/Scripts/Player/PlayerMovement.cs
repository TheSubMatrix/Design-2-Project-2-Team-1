using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour, ISlowable
{
    [Header("Assign in Editor")] 
    [SerializeField] CapsuleCollider m_playerCollider;
    [SerializeField] Transform m_orientation;
    [SerializeField] Rigidbody m_rb;
    [SerializeField, RequiredField] PlayerInputManager m_inputManager;
    
    [Header("General Movement Options")]
    [SerializeField] float m_baseSpeed;
    [SerializeField] float m_heightChangeSpeed;
    [Header("States")]
    [SerializeField] WalkState m_walkState = new();
    [SerializeField] SprintState m_sprintState = new();
    [SerializeField] CrouchState m_crouchState = new();
    [SerializeField] SlideState m_slideState = new();
    
    [Header("Audio"), SerializeField]
    PlayerAudioController m_audioController = new();

    [Header("Unity Events"), SerializeField]
    UnityEvent m_onSlow = new();
    
    
    public WalkState WalkState => m_walkState;
    public SprintState SprintState => m_sprintState;
    public CrouchState CrouchState => m_crouchState;
    public SlideState SlideState => m_slideState;
    public PlayerMovementState CurrentState { get; private set; }
    public float BaseSpeed => m_baseSpeed;
    public PlayerPhysicsContext PhysicsContext { get; } = new();
    public Transform Orientation => m_orientation;
    PlayerMovementInputAdapter m_inputAdapter;
    
    Vector3 m_modifiedVelocity;
    Coroutine m_currentSlowRoutine;
    float m_currentSpeedPercent = 1.0f;
    float m_playerHeight;
    EventBinding<UpdatePlayerInputState> m_updateInputStateEvent;
    
    void Awake()
    {
        m_inputAdapter = new PlayerMovementInputAdapter();
        m_inputAdapter.Initialize(this, m_inputManager);
        m_walkState.Initialize(this);
        m_sprintState.Initialize(this);
        m_crouchState.Initialize(this);
        m_slideState.Initialize(this);
        CurrentState = m_walkState;
    }

    void OnEnable()
    {
        m_updateInputStateEvent = new EventBinding<UpdatePlayerInputState>(HandleInputStateChange);
        EventBus<UpdatePlayerInputState>.Register(m_updateInputStateEvent);
    }
    
    void OnDisable()
    {
        EventBus<UpdatePlayerInputState>.Deregister(m_updateInputStateEvent);
        m_inputAdapter.Cleanup();
    }
    
    void Start()
    {
        m_audioController.Initialize(m_sprintState.GetMoveSpeed(m_baseSpeed, m_currentSpeedPercent));
        m_playerHeight = m_playerCollider.height;
        CurrentState.Enter();
    }

    void HandleInputStateChange(UpdatePlayerInputState state)
    {
        if (state.DesiredInputState) return;
        m_inputAdapter.HandleInputDisabled();
    }

    public void TransitionToState(PlayerMovementState newState)
    {
        if (CurrentState == newState) return;
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    void FixedUpdate()
    {
        PhysicsContext.Rb = m_rb;
        PhysicsContext.PlayerCollider = m_playerCollider;
        CurrentState.SetPhysicsParameters(PhysicsContext);
        
        CurrentState.UpdatePhysics(PhysicsContext);
        m_modifiedVelocity = m_rb.linearVelocity;
        CurrentState.UpdateCapsuleHeight(m_playerCollider, m_playerHeight, m_heightChangeSpeed, PhysicsContext.CeilingContactCount, Time.deltaTime);
        Vector2 input = m_inputAdapter.Input;
        
        CurrentState.CalculateMovement(
            ref m_modifiedVelocity,
            input,
            m_orientation,
            m_baseSpeed,
            m_currentSpeedPercent,
            PhysicsContext,
            Time.deltaTime
        );
        
        m_audioController.UpdateWalkingSound(m_modifiedVelocity, PhysicsContext.IsGrounded);
        m_inputAdapter.ProcessBufferedInputs(PhysicsContext, ref m_modifiedVelocity, m_baseSpeed);
        CurrentState.FixedUpdate(PhysicsContext);
        m_rb.linearVelocity = m_modifiedVelocity;
        PhysicsContext.Reset();
    }
    
    void OnCollisionStay(Collision collision) => PhysicsContext.EvaluateCollision(collision);
    void OnCollisionExit(Collision collision) => PhysicsContext.EvaluateCollision(collision);
    
    public void Slow(float slowAmount, float slowDuration)
    {
        m_onSlow?.Invoke();
        if (m_currentSlowRoutine != null)
        {
            StopCoroutine(m_currentSlowRoutine);
            m_currentSpeedPercent = 1.0f;
        }
        m_currentSlowRoutine = StartCoroutine(SlowForTimeAsync(slowAmount, slowDuration));
    }

    IEnumerator SlowForTimeAsync(float slowAmount, float slowDuration)
    {
        m_currentSpeedPercent -= slowAmount;
        yield return new WaitForSeconds(slowDuration);
        m_currentSpeedPercent = 1;
    }
}