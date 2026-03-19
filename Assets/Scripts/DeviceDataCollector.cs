using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class DeviceDataCollector
{
    public static DeviceData CollectDeviceData()
    {
        return new DeviceData
        {
            // Device capabilities
            processorFrequency = SystemInfo.processorFrequency,
            processorCount = SystemInfo.processorCount,
            systemMemorySize = SystemInfo.systemMemorySize,
            graphicsMemorySize = SystemInfo.graphicsMemorySize,
            graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
            
            // Location data (if permission granted)
            locationEnabled = Input.location.isEnabledByUser,
            currentLocation = GetCurrentLocation(),
            
            // Network status
            networkReachability = Application.internetReachability,
            networkType = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork ? 
                "WiFi" : "MobileData",
            
            // Battery status
            batteryLevel = SystemInfo.batteryLevel,
            batteryStatus = SystemInfo.batteryStatus,
            
            // Time and locale
            currentTime = DateTime.Now,
            timeZone = TimeZoneInfo.Local,
            deviceLanguage = Application.systemLanguage.ToString(),
            
            // AR capabilities
            arSupported = CheckARSupport(),
            
            // Storage
            storageFreeSpace = GetFreeStorageSpace()
        };
    }
    
    // Helper methods...
    private static Vector2? GetCurrentLocation()
    {
        // Implementation to get current location
        // This is a placeholder - actual implementation would use Unity's LocationService
        if (Input.location.status == LocationServiceStatus.Running)
        {
            LocationInfo location = Input.location.lastData;
            return new Vector2(location.latitude, location.longitude);
        }
        return null;
    }
    
    private static bool CheckARSupport()
    {
        // Check if AR is supported on the current device
        // This is a more compatible way to check for AR support
        #if UNITY_EDITOR
        // In editor, we'll assume AR is supported for testing
        return true;
        #else
        // At runtime, check if any AR session subsystem is available
        var descriptors = new List<UnityEngine.XR.ARSubsystems.XRSessionSubsystemDescriptor>();
        UnityEngine.SubsystemManager.GetSubsystemDescriptors(descriptors);
        return descriptors.Count > 0;
        #endif
    }
    
    private static long GetFreeStorageSpace()
    {
        // Implementation to get free storage space
        // This is a simplified example - actual implementation would use platform-specific code
        return System.IO.DriveInfo.GetDrives()[0].AvailableFreeSpace;
    }
}

[System.Serializable]
public class DeviceData
{
    // Device capabilities
    public int processorFrequency;
    public int processorCount;
    public int systemMemorySize;
    public int graphicsMemorySize;
    public string graphicsDeviceVersion;
    
    // Location
    public bool locationEnabled;
    public UnityEngine.Vector2? currentLocation; // Latitude, Longitude
    
    // Network
    public UnityEngine.NetworkReachability networkReachability;
    public string networkType;
    
    // Battery
    public float batteryLevel;
    public UnityEngine.BatteryStatus batteryStatus;
    
    // Time and locale
    public DateTime currentTime;
    public TimeZoneInfo timeZone;
    public string deviceLanguage;
    
    // AR capabilities
    public bool arSupported;
    
    // Storage
    public long storageFreeSpace; // in bytes
}