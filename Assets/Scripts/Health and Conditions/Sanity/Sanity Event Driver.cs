using UnityEngine;

public class SanityEventDriver : MonoBehaviour
{
    public void ChangeSanity(int amount)
    {
        EventBus<RequestSanityChange>.Raise(new RequestSanityChange(amount));
    }
}
