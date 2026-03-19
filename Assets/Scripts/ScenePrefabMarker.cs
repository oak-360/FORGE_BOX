using UnityEngine;
using UnityEngine.AddressableAssets;

//Attach this to any prefab in the scene you want to be part of the experience:

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class ScenePrefabMarker : MonoBehaviour
{
    public AssetReference prefabReference; // Assign the Addressable prefab reference here
}