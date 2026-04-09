using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// One-click scene setup for the Table Tennis game.
/// Run via:  Tools > Table Tennis > Setup Scene
/// </summary>
public static class SetupTableTennis
{
    [MenuItem("Tools/Table Tennis/Setup Scene")]
    public static void SetupScene()
    {
        // ── 1. GameManager ──────────────────────────────────────────────
        var mgr = GameObject.Find("GameManager");
        if (mgr == null)
        {
            mgr = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(mgr, "Create GameManager");
        }

        var ttm = mgr.GetComponent<TableTennisManager>();
        if (ttm == null)
            ttm = Undo.AddComponent<TableTennisManager>(mgr);

        // ── 2. Player bat ───────────────────────────────────────────────
        var playerBatObj = GameObject.Find("PingPongBat");
        if (playerBatObj != null)
        {
            if (playerBatObj.GetComponent<PlayerBatController>() == null)
                Undo.AddComponent<PlayerBatController>(playerBatObj);

            // Make sure the Rigidbody is there (it should be)
            var rb = playerBatObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Undo.RecordObject(rb, "Configure player bat RB");
                rb.isKinematic = true;
                rb.useGravity  = false;
            }
        }
        else
        {
            Debug.LogWarning("[SetupTableTennis] 'PingPongBat' not found in scene.");
        }

        // ── 3. AI bat ───────────────────────────────────────────────────
        var aiBatObj = GameObject.Find("PingPongBat (1)");
        if (aiBatObj != null)
        {
            if (aiBatObj.GetComponent<AIBatController>() == null)
                Undo.AddComponent<AIBatController>(aiBatObj);

            var rb = aiBatObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Undo.RecordObject(rb, "Configure AI bat RB");
                rb.isKinematic = true;
                rb.useGravity  = false;
            }
        }
        else
        {
            Debug.LogWarning("[SetupTableTennis] 'PingPongBat (1)' not found in scene.");
        }

        // ── 4. Ball – ensure gravity on, not kinematic ──────────────────
        var ballObj = GameObject.Find("Ball");
        if (ballObj != null)
        {
            var rb = ballObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Undo.RecordObject(rb, "Configure ball RB");
                rb.useGravity  = true;
                rb.isKinematic = false;
            }
        }
        else
        {
            Debug.LogWarning("[SetupTableTennis] 'Ball' not found in scene.");
        }

        // ── 5. Directional Light – turn on for visibility ───────────────
        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            Undo.RecordObject(dirLight, "Enable Directional Light");
            dirLight.SetActive(true);
        }

        // ── 6. Save scene ───────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[SetupTableTennis] Scene setup complete! " +
                  "GameManager + TableTennisManager created, " +
                  "PlayerBatController on PingPongBat, " +
                  "AIBatController on PingPongBat (1). " +
                  "Press Play to start the game.");

        EditorUtility.DisplayDialog(
            "Table Tennis Setup",
            "Scene configured successfully!\n\n" +
            "• GameManager  →  TableTennisManager\n" +
            "• PingPongBat  →  PlayerBatController\n" +
            "• PingPongBat (1)  →  AIBatController\n\n" +
            "Press Play to start.\n\n" +
            "Controls:\n" +
            "  Move mouse  →  move racket\n" +
            "  Left click  →  serve\n" +
            "  Drag mouse UP + release  →  hit ball",
            "OK");
    }

    [MenuItem("Tools/Table Tennis/Setup Scene", true)]
    static bool ValidateSetup()
    {
        // Only available when a scene is open
        return !string.IsNullOrEmpty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
