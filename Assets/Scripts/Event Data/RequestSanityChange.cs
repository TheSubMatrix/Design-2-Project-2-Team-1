
public struct RequestSanityChange: IEvent
{
    public RequestSanityChange(int sanityDifference)
    {
        SanityDifference = sanityDifference;
    }
    public readonly int SanityDifference;
}
