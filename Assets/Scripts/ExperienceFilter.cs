using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ExperienceFilter
{
    public static List<ExperienceData> FilterExperiences(
        List<ExperienceData> allExperiences, 
        UserProfile userProfile, 
        DeviceData deviceData)
    {
        return allExperiences
            .Where(exp => FilterByLocation(exp, userProfile, deviceData))
            .Where(exp => FilterByDeviceCapabilities(exp, deviceData))
            .Where(exp => FilterByUserPreferences(exp, userProfile))
            .Where(exp => FilterByNetworkConditions(exp, deviceData))
            .Where(exp => FilterByBatteryStatus(exp, deviceData))
            .OrderByDescending(exp => CalculateRelevanceScore(exp, userProfile, deviceData))
            .ToList();
    }
    
    private static bool FilterByLocation(ExperienceData exp, UserProfile userProfile, DeviceData deviceData)
    {
        if (!userProfile.allowLocationTracking || !deviceData.locationEnabled)
            return true; // If location is disabled, don't filter by location
            
        // If experience has no location data, don't filter it out
        if (exp.latitude == 0 && exp.longitude == 0)
            return true;
            
        // Calculate distance between user and experience
        float distance = CalculateDistance(
            (float)exp.latitude, 
            (float)exp.longitude,
            deviceData.currentLocation.Value.x,
            deviceData.currentLocation.Value.y);
            
        // Filter out experiences that are too far away
        return distance < 1000; // 1km radius
    }
    
    private static bool FilterByDeviceCapabilities(ExperienceData exp, DeviceData deviceData)
    {
        // Filter based on device capabilities
        if (exp.requiresHighPerformance && 
            (deviceData.systemMemorySize < 4000 || 
             deviceData.graphicsMemorySize < 2000))
        {
            return false;
        }
        
        // Filter based on AR support if needed
        if (exp.requiresAR && !deviceData.arSupported)
            return false;
            
        return true;
    }
    
    private static bool FilterByUserPreferences(ExperienceData exp, UserProfile userProfile)
    {
        // Filter out completed experiences if user prefers not to see them
        if (userProfile.completedExperiences != null && 
            userProfile.completedExperiences.Contains(exp.experienceName) &&
            userProfile.hideCompletedExperiences)
            return false;
            
        // Filter by user interests
        if (userProfile.interests != null && userProfile.interests.Count > 0 &&
            exp.tags != null && exp.tags.Count > 0)
        {
            bool matchesInterest = exp.tags.Any(tag => 
                userProfile.interests.Contains(tag));
                
            if (!matchesInterest)
                return false;
        }
        
        return true;
    }
    
    private static bool FilterByNetworkConditions(ExperienceData exp, DeviceData deviceData)
    {
        // Don't download large experiences on mobile data if user is concerned about data usage
        if (deviceData.networkType == "MobileData" && 
            exp.estimatedDownloadSize > 10 * 1024 * 1024) // 10MB
        {
            return false;
        }
        
        return true;
    }
    
    private static bool FilterByBatteryStatus(ExperienceData exp, DeviceData deviceData)
    {
        // Don't show resource-intensive experiences on low battery
        // Using BatteryLevel < 0.15f (15%) as a threshold for critical battery
        if ((deviceData.batteryStatus == UnityEngine.BatteryStatus.Discharging && 
             deviceData.batteryLevel < 0.15f) && 
            exp.isResourceIntensive)
        {
            return false;
        }
        
        return true;
    }
    
    private static float CalculateRelevanceScore(ExperienceData exp, UserProfile userProfile, DeviceData deviceData)
    {
        float score = 0;
        
        // Boost score based on user interests
        if (userProfile.interests != null && exp.tags != null)
        {
            score += exp.tags.Count(tag => 
                userProfile.interests.Contains(tag)) * 10;
        }
        
        // Boost score for nearby experiences
        if (deviceData.locationEnabled && deviceData.currentLocation.HasValue &&
            exp.latitude != 0 && exp.longitude != 0)
        {
            float distance = CalculateDistance(
                (float)exp.latitude, 
                (float)exp.longitude,
                deviceData.currentLocation.Value.x,
                deviceData.currentLocation.Value.y);
                
            // Closer experiences get higher scores
            score += 100f / Mathf.Max(0.1f, distance);
        }
        
        // Boost score for new experiences
        if (userProfile.completedExperiences == null || 
            !userProfile.completedExperiences.Contains(exp.experienceName))
        {
            score += 20;
        }
        
        return score;
    }
    
    // Helper method to calculate distance between two points using Haversine formula
    private static float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
    {
        const float R = 6371000f; // Earth's radius in meters
        
        float dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        float dLon = (lon2 - lon1) * Mathf.Deg2Rad;
        
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                 Mathf.Cos(lat1 * Mathf.Deg2Rad) * Mathf.Cos(lat2 * Mathf.Deg2Rad) *
                 Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
                 
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        
        return R * c; // Distance in meters
    }
}