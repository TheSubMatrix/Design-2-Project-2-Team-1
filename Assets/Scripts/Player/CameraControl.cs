using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraControl : MonoBehaviour
{

    [FormerlySerializedAs("xMouseSensitivity")] public float m_xMouseSensitivity = 10f;
    [FormerlySerializedAs("yMouseSensitivity")] public float m_yMouseSensitivity = 10f;
    [FormerlySerializedAs("cam")] public Transform m_cam;
    [FormerlySerializedAs("orientation")] public Transform m_orientation;

    Vector2 m_mousePos;
    Vector2 m_rotation;
    bool m_shouldReadInput = true;

    EventBinding<PlayerMovement.UpdatePlayerInputState> m_updateInputStateEvent;
    void OnEnable()
    {
        m_updateInputStateEvent = new EventBinding<PlayerMovement.UpdatePlayerInputState>(HandleInputReadChange);
        EventBus<PlayerMovement.UpdatePlayerInputState>.Register(m_updateInputStateEvent);
    }
    
    void OnDisable()
    {
        EventBus<PlayerMovement.UpdatePlayerInputState>.Deregister(m_updateInputStateEvent);
    }
    void HandleInputReadChange(PlayerMovement.UpdatePlayerInputState state)
    {
        m_shouldReadInput = state.DesiredInputState;
    }
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale <= 0 || !m_shouldReadInput) return;
        //get the mouse x and y values
        m_mousePos.x = Input.GetAxisRaw("Mouse X");
        m_mousePos.y = Input.GetAxisRaw("Mouse Y");
        //set the rotation values based on the sensitivity
        m_rotation.y += m_mousePos.x * m_xMouseSensitivity;
        m_rotation.x -= m_mousePos.y * m_yMouseSensitivity;
        //clamp the x rotation so that the camera can't rotate past a certain point on that axis
        m_rotation.x = Mathf.Clamp(m_rotation.x, -90f, 90f);
        //transform the camera's rotation based on the values
        m_cam.transform.localRotation = Quaternion.Euler(m_rotation.x, m_rotation.y, 0);
        //transform the orientation value so it can be used as the forward in the player movement script
        m_orientation.transform.localRotation = Quaternion.Euler(0, m_rotation.y, 0);

    }


}