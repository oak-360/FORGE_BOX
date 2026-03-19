using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Events/BatchReady")]
public class BatchReadyEventChannelSO : ScriptableObject
{
    public event System.Action<List<ExperienceData>, int, int> OnRaised;
    
    public void Raise(List<ExperienceData> batch, int batchIndex, int totalBatches)
    {
        Debug.Log($"[BatchReadyEvent] Raising event for batch {batchIndex + 1} of {totalBatches} with {batch.Count} items");
        OnRaised?.Invoke(batch, batchIndex, totalBatches);
    }
}
