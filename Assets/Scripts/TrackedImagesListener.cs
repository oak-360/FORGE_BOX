using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Linq;


/// Listens for AR image detection events and maps detected images to corresponding experiences.
/// 
/// This component works with ARTrackedImageManager to detect when reference images are recognized
/// in the camera view. It matches detected images to ExperienceData entries using their GUIDs
/// and raises events when experiences are detected.
/// 
/// Dependencies:
/// - ARTrackedImageManager: For receiving image tracking events
/// - ExperienceDatabaseSO: Contains the collection of experiences to match against
/// 
/// Events:
/// - OnExperienceDetected: Raised when a tracked image matches a known experience
/// 
/// Note: This component expects the reference image's name to match the imageReferenceGuid
/// in the ExperienceData for proper matching.
/// By Ofori Ankah Kofi

public class TrackedImagesListener : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public ExperienceDatabaseSO experienceDatabase;

    public static event Action<ExperienceData> OnExperienceDetected;

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        Debug.Log($"[TrackedImagesListener] OnTrackedImagesChanged - Added: {args.added?.Count ?? 0}, Updated: {args.updated?.Count ?? 0}, Removed: {args.removed?.Count ?? 0}");

        foreach (var trackedImage in args.added)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"[TrackedImagesListener] Added image - Name: {imageName}, Tracking State: {trackedImage.trackingState}, Size: {trackedImage.size}");
            
            if (experienceDatabase == null)
            {
                Debug.LogError("[TrackedImagesListener] Experience database is null!");
                return;
            }

            if (experienceDatabase.experiences == null)
            {
                Debug.LogError("[TrackedImagesListener] Experiences list is null!");
                return;
            }

            
            Debug.Log($"[TrackedImagesListener] Searching for experience with GUID: {imageName}");
            
            // Log all available GUIDs for debugging
            foreach (var exp in experienceDatabase.experiences)
            {
                Debug.Log($"[TrackedImagesListener] Available - {exp.experienceName}: {exp.imageReferenceGuid}");
            }

            var experience = experienceDatabase.experiences
                .FirstOrDefault(exp => exp.imageReferenceGuid == imageName);

            if (experience != null)
            {
                Debug.Log($"[TrackedImagesListener] Matched experience: {experience.experienceName} (GUID: {experience.imageReferenceGuid})");
                OnExperienceDetected?.Invoke(experience);
            }
            else
            {
                Debug.LogWarning($"[TrackedImagesListener] No ExperienceData found for image with name: {imageName}");
            }
        }
    }
}