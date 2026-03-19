using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Events/BatchesReady")]
public class BatchesReadyEventChannelSO : ScriptableObject
{
    public event System.Action<List<List<ExperienceData>>> OnRaised;
    public void Raise(List<List<ExperienceData>> batches) => OnRaised?.Invoke(batches);
}