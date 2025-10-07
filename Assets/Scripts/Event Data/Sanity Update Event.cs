using UnityEngine;

public struct SanityUpdateEvent : IEvent
{
    public readonly uint Sanity;
    public readonly uint SanityMax;
    public SanityUpdateEvent(uint sanity, uint sanityMax)
    {
        Sanity = sanity;
        SanityMax = sanityMax;
    }
}
