using UnityEngine;
using UnityEngine.XR.ARFoundation;

[CreateAssetMenu(menuName="Events/ARManagerRegistered")]
public class ARManagerRegisteredEventChannelSO : ScriptableObject
{
    public event System.Action<ARTrackedImageManager> OnRaised;
    public void Raise(ARTrackedImageManager mgr) => OnRaised?.Invoke(mgr);
}