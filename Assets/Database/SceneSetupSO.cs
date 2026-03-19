using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Lenx/SceneSetup")]
public class SceneSetupSO : ScriptableObject
{
    [System.Serializable]
    public class PrefabSetup
    {
        public AssetReference prefabReference; // Addressable prefab
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public PrefabSetup[] prefabsToSpawn;

    public bool enableImageTracking = true;
    public bool enablePlaneTracking = false;
    public bool enableFaceTracking = false;
    // Add more AR subsystems as needed
}