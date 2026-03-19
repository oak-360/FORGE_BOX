using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserProfile
{
    // Basic user info
    public string userId;
    public string userName;
    public List<string> interests;
    public List<string> preferredCategories;
    
    // Device info
    public string deviceModel;
    public string operatingSystem;
    public string deviceLanguage;
    public string timeZone;
    public DeviceData deviceData;
    
    // Usage statistics
    public Dictionary<string, int> experienceInteractionCount;
    public List<string> completedExperiences;
    public float averageSessionDuration;
    
    // Preferences
    public bool allowLocationTracking = true;
    public bool allowAnalytics = true;
    public bool hideCompletedExperiences = false; 
    public QualityLevel preferredQuality = QualityLevel.Medium;
    
    public enum QualityLevel
    {
        Low,        // For older devices
        Medium,     // For most modern devices
        High        // For high-end devices
    }
}