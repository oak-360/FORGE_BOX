// Scripts/Events/MutableSystemEventChannelSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Mutable System Event Channel")]
public class MutableSystemEventChannelSO : EventChannelSO<MutableSystemEvent>
{
    // You can add any specific functionality for this event channel here
    // For example, custom methods or validation
}

// Define the MutableSystemEvent class that will be passed through the event
public class MutableSystemEvent
{
    // Add any data you want to pass through the event
    public enum EventType { LibraryReady, ProcessingComplete, Error }
    
    public EventType Type { get; }
    public string Message { get; }
    public object Data { get; }

    public MutableSystemEvent(EventType type, string message = "", object data = null)
    {
        Type = type;
        Message = message;
        Data = data;
    }
}