using UnityEngine;

/// <summary>
/// Listens for experience detection events and configures the AR environment accordingly.
/// 
/// This component connects TrackedImagesListener with ARSpawnManager to automatically
/// set up the AR environment when an experience is detected.
/// 
/// Dependencies:
/// - TrackedImagesListener: For receiving experience detection events
/// - ARSpawnManager: For spawning and managing AR content
/// 
/// By Ofori Ankah Kofi
/// </summary>
public class ExperienceSceneManager : MonoBehaviour
{
    [Tooltip("Reference to the TrackedImagesListener component")]
    [SerializeField] private TrackedImagesListener trackedImagesListener;

    [Tooltip("Reference to the AR Spawn Manager component")]
    [SerializeField] private ARSpawnManager spawnManager;

    private void OnEnable()
    {
        if (trackedImagesListener != null)
        {
            TrackedImagesListener.OnExperienceDetected += HandleExperienceDetected;
        }
        else
        {
            Debug.LogError($"{nameof(ExperienceSceneManager)}: TrackedImagesListener reference is missing!");
        }
    }

    private void OnDisable()
    {
        if (trackedImagesListener != null)
        {
            TrackedImagesListener.OnExperienceDetected -= HandleExperienceDetected;
        }
    }

    private void HandleExperienceDetected(ExperienceData experienceData)
    {
        if (spawnManager == null)
        {
            Debug.LogError($"{nameof(ExperienceSceneManager)}: ARSpawnManager reference is missing!");
            return;
        }

        if (experienceData?.sceneSetup != null)
        {
            Debug.Log($"[ExperienceSceneManager] Setting up scene for experience: {experienceData.experienceName}");
            spawnManager.SetupScene(experienceData.sceneSetup);
        }
        else
        {
            Debug.LogWarning($"[ExperienceSceneManager] No scene setup found for experience: {experienceData?.experienceName ?? "Unknown"}");
        }
    }

    private void Reset()
    {
        // Try to auto-assign references in the editor
        if (trackedImagesListener == null)
        {
            trackedImagesListener = FindObjectOfType<TrackedImagesListener>();
        }

        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<ARSpawnManager>();
        }
    }
}