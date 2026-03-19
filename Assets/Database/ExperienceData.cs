using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Collections.Generic;

[System.Serializable]
public class ExperienceData
{
    // Basic experience info
    public string experienceName;
    public string details;
    public AssetReferenceTexture2D thumbnail;
    public AssetReferenceTexture2D imageTriggerTexture;
    public string imageReferenceGuid;
    public SceneSetupSO sceneSetup;
    
    // Location data
    public double latitude;
    public double longitude;
    public double height;
    
    // Performance requirements
    public bool requiresHighPerformance = false;
    public bool requiresAR = false;
    public bool isResourceIntensive = false;
    
    // Content metadata
    public List<string> tags = new List<string>();
    public long estimatedDownloadSize = 0; // in bytes
    
    // Experience requirements
    public int minDeviceMemoryMB = 0; // Minimum required device memory in MB
    public bool requiresInternet = false;
    public float estimatedBatteryDrain = 0f; // Estimated battery drain per minute
    
    // Statistics
    public int timesCompleted = 0;
    public float averageCompletionTime = 0f;
    public DateTime lastAccessed = DateTime.MinValue;
    
    // Add any additional properties needed for your specific use case
    // ...
}