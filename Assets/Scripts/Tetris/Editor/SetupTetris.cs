using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility – opens the Tetris scene and wires up the TetrisGame
/// component.  Run via  Tools > Tetris > Setup Scene.
/// </summary>
public static class SetupTetris
{
    [MenuItem("Tools/Tetris/Setup Scene")]
    static void Setup()
    {
        // Open the Tetris scene if it isn't already active
        const string SCENE_PATH = "Assets/Scenes/Tetris.unity";
        var scene = SceneManager.GetActiveScene();
        if (scene.path != SCENE_PATH)
        {
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(SCENE_PATH);
        }

        // Remove any existing TetrisGame objects
        foreach (var existing in Object.FindObjectsByType<TetrisGame>(FindObjectsSortMode.None))
            Object.DestroyImmediate(existing.gameObject);

        // Create the controller GameObject
        var go = new GameObject("TetrisGame");
        go.AddComponent<TetrisGame>();
        Undo.RegisterCreatedObjectUndo(go, "Create TetrisGame");

        // Make sure the camera is set to 2D-friendly settings
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.07f);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[Tetris Setup] Done. Press Play to start the game.");
        Selection.activeGameObject = go;
    }
}
