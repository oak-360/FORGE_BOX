using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using System.Collections;

//Quickly load the Lenx scene (via Addressables) from the PersistentManager scene,
//Pass a SceneSetupSO (or null) to the Lenx scene (for initial setup, if needed),
//Allow the database to update in the background,
//Enable AR image tracking immediately,
//And let the system dynamically check for and add new images as the database updates.

public class ExperienceLoader : MonoBehaviour
{
    [SerializeField] private string lenxSceneAddress = "Lenx";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Call this from UI or game flow
    public void OpenLenxScene(SceneSetupSO setup)
    {
        StartCoroutine(LoadLenxScene(setup));
    }

    private IEnumerator LoadLenxScene(SceneSetupSO setup)
    {
        var handle = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(lenxSceneAddress, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        yield return handle;

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Lenx scene loaded.");
            if (setup != null)
            {
                var managers = GameObject.FindObjectsOfType<ARSpawnManager>();
                foreach (var manager in managers)
                {
                    manager.SetupScene(setup);
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load Lenx scene.");
        }
    }
}