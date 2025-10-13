using AudioSystem;
using UnityEngine;

public abstract class FloatingPickup : MonoBehaviour, IInteractable
{
    [SerializeField] protected float RotationSpeed = 100;
    [SerializeField] protected float BobbingSpeed = 1;
    [SerializeField] protected float BobbingAmount = 0.25f;
    [SerializeField] protected SoundData PickupSound;
    protected virtual void Update()
    {
        transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y + (Mathf.Sin(Time.time * BobbingSpeed) * Time.deltaTime * BobbingAmount), transform.position.z);
    }

    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        SoundManager.Instance.CreateSound().WithSoundData(PickupSound).WithPosition(transform.position).WithRandomPitch().Play();
        OnPickup(interactor);
    }
    protected abstract void OnPickup(MonoBehaviour interactor);
}
