using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ARLibraryManager : MonoBehaviour
{
    [SerializeField] private float widthInMeters = 0.1f;
    [SerializeField] private BatchReadyEventChannelSO onBatchReady;
    [SerializeField] private FirstBatchReadyEventChannelSO onFirstBatchReady;
    [SerializeField] private ARManagerRegisteredEventChannelSO onARRegistered;
    
    private ARTrackedImageManager arMgr;
    private MutableRuntimeReferenceImageLibrary mutableLibrary;
    private Texture2D[] batchTextures;
    private int texturesProcessed = 0;
    private int currentBatchIndex = 0;
    private int totalBatches = 1;

    private void OnEnable()
    {
        onARRegistered.OnRaised += RegisterTrackedImageManager;
        onBatchReady.OnRaised += ProcessBatch;
    }

    private void OnDisable()
    {
        onARRegistered.OnRaised -= RegisterTrackedImageManager;
        onBatchReady.OnRaised -= ProcessBatch;
    }

    private void RegisterTrackedImageManager(ARTrackedImageManager mgr)
    {
        arMgr = mgr;
        mutableLibrary = arMgr.CreateRuntimeLibrary() as MutableRuntimeReferenceImageLibrary;
        Debug.Log("[ARLibraryManager] ARTrackedImageManager registered!");
    }

    public void ProcessBatch(List<ExperienceData> batch, int batchIndex, int totalBatches)
    {
        this.currentBatchIndex = batchIndex;
        this.totalBatches = totalBatches;
        
        Debug.Log($"[ARLibraryManager] ProcessBatch called with {batch?.Count ?? 0} experiences, batch {batchIndex + 1} of {totalBatches}");
        
        if (arMgr == null || mutableLibrary == null)
        {
            Debug.LogError("[ARLibraryManager] AR components not initialized - " +
                         $"arMgr: {arMgr != null}, mutableLibrary: {mutableLibrary != null}");
            return;
        }

        if (batch == null || batch.Count == 0)
        {
            Debug.LogWarning("[ARLibraryManager] Batch is null or empty");
            return;
        }

        batchTextures = new Texture2D[batch.Count];
        texturesProcessed = 0;
        Debug.Log($"[ARLibraryManager] Starting to process {batch.Count} experiences in batch {batchIndex + 1}");

        foreach (var exp in batch)
        {
            if (exp.imageTriggerTexture == null)
            {
                Debug.LogError($"[ARLibraryManager] Missing image trigger texture for {exp.experienceName}");
                continue;
            }

            if (exp.imageTriggerTexture.OperationHandle.IsValid() && 
                exp.imageTriggerTexture.OperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var texture = exp.imageTriggerTexture.Asset as Texture2D;
                if (texture != null)
                {
                    AddTextureToLibrary(texture, exp);
                }
            }
            else
            {
                var handle = exp.imageTriggerTexture.LoadAssetAsync<Texture2D>();
                handle.Completed += (operation) => 
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        AddTextureToLibrary(operation.Result, exp);
                    }
                    else
                    {
                        Debug.LogError($"[ARLibraryManager] Failed to load texture for {exp.experienceName}");
                        texturesProcessed++;
                        CheckBatchComplete();
                    }
                };
            }
        }
    }

    private void AddTextureToLibrary(Texture2D texture, ExperienceData exp)
    {
        Debug.Log($"[ARLibraryManager] AddTextureToLibrary - Experience: {exp.experienceName}, " +
                 $"Texture: {texture?.name ?? "null"}, Size: {texture?.width}x{texture?.height}");
                 
        if (texture == null)
        {
            Debug.LogError("[ARLibraryManager] Texture is null");
            texturesProcessed++;
            CheckBatchComplete();
            return;
        }

        if (mutableLibrary == null)
        {
            Debug.LogError("[ARLibraryManager] mutableLibrary is null! Cannot add texture");
            texturesProcessed++;
            CheckBatchComplete();
            return;
        }

        try
        {
            Debug.Log($"[ARLibraryManager] Scheduling image addition for {exp.experienceName} with GUID: {exp.imageReferenceGuid}");
            var jobHandle = mutableLibrary.ScheduleAddImageWithValidationJob(
                texture,
                exp.imageReferenceGuid,
                widthInMeters);
                
            Debug.Log($"[ARLibraryManager] Successfully scheduled image job for {exp.experienceName}");
            texturesProcessed++;
            
            // If this is the first texture of the first batch, notify that tracking can begin
            if (texturesProcessed == 1 && batchTextures.Length > 0)
            {
                arMgr.referenceLibrary = mutableLibrary;
                arMgr.enabled = false;
                arMgr.enabled = true;
                onFirstBatchReady.Raise();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ARLibraryManager] Error adding image for {exp.experienceName}: {e.Message}");
            texturesProcessed++;
        }
        
        CheckBatchComplete();
    }

    private void CheckBatchComplete()
    {
        Debug.Log($"[ARLibraryManager] CheckBatchComplete - Processed: {texturesProcessed}, Total: {batchTextures?.Length ?? 0}");
        
        if (texturesProcessed >= (batchTextures?.Length ?? 0))
        {
            Debug.Log($"[ARLibraryManager] Batch {currentBatchIndex + 1} of {totalBatches} processed successfully");
            
            // If we have more batches, process the next one
            if (currentBatchIndex < totalBatches - 1)
            {
                Debug.Log($"[ARLibraryManager] Starting next batch {currentBatchIndex + 2} of {totalBatches}");
                // You might want to add logic here to process the next batch
            }
            else
            {
                Debug.Log("[ARLibraryManager] All batches processed successfully");
            }
        }
        else
        {
            Debug.LogWarning($"[ARLibraryManager] Batch not complete yet. Processed {texturesProcessed} of {batchTextures?.Length ?? 0} textures");
        }
    }
}