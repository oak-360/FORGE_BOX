using UnityEngine;
using System.Collections.Generic;
using System;

public class UserProfileManager : MonoBehaviour
{
    [SerializeField] private UserProfileEventChannelSO onUserProfileLoaded;
    [SerializeField] private ExperienceInteractionEventChannelSO onExperienceInteracted;
    
    private UserProfile userProfile;
    private DeviceData deviceData;

    private void Start()
    {
        LoadUserProfile();
    }

    private void OnEnable()
    {
        onExperienceInteracted.OnRaised += HandleExperienceInteraction;
    }

    private void OnDisable()
    {
        onExperienceInteracted.OnRaised -= HandleExperienceInteraction;
    }

    private void LoadUserProfile()
    {
        // Load from persistent storage or create new
        userProfile = new UserProfile
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
        
        deviceData = DeviceDataCollector.CollectDeviceData();
        onUserProfileLoaded.Raise(userProfile);
    }

    private void HandleExperienceInteraction(ExperienceData experience)
    {
        if (!userProfile.experienceInteractionCount.ContainsKey(experience.experienceName))
        {
            userProfile.experienceInteractionCount[experience.experienceName] = 0;
        }
        userProfile.experienceInteractionCount[experience.experienceName]++;
    }

    public UserProfile GetUserProfile() => userProfile;
    public DeviceData GetDeviceData() => deviceData;
}