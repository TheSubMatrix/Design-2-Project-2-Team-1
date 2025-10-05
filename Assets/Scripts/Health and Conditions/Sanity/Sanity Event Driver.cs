using UnityEngine;

public class SanityEventDriver : MonoBehaviour
{
    public void ChangeSanity(int amount)
    {
        EventBus<SanityChange>.Raise(new SanityChange(amount));
    }
}
