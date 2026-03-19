using System.Collections.Generic;
using UnityEngine;

public class ExperienceProcessor : MonoBehaviour
{
    [SerializeField] private UserProfileEventChannelSO onUserProfileLoaded;
    [SerializeField] private DatabaseLoadedEventChannelSO onDatabaseLoaded;
    [SerializeField] private ExperiencesProcessedEventChannelSO onExperiencesProcessed;
    
    private List<ExperienceData> allExperiences;
    private UserProfile userProfile;
    private DeviceData deviceData;

    private void OnEnable()
    {
        onUserProfileLoaded.OnRaised += HandleUserProfileLoaded;
        onDatabaseLoaded.OnRaised += HandleDatabaseLoaded;
    }

    private void OnDisable()
    {
        onUserProfileLoaded.OnRaised -= HandleUserProfileLoaded;
        onDatabaseLoaded.OnRaised -= HandleDatabaseLoaded;
    }

    private void HandleDatabaseLoaded()
    {
        if (allExperiences != null && userProfile != null)
        {
            FilterAndProcessExperiences();
        }
    }

    private void HandleUserProfileLoaded(UserProfile profile)
    {
        userProfile = profile;
        deviceData = DeviceDataCollector.CollectDeviceData();
        
        if (allExperiences != null)
        {
            FilterAndProcessExperiences();
        }
    }

    public void SetExperiences(List<ExperienceData> experiences)
    {
        allExperiences = experiences;
        
        if (userProfile != null)
        {
            FilterAndProcessExperiences();
        }
    }

    private void FilterAndProcessExperiences()
    {
        if (allExperiences == null || allExperiences.Count == 0)
        {
            Debug.LogWarning("[ExperienceProcessor] No experiences to process");
            return;
        }

        var filtered = ExperienceFilter.FilterExperiences(
            allExperiences, 
            userProfile, 
            deviceData);
            
        Debug.Log($"[ExperienceProcessor] Filtered to {filtered.Count} relevant experiences");
        onExperiencesProcessed.Raise(filtered);
    }
}