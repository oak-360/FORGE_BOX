using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;

public class MutableLibraryManager : MonoBehaviour
{
    // Event Channels
    public ARManagerRegisteredEventChannelSO onARRegistered;
    public SceneLoadedEventChannelSO onSceneLoaded;
    public BatchesReadyEventChannelSO onBatchesReady;
    public FirstBatchReadyEventChannelSO onFirstBatchReady;

    // Configuration
    [Header("Configuration")]
    public int overrideBatchSize = 10; // Default batch size
    public float widthInMeters = 0.1f; // Default image width

    // References
    private ARTrackedImageManager arMgr;
    private List<List<ExperienceData>> batches;
    private int currentBatch = 0;
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    
    // User and device data
    private UserProfile userProfile;
    private DeviceData deviceData;

    void OnEnable()
    {
        // Subscribe to all events
        onARRegistered.OnRaised += RegisterTrackedImageManager;
        onSceneLoaded.OnRaised += HandleSceneLoaded;
        onBatchesReady.OnRaised += HandleBatchesReady;
        onFirstBatchReady.OnRaised += HandleFirstBatchReady;
    }

    void OnDisable()
    {
        // Unsubscribe from all events
        onARRegistered.OnRaised -= RegisterTrackedImageManager;
        onSceneLoaded.OnRaised -= HandleSceneLoaded;
        onBatchesReady.OnRaised -= HandleBatchesReady;
        onFirstBatchReady.OnRaised -= HandleFirstBatchReady;
    }
    
    private void Start()
    {
        // Load user profile (or create a new one)
        userProfile = LoadOrCreateUserProfile();
        
        // Collect device data
        deviceData = DeviceDataCollector.CollectDeviceData();
        
        Debug.Log($"[MutableLibraryManager] Initialized with user: {userProfile.userId}");
    }
    
    private UserProfile LoadOrCreateUserProfile()
    {
        // TODO: Implement actual user profile loading/saving
        return new UserProfile
        {
            userId = SystemInfo.deviceUniqueIdentifier,
            deviceModel = SystemInfo.deviceModel,
            operatingSystem = SystemInfo.operatingSystem,
            deviceLanguage = Application.systemLanguage.ToString(),
            timeZone = TimeZoneInfo.Local.StandardName,
            interests = new List<string>(),
            preferredCategories = new List<string>(),
            completedExperiences = new List<string>(),
            experienceInteractionCount = new Dictionary<string, int>(),
            allowLocationTracking = true,
            allowAnalytics = true
        };
    }
    
    public void ProcessExperiences(List<ExperienceData> allExperiences)
    {
        if (allExperiences == null || allExperiences.Count == 0)
        {
            Debug.LogWarning("[MutableLibraryManager] No experiences to process");
            return;
        }
        
        Debug.Log($"[MutableLibraryManager] Processing {allExperiences.Count} experiences");
        
        // Filter experiences based on user and device data
        var filteredExperiences = ExperienceFilter.FilterExperiences(
            allExperiences, 
            userProfile, 
            deviceData);
            
        Debug.Log($"[MutableLibraryManager] Filtered to {filteredExperiences.Count} relevant experiences");
        
        if (filteredExperiences.Count == 0)
        {
            Debug.LogWarning("[MutableLibraryManager] No experiences passed the filter criteria");
            return;
        }
            
        // Process the filtered experiences in batches
        ProcessInBatches(filteredExperiences);
    }
    
    private void ProcessInBatches(List<ExperienceData> experiences)
    {
        // Create batches from the filtered experiences
        batches = ExperienceBatcher.Chunk(experiences, overrideBatchSize);
        Debug.Log($"[MutableLibraryManager] Created {batches.Count} batches");
        
        if (batches.Count > 0)
        {
            // Notify that batches are ready
            onBatchesReady?.Raise(batches);
            
            // Start processing the first batch
            currentBatch = 0;
            StartCoroutine(ProcessBatch(currentBatch));
        }
    }

    private void RegisterTrackedImageManager(ARTrackedImageManager mgr)
    {
        arMgr = mgr;
        Debug.Log("[MutableLibraryManager] ARTrackedImageManager registered!");
    }

    private void HandleSceneLoaded()
    {
        Debug.Log("[MutableLibraryManager] Scene loaded, starting batch processing...");
        StartCoroutine(ProcessBatches());
    }

    private void HandleBatchesReady(List<List<ExperienceData>> batchList)
    {
        batches = batchList;
        Debug.Log($"[MutableLibraryManager] Received {batches.Count} batches");
        if (batches.Count > 0)
        {
            currentBatch = 0;
            StartCoroutine(ProcessBatch(currentBatch));
        }
    }

    private void HandleFirstBatchReady()
    {
        Debug.Log("[MutableLibraryManager] First batch ready!");
    }

    private IEnumerator ProcessBatches()
    {
        yield return null;

        // Get the experience list from the database
        var dbLoader = FindObjectOfType<ContentDatabaseLoader>();
        if (dbLoader == null || !dbLoader.IsDatabaseLoaded)
        {
            Debug.LogError("[MutableLibraryManager] Database not loaded!");
            yield break;
        }

        // Process experiences with filtering
        ProcessExperiences(dbLoader.experienceList);
    }

    private IEnumerator ProcessBatch(int idx)
    {
        if (arMgr == null)
        {
            Debug.LogError("[MutableLibraryManager] No ARTrackedImageManager registered!");
            yield break;
        }

        var lib = arMgr.CreateRuntimeLibrary() as MutableRuntimeReferenceImageLibrary;
        if (lib == null)
        {
            Debug.LogError("[MutableLibraryManager] Failed to create mutable library!");
            yield break;
        }

        foreach (var exp in batches[idx])
        {
            if (exp.imageTriggerTexture == null)
            {
                Debug.LogError($"[MutableLibraryManager] Missing image trigger texture for {exp.experienceName}");
                continue;
            }

            // Check cache first
            if (textureCache.TryGetValue(exp.imageReferenceGuid, out Texture2D cachedTexture))
            {
                Debug.Log($"[MutableLibraryManager] Using cached texture for {exp.experienceName}");
                ScheduleImageAddition(lib, cachedTexture, exp);
                continue;
            }

            // Load texture
            yield return LoadAndProcessTexture(exp, lib);
        }

        // Apply the library
        arMgr.referenceLibrary = lib;
        Debug.Log($"[MutableLibraryManager] Batch {idx + 1} of {batches.Count} processed");

        // Re-enable the manager to ensure tracking
        arMgr.enabled = false;
        arMgr.enabled = true;

        // If this was the first batch, raise the event
        if (idx == 0)
        {
            onFirstBatchReady.Raise();
        }

        // Process next batch if available
        if (idx < batches.Count - 1)
        {
            currentBatch++;
            StartCoroutine(ProcessBatch(currentBatch));
        }
    }

    private IEnumerator LoadAndProcessTexture(ExperienceData exp, MutableRuntimeReferenceImageLibrary lib)
    {
        Texture2D texture = null;
        AsyncOperationHandle<Texture2D> handle = default;

        if (exp.imageTriggerTexture.OperationHandle.IsValid())
        {
            if (exp.imageTriggerTexture.OperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                texture = exp.imageTriggerTexture.Asset as Texture2D;
            }
            else if (exp.imageTriggerTexture.OperationHandle.Status == AsyncOperationStatus.Failed)
            {
                Addressables.Release(exp.imageTriggerTexture.OperationHandle);
                handle = exp.imageTriggerTexture.LoadAssetAsync<Texture2D>();
                yield return handle;
                
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    texture = handle.Result;
                }
            }
            else
            {
                yield return exp.imageTriggerTexture.OperationHandle;
                if (exp.imageTriggerTexture.OperationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    texture = exp.imageTriggerTexture.Asset as Texture2D;
                }
            }
        }
        else
        {
            handle = exp.imageTriggerTexture.LoadAssetAsync<Texture2D>();
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                texture = handle.Result;
            }
        }

        if (texture != null && texture.isReadable)
        {
            textureCache[exp.imageReferenceGuid] = texture;
            ScheduleImageAddition(lib, texture, exp);
        }
        else
        {
            Debug.LogError($"[MutableLibraryManager] Invalid texture for {exp.experienceName}");
        }
    }

    private void ScheduleImageAddition(MutableRuntimeReferenceImageLibrary lib, Texture2D texture, ExperienceData exp)
    {
        try
        {
            lib.ScheduleAddImageWithValidationJob(
                texture,
                exp.imageReferenceGuid,
                widthInMeters);
                
            // Update interaction count
            if (!userProfile.experienceInteractionCount.ContainsKey(exp.experienceName))
            {
                userProfile.experienceInteractionCount[exp.experienceName] = 0;
            }
            userProfile.experienceInteractionCount[exp.experienceName]++;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MutableLibraryManager] Error adding image for {exp.experienceName}: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        // Clear the texture cache
        textureCache.Clear();
    }
}