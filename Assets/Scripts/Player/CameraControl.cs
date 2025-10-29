using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] float m_xMouseSensitivity = 10f;
    [SerializeField] float m_yMouseSensitivity = 10f;
    [SerializeField] Transform m_cam;
    [SerializeField] Transform m_orientation;
    [SerializeField, RequiredField] PlayerInputManager m_inputManager;

    Vector2 m_rotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Time.timeScale <= 0 || m_inputManager is null) return;

        Vector2 mouseInput = m_inputManager.LookInput;
        
        // Set the rotation values based on the sensitivity
        m_rotation.y += mouseInput.x * m_xMouseSensitivity;
        m_rotation.x -= mouseInput.y * m_yMouseSensitivity;
        
        // Clamp the x rotation so that the camera can't rotate past a certain point on that axis
        m_rotation.x = Mathf.Clamp(m_rotation.x, -90f, 90f);
        
        // Transform the camera's rotation based on the values
        m_cam.transform.localRotation = Quaternion.Euler(m_rotation.x, m_rotation.y, 0);
        
        // Transform the orientation value so it can be used as the forward in the Player movement script
        m_orientation.transform.localRotation = Quaternion.Euler(0, m_rotation.y, 0);
    }
}