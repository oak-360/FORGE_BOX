using System.Collections.Generic;
using UnityEngine;

public class BatchManager : MonoBehaviour
{
    [SerializeField] private int batchSize = 10;
    [SerializeField] private ExperiencesProcessedEventChannelSO onExperiencesProcessed;
    [SerializeField] private BatchReadyEventChannelSO onBatchReady;
    
    private List<List<ExperienceData>> batches;
    private int currentBatchIndex = 0;

    private void OnEnable()
    {
        onExperiencesProcessed.OnRaised += HandleExperiencesProcessed;
    }

    private void OnDisable()
    {
        onExperiencesProcessed.OnRaised -= HandleExperiencesProcessed;
    }

    private void HandleExperiencesProcessed(List<ExperienceData> experiences)
    {
        batches = Chunk(experiences, batchSize);
        currentBatchIndex = 0;
        
        if (batches.Count > 0)
        {
            ProcessNextBatch();
        }
    }

    public void ProcessNextBatch()
    {
        if (batches == null || currentBatchIndex >= batches.Count)
        {
            Debug.Log("[BatchManager] No more batches to process");
            return;
        }

        var currentBatch = batches[currentBatchIndex];
        Debug.Log($"[BatchManager] Processing batch {currentBatchIndex + 1} of {batches.Count} with {currentBatch.Count} experiences");
        
        onBatchReady.Raise(currentBatch, currentBatchIndex, batches.Count);
        currentBatchIndex++;
    }

    private List<List<T>> Chunk<T>(List<T> list, int chunkSize)
    {
        var chunks = new List<List<T>>();
        for (int i = 0; i < list.Count; i += chunkSize)
        {
            chunks.Add(list.GetRange(i, Mathf.Min(chunkSize, list.Count - i)));
        }
        return chunks;
    }
}