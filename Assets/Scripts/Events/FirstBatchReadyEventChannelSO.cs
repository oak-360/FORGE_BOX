using UnityEngine;

[CreateAssetMenu(menuName="Events/FirstBatchReady")]
public class FirstBatchReadyEventChannelSO : ScriptableObject
{
    public event System.Action OnRaised;
    public void Raise() => OnRaised?.Invoke();
}