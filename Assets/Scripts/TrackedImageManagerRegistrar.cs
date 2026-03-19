using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TrackedImageManagerRegistrar : MonoBehaviour
{
    public ARManagerRegisteredEventChannelSO OnRegistered;

    void Start()
    {
        var mgr = GetComponent<ARTrackedImageManager>();
        if (mgr != null)
        {
            Debug.Log("[TrackedImageManagerRegistrar] Found ARTrackedImageManager, raising event");
            OnRegistered.Raise(mgr);
        }
        else
        {
            Debug.LogError("[TrackedImageManagerRegistrar] No ARTrackedImageManager found!");
        }
    }
}