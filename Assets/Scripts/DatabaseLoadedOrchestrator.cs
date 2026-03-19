using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DatabaseLoadedOrchestrator : MonoBehaviour
{
    public DatabaseLoadedEventChannelSO databaseLoadedEventChannel;
    public SceneLoader sceneLoader; // Assign in Inspector

    void OnEnable()
    {
        databaseLoadedEventChannel.OnRaised += HandleDatabaseLoaded;
    }
    void OnDisable()
    {
        databaseLoadedEventChannel.OnRaised -= HandleDatabaseLoaded;
    }

    private void HandleDatabaseLoaded()
    {
        Debug.Log("[Orchestrator] Database loaded, loading AR scene...");
        sceneLoader.LoadLenx();
    }
}