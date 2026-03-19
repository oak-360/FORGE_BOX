// SceneLoader.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
  public SceneLoadedEventChannelSO OnSceneLoaded;
  [SerializeField] private string lenxAddress = "Lenx.unity";

  public void LoadLenx()
  {
    Addressables.LoadSceneAsync(lenxAddress, LoadSceneMode.Additive)
      .Completed += handle =>
      {
        if (handle.Status == AsyncOperationStatus.Succeeded)
          OnSceneLoaded.Raise();
        else
          Debug.LogError("Failed to load Lenx");
      };
  }
}