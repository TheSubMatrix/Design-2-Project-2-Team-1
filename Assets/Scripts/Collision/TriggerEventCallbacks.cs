using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Collider))]
public class TriggerEventCallbacks : MonoBehaviour
{
    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider>
    {
        public bool m_shouldBeInvoked;
    }
    public TriggerEvent OnTriggerEnterEvent = new TriggerEvent();
    public TriggerEvent OnTriggerExitEvent = new TriggerEvent();
    public TriggerEvent OnTriggerStayEvent = new TriggerEvent();
    public Collider ReferencedCollider;
    void Awake()
    {
        ReferencedCollider = GetComponent<Collider>();
        if (ReferencedCollider is null)
        {
            Debug.LogError("Referenced Collider is null");
            return;
        }
        ReferencedCollider.isTrigger = true;
    }
    void OnTriggerEnter(Collider other)
    {
        if (!OnTriggerEnterEvent.m_shouldBeInvoked) return;
        OnTriggerEnterEvent.Invoke(other);
    }
    void OnTriggerExit(Collider other)
    {
        if (!OnTriggerExitEvent.m_shouldBeInvoked) return;
        OnTriggerExitEvent.Invoke(other);
    }
    void OnTriggerStay(Collider other)
    {
        if (!OnTriggerStayEvent.m_shouldBeInvoked) return;
        OnTriggerStayEvent.Invoke(other);
    }
}
