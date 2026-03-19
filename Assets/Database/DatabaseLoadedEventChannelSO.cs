using UnityEngine;

[CreateAssetMenu(menuName="Events/DatabaseLoaded")]
public class DatabaseLoadedEventChannelSO : ScriptableObject
{
    public event System.Action OnRaised;
    public void Raise() => OnRaised?.Invoke();
}