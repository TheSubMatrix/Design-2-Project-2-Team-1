
public struct SanityChange: IEvent
{
    public SanityChange(int sanityDifference)
    {
        SanityDifference = sanityDifference;
    }
    public readonly int SanityDifference;
}
