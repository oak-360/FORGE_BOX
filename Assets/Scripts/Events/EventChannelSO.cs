using UnityEngine;
using System;

[Serializable]
public class EventChannelSO<T> : ScriptableObject
{
    public event Action<T> OnRaised;

    [TextArea] public string description;
    
    public void Raise(T value)
    {
        Debug.Log($"[{name}] Event raised with value: {value}");
        OnRaised?.Invoke(value);
    }
}
