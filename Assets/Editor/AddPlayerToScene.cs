#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class AddPlayerToScene : EditorWindow
{
    [MenuItem("Tools/Add Player to Current Scene")]
    public static void AddPlayer()
    {
        if (FindAnyObjectByType<LevelTestBootstrap>() != null)
        {
            Debug.LogWarning("LevelTestBootstrap already exists in the scene.");
            return;
        }

        GameObject bootstrapObj = new GameObject("LevelTestBootstrap");
        LevelTestBootstrap bootstrap = bootstrapObj.AddComponent<LevelTestBootstrap>();

        string[] guids = AssetDatabase.FindAssets("t:InputActionAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            bootstrap.inputActions = asset;
        }
        else
        {
            Debug.LogWarning("Could not automatically find InputActionAsset. Please assign it manually on the LevelTestBootstrap component.");
        }

        Selection.activeGameObject = bootstrapObj;
        Debug.Log("Added LevelTestBootstrap. Press Play to spawn the player at (0,0)!");
    }
}
#endif
