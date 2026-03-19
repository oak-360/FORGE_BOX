using UnityEngine;

[CreateAssetMenu(menuName="Events/SceneLoaded")]
public class SceneLoadedEventChannelSO : ScriptableObject
{
    public event System.Action OnRaised;
    public void Raise() => OnRaised?.Invoke();
}