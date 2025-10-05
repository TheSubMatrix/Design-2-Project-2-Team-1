using UnityEngine;

public struct BarUpdateEvent : IEvent
{
    public string BarName;
    public float BarPercentage;

    public BarUpdateEvent(string barName, float barPercentage)
    {
        BarName = barName;
        BarPercentage = barPercentage;
    }
}
