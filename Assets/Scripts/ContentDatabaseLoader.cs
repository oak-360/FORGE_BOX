using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class ContentDatabaseLoader : MonoBehaviour
{
    [SerializeField] private string experienceDatabaseAddress; // Set this to the SO's addressable key

    public Dictionary<string, ExperienceData> experienceDict { get; private set; } = new Dictionary<string, ExperienceData>();
    public List<ExperienceData> experienceList { get; private set; } = new List<ExperienceData>();

    public DatabaseLoadedEventChannelSO databaseLoadedEventChannel; // Assign in Inspector
    
    // Flag to track if database is already loaded
    public bool IsDatabaseLoaded { get; private set; } = false;

    void Awake()
    {
    DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Debug.Log("[ContentDatabaseLoader] Starting database load");
        LoadExperienceDatabase();
    }

    public void LoadExperienceDatabase()
    {
        Debug.Log("[ContentDatabaseLoader] Requesting database from Addressables: " + experienceDatabaseAddress);
        Addressables.LoadAssetAsync<ExperienceDatabaseSO>(experienceDatabaseAddress).Completed += OnDatabaseLoadedHandler;
    }
    
    // Public method that allows other scripts to manually request the database
    // Returns true if database was already loaded and the callback was invoked immediately
    public bool RequestDatabase(System.Action callback)
    {
        if (callback == null) return false;
        
        Debug.Log("[ContentDatabaseLoader] Database manually requested by another script");
        
        if (IsDatabaseLoaded)
        {
            Debug.Log("[ContentDatabaseLoader] Database already loaded, invoking callback immediately");
            callback.Invoke();
            return true;
        }
        else
        {
            Debug.Log("[ContentDatabaseLoader] Database not yet loaded, adding callback to OnDatabaseLoaded");
            databaseLoadedEventChannel.OnRaised += callback;
            return false;
        }
    }

    private void OnDatabaseLoadedHandler(AsyncOperationHandle<ExperienceDatabaseSO> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
          {
        var db = handle.Result;
        experienceList.Clear();
        experienceDict.Clear();
        foreach (var exp in db.experiences)
        {
            experienceList.Add(exp);
            experienceDict[exp.experienceName] = exp;
        }
        // Log all loaded experiences
        Debug.Log($"Database loaded with {experienceList.Count} experiences:");
        foreach (var exp in experienceList)
        {
            Debug.Log($"Experience: {exp.experienceName}, Scene Setup: {exp.sceneSetup}, Details: {exp.details}");
        }
        Debug.Log("[ContentDatabaseLoader] Database processing complete, raising SO event.");
        IsDatabaseLoaded = true;
        if (databaseLoadedEventChannel != null)
        {
            databaseLoadedEventChannel.Raise();
        }
        }
        else
        {
            Debug.LogError("Failed to load Experience Database from CCD.");
        }
    }
}