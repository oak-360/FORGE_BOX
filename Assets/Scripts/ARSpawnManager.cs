using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.AddressableAssets;
using System.Collections;

/// Manages AR scene setup and object instantiation based on SceneSetupSO configuration.
/// 
/// This component coordinates the activation of AR subsystems (image tracking, plane detection, etc.)
/// and handles the asynchronous loading and instantiation of addressable prefabs. It serves as
/// the central manager for configuring AR experiences at runtime.
/// 
/// Dependencies:
/// - AR Foundation components (ARTrackedImageManager, ARPlaneManager, ARFaceManager)
/// - Addressables system for prefab loading
/// - SceneSetupSO for configuration
/// 
/// Usage:
/// 1. Assign required AR manager references in the inspector
/// 2. Call SetupScene() with a SceneSetupSO to configure the AR environment
/// 3. The manager will handle enabling/disabling AR features and spawning prefabs
/// 
/// Note: All prefabs must be set up as Addressables to be properly loaded.
/// 
/// By Ofori Ankah Kofi

public class ARSpawnManager : MonoBehaviour
{
    [Header("AR Managers (assign XR Origin components)")]
    public ARTrackedImageManager imageManager;
    public ARPlaneManager planeManager;
    public ARFaceManager faceManager;
    // Add more AR managers as needed

    public void SetupScene(SceneSetupSO setup)
    {
        // Enable/disable AR subsystem components based on SceneSetupSO
        if (imageManager != null) imageManager.enabled = setup.enableImageTracking;
        if (planeManager != null) planeManager.enabled = setup.enablePlaneTracking;
        if (faceManager != null) faceManager.enabled = setup.enableFaceTracking;
        // Add more as needed

        // Spawn addressable prefabs
        foreach (var prefabSetup in setup.prefabsToSpawn)
        {
            StartCoroutine(SpawnPrefab(prefabSetup));
        }
    }

    private IEnumerator SpawnPrefab(SceneSetupSO.PrefabSetup setup)
    {
        var handle = setup.prefabReference.LoadAssetAsync<GameObject>();
        yield return handle;

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            GameObject obj = Instantiate(handle.Result, setup.position, setup.rotation);
            obj.transform.localScale = setup.scale;
        }
        else
        {
            Debug.LogError("Failed to load prefab: " + setup.prefabReference.RuntimeKey);
        }
    }
}