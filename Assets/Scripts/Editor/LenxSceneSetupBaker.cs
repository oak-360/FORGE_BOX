using UnityEngine;
using UnityEditor;

//Custom Editor Script: Bake Scene to SceneSetupSO
//This script finds all ScenePrefabMarker objects in the scene, reads
// their prefab reference and transform, and writes them to the selected SceneSetupSO.


public class LenxSceneSetupBaker : EditorWindow
{
    public SceneSetupSO targetSetupSO;

    [MenuItem("Lenx/Scene Setup Baker")]
    public static void ShowWindow()
    {
        GetWindow<LenxSceneSetupBaker>("Lenx Scene Setup Baker");
    }

    void OnGUI()
    {
        targetSetupSO = (SceneSetupSO)EditorGUILayout.ObjectField("Scene Setup SO", targetSetupSO, typeof(SceneSetupSO), false);

        if (GUILayout.Button("Bake Scene Prefabs to SO") && targetSetupSO != null)
        {
            BakeSceneToSO(targetSetupSO);
        }
    }

    void BakeSceneToSO(SceneSetupSO so)
    {
        var markers = GameObject.FindObjectsOfType<ScenePrefabMarker>();
        var prefabSetups = new SceneSetupSO.PrefabSetup[markers.Length];

        for (int i = 0; i < markers.Length; i++)
        {
            var marker = markers[i];
            prefabSetups[i] = new SceneSetupSO.PrefabSetup
            {
                prefabReference = marker.prefabReference,
                position = marker.transform.position,
                rotation = marker.transform.rotation,
                scale = marker.transform.localScale
            };
        }

        so.prefabsToSpawn = prefabSetups;
        EditorUtility.SetDirty(so);
        AssetDatabase.SaveAssets();
        Debug.Log($"Baked {markers.Length} prefabs to {so.name}!");
    }
}