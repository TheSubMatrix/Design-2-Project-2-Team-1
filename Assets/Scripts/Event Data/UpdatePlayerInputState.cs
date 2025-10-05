
public struct UpdatePlayerInputState : IEvent
{
    public UpdatePlayerInputState(bool state)
    {
        DesiredInputState = state;
    }
    public readonly bool DesiredInputState;
}