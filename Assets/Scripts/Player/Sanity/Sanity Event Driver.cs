using UnityEngine;

public class SanityEventDriver : MonoBehaviour
{
    public void ChangeSanity(int amount)
    {
        EventBus<Sanity.SanityChange>.Raise(new Sanity.SanityChange(amount));
    }
}
