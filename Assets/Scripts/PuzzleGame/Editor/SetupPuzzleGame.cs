using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame;

public static class SetupPuzzleGame
{
    // ── Colors ──────────────────────────────────────────────────────────────
    static readonly Color CYellow = new Color(1.00f, 0.85f, 0.15f);
    static readonly Color CGreen  = new Color(0.20f, 0.75f, 0.28f);
    static readonly Color CRed    = new Color(0.90f, 0.22f, 0.28f);
    static readonly Color CCyan   = new Color(0.15f, 0.78f, 0.88f);
    static readonly Color CPurple = new Color(0.60f, 0.20f, 0.88f);
    static readonly Color CGray   = new Color(0.35f, 0.35f, 0.35f);
    static readonly Color CBg     = new Color(0.06f, 0.14f, 0.07f);

    const string LevelFolder   = "Assets/Prefabs/PuzzleGame/Levels";
    const string PrefabFolder  = "Assets/Prefabs/PuzzleGame";

    [MenuItem("Tools/Puzzle Game/Setup Scene")]
    public static void Setup()
    {
        EnsureFolders();
        var levels = CreateAllLevels();
        BuildScene(levels);
        AssetDatabase.SaveAssets();
        EditorApplication.ExecuteMenuItem("File/Save");
        Debug.Log("[PuzzleGame] Scene setup complete.");
    }

    // ── Folder helpers ───────────────────────────────────────────────────────
    static void EnsureFolders()
    {
        CreateFolder("Assets/Prefabs");
        CreateFolder("Assets/Prefabs/PuzzleGame");
        CreateFolder("Assets/Prefabs/PuzzleGame/Levels");
    }

    static void CreateFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts = path.Split('/');
            var parent = string.Join("/", parts, 0, parts.Length - 1);
            AssetDatabase.CreateFolder(parent, parts[parts.Length - 1]);
        }
    }

    // ── Level Data ───────────────────────────────────────────────────────────
    static LevelData[] CreateAllLevels()
    {
        var defs = BuildLevelDefinitions();
        var assets = new LevelData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
            assets[i] = SaveLevel(defs[i], i + 1);
        return assets;
    }

    static LevelData SaveLevel(LevelData data, int num)
    {
        string path = $"{LevelFolder}/Level{num:00}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(data, existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }
        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    // Row helper: all cols in a row
    static int[] Row(int r, int cols) { var a = new int[cols]; for (int c = 0; c < cols; c++) a[c] = r * cols + c; return a; }
    // Col helper: all rows in a col
    static int[] Col(int c, int cols, int rows) { var a = new int[rows]; for (int r = 0; r < rows; r++) a[r] = r * cols + c; return a; }
    // Merge helper
    static int[] Merge(params int[][] groups)
    {
        var list = new System.Collections.Generic.List<int>();
        foreach (var g in groups) list.AddRange(g);
        return list.ToArray();
    }
    static int[] Indices(params int[] idx) => idx;

    // ── All 10 Levels ────────────────────────────────────────────────────────
    static LevelData[] BuildLevelDefinitions()
    {
        // 4 rows x 5 cols = 20 tiles unless noted
        // Flat index: row*cols + col

        // ─── Level 1: Row buttons, 3 colors, trivial ────────────────────────
        var l1 = ScriptableObject.CreateInstance<LevelData>();
        l1.rows = 3; l1.cols = 5;
        l1.targetColors = Repeat(CYellow,5, CGreen,5, CRed,5);
        l1.buttons = new[]
        {
            Btn(CYellow, Row(0,5)),
            Btn(CGreen,  Row(1,5)),
            Btn(CRed,    Row(2,5)),
        };

        // ─── Level 2: Row buttons, pick the right ones ───────────────────────
        var l2 = ScriptableObject.CreateInstance<LevelData>();
        l2.rows = 3; l2.cols = 5;
        l2.targetColors = Repeat(CCyan,5, CYellow,5, CGreen,5);
        l2.buttons = new[]
        {
            Btn(CYellow, Row(1,5)),
            Btn(CGreen,  Row(2,5)),
            Btn(CCyan,   Row(0,5)),
            Btn(CRed,    Row(2,5)),   // distractor
        };

        // ─── Level 3: 4 rows, extra buttons (order matters within row) ────────
        var l3 = ScriptableObject.CreateInstance<LevelData>();
        l3.rows = 4; l3.cols = 5;
        l3.targetColors = Repeat(CYellow,5, CGreen,5, CCyan,5, CRed,5);
        l3.buttons = new[]
        {
            Btn(CYellow, Row(0,5)),
            Btn(CGreen,  Row(1,5)),
            Btn(CCyan,   Row(2,5)),
            Btn(CRed,    Row(3,5)),
            Btn(CPurple, Row(0,5)),   // distractor – overwrites row0 if pressed
        };

        // ─── Level 4: Ordering matters – one button covers 2 rows ────────────
        // Target: Green / Cyan / Yellow / Red
        var l4 = ScriptableObject.CreateInstance<LevelData>();
        l4.rows = 4; l4.cols = 5;
        l4.targetColors = Repeat(CGreen,5, CCyan,5, CYellow,5, CRed,5);
        l4.buttons = new[]
        {
            Btn(CYellow, Merge(Row(1,5), Row(2,5))), // fills rows 1+2 Yellow
            Btn(CGreen,  Row(0,5)),
            Btn(CCyan,   Row(1,5)),                  // overwrites row1 → must come after B0
            Btn(CRed,    Row(3,5)),
        };
        // Solution: B0→B2→B1→B3

        // ─── Level 5: Column buttons ─────────────────────────────────────────
        // Target: alternating cols Y G Y G Y  (all rows same)
        var l5 = ScriptableObject.CreateInstance<LevelData>();
        l5.rows = 4; l5.cols = 5;
        var t5 = new Color[20];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 5; c++)
                t5[r*5+c] = c % 2 == 0 ? CYellow : CGreen;
        l5.targetColors = t5;
        l5.buttons = new[]
        {
            Btn(CYellow, Merge(Col(0,5,4), Col(2,5,4), Col(4,5,4))),
            Btn(CGreen,  Merge(Col(1,5,4), Col(3,5,4))),
            Btn(CRed,    Merge(Col(0,5,4), Col(1,5,4))),  // distractor
        };

        // ─── Level 6: Mix rows + columns ────────────────────────────────────
        // Target: Row0+Row2=Red; Row1+Row3 alternating Y G Y G Y
        var l6 = ScriptableObject.CreateInstance<LevelData>();
        l6.rows = 4; l6.cols = 5;
        var t6 = new Color[20];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 5; c++)
                t6[r*5+c] = r % 2 == 0 ? CRed : (c % 2 == 0 ? CYellow : CGreen);
        l6.targetColors = t6;
        l6.buttons = new[]
        {
            Btn(CRed,    Merge(Row(0,5), Row(2,5))),
            Btn(CYellow, Indices(5,7,9,15,17,19)),      // col 0,2,4 of rows 1,3
            Btn(CGreen,  Indices(6,8,16,18)),            // col 1,3 of rows 1,3
            Btn(CCyan,   Row(1,5)),                      // distractor
        };

        // ─── Level 7: Partial patterns / shapes ──────────────────────────────
        // Target: checkerboard – even cells Yellow, odd cells Gray
        var l7 = ScriptableObject.CreateInstance<LevelData>();
        l7.rows = 4; l7.cols = 5;
        var t7 = new Color[20];
        for (int i = 0; i < 20; i++) t7[i] = i % 2 == 0 ? CYellow : CGray;
        l7.targetColors = t7;
        l7.buttons = new[]
        {
            Btn(CYellow, Indices(0,2,4,6,8,10,12,14,16,18)),   // even indices
            Btn(CGreen,  Row(0,5)),                              // distractor – overwrites row0
            Btn(CGray,   Indices(1,3,5,7,9,11,13,15,17,19)),   // odd → gray (reset)
        };
        // Solution: B0 (even=Y), then if player pressed B1 accidentally use B0+B2 to fix

        // ─── Level 8: Layered partial patterns ──────────────────────────────
        // Target: top-half Left=Y Right=G | bottom-half Left=R Right=C
        // Y Y G G Y / Y Y G G Y / R R C C R / R R C C R
        var l8 = ScriptableObject.CreateInstance<LevelData>();
        l8.rows = 4; l8.cols = 5;
        Color[] t8 = new Color[20];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 5; c++)
            {
                bool top  = r < 2;
                bool left = c < 2 || c == 4;
                t8[r*5+c] = top ? (left ? CYellow : CGreen) : (left ? CRed : CCyan);
            }
        l8.targetColors = t8;
        l8.buttons = new[]
        {
            Btn(CYellow, Indices(0,1,4,5,6,9)),           // top rows left+col4
            Btn(CGreen,  Indices(2,3,7,8)),                // top rows mid
            Btn(CRed,    Indices(10,11,14,15,16,19)),      // bottom rows left+col4
            Btn(CCyan,   Indices(12,13,17,18)),            // bottom rows mid
        };

        // ─── Level 9: Overlapping – order is critical ────────────────────────
        // Target: border=Cyan, inner ring=Yellow, center=Red
        // C C C C C / C Y Y Y C / C Y R Y C / C C C C C
        var l9 = ScriptableObject.CreateInstance<LevelData>();
        l9.rows = 4; l9.cols = 5;
        Color[] t9 = {
            CCyan,  CCyan,  CCyan,  CCyan,  CCyan,
            CCyan,  CYellow,CYellow,CYellow,CCyan,
            CCyan,  CYellow,CRed,   CYellow,CCyan,
            CCyan,  CCyan,  CCyan,  CCyan,  CCyan,
        };
        l9.targetColors = t9;
        l9.buttons = new[]
        {
            Btn(CCyan,   Merge(Row(0,5), Row(1,5), Row(2,5), Row(3,5))), // fill all
            Btn(CYellow, Indices(6,7,8,11,12,13)),                       // inner 2x3
            Btn(CRed,    Indices(12)),                                    // center
        };
        // Solution: B0 → B1 → B2

        // ─── Level 10: All mechanics, complex ordering ───────────────────────
        // Target: Y G Y G Y / R C R C R / G Y G Y G / C R C R C
        var l10 = ScriptableObject.CreateInstance<LevelData>();
        l10.rows = 4; l10.cols = 5;
        Color[] t10 = {
            CYellow,CGreen, CYellow,CGreen, CYellow,
            CRed,   CCyan,  CRed,   CCyan,  CRed,
            CGreen, CYellow,CGreen, CYellow,CGreen,
            CCyan,  CRed,   CCyan,  CRed,   CCyan,
        };
        l10.targetColors = t10;
        // Buttons designed so solution is: B1 → B0 → B2 → B3
        l10.buttons = new[]
        {
            // B0 (Yellow): col0,2,4 row0 + col1,3 row2
            Btn(CYellow, Indices(0,2,4, 11,13)),
            // B1 (Green):  col1,3 row0 + all row2
            Btn(CGreen,  Indices(1,3, 10,11,12,13,14)),
            // B2 (Red):    all row1 + col1,3 row3
            Btn(CRed,    Indices(5,6,7,8,9, 16,18)),
            // B3 (Cyan):   col1,3 row1 + all row3
            Btn(CCyan,   Indices(6,8, 15,16,17,18,19)),
            // Distractor
            Btn(CPurple, Row(0,5)),
        };
        // Solution: B1(Green fills col1,3 row0 + all row2) →
        //           B0(Yellow fixes col0,2,4 row0 + col1,3 row2) →
        //           B2(Red fills row1 + col1,3 row3) →
        //           B3(Cyan fixes col1,3 row1 + row3)

        return new[] { l1, l2, l3, l4, l5, l6, l7, l8, l9, l10 };
    }

    static ButtonDef Btn(Color c, int[] tiles) => new ButtonDef { color = c, tileIndices = tiles };

    static Color[] Repeat(params object[] args)
    {
        var list = new System.Collections.Generic.List<Color>();
        for (int i = 0; i < args.Length; i += 2)
        {
            var col = (Color)args[i];
            int count = (int)args[i + 1];
            for (int j = 0; j < count; j++) list.Add(col);
        }
        return list.ToArray();
    }

    // ── Scene Build ──────────────────────────────────────────────────────────
    static void BuildScene(LevelData[] levels)
    {
        // Clear existing puzzle objects
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        foreach (var go in roots)
            if (go.name != "Main Camera" && go.name != "Directional Light")
                Object.DestroyImmediate(go);

        // ── Camera ──────────────────────────────────────────────────────────
        var camGO = GameObject.FindObjectOfType<Camera>()?.gameObject
                    ?? new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.GetOrAddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = CBg;
        cam.orthographic = true;

        // ── Canvas ──────────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(390, 844);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Background ──────────────────────────────────────────────────────
        var bg = MakeImage(canvasGO.transform, "Background", CBg);
        StretchFill(bg.GetComponent<RectTransform>());

        // ── Root layout: vertical group ──────────────────────────────────────
        var root = MakePanel(canvasGO.transform, "Root");
        StretchFill(root.GetComponent<RectTransform>());
        var rootVG = root.AddComponent<VerticalLayoutGroup>();
        rootVG.childAlignment = TextAnchor.UpperCenter;
        rootVG.childForceExpandWidth  = true;
        rootVG.childForceExpandHeight = false;
        rootVG.childControlWidth  = true;
        rootVG.childControlHeight = false;
        rootVG.spacing = 16;
        rootVG.padding = new RectOffset(20, 20, 30, 20);

        // ── Header ──────────────────────────────────────────────────────────
        var header = MakePanel(root.transform, "Header");
        SetHeight(header, 60);
        var headerHG = header.AddComponent<HorizontalLayoutGroup>();
        headerHG.childAlignment = TextAnchor.MiddleCenter;
        headerHG.childForceExpandWidth  = false;
        headerHG.childForceExpandHeight = true;
        headerHG.childControlWidth  = false;
        headerHG.childControlHeight = true;
        headerHG.spacing = 10;

        var backBtn  = MakeButton(header.transform, "BackButton",  "<",  60, 50);
        var levelTxt = MakeTMPText(header.transform, "LevelText", "LEVEL 1", 28, true);
        var levelLE  = levelTxt.AddComponent<LayoutElement>();
        levelLE.preferredWidth  = 220;
        levelLE.preferredHeight = 50;
        var restartBtn = MakeButton(header.transform, "RestartButton", "↺", 60, 50);

        // ── Label: Target ────────────────────────────────────────────────────
        var targetLabel = MakeTMPText(root.transform, "TargetLabel", "TARGET", 18, false);
        SetHeight(targetLabel.gameObject, 26);

        // ── Target Grid ──────────────────────────────────────────────────────
        var targetGridGO = MakeGridContainer(root.transform, "TargetGrid");

        // ── Label: Your Grid ─────────────────────────────────────────────────
        var playerLabel = MakeTMPText(root.transform, "PlayerLabel", "YOUR GRID", 18, false);
        SetHeight(playerLabel.gameObject, 26);

        // ── Player Grid ──────────────────────────────────────────────────────
        var playerGridGO = MakeGridContainer(root.transform, "PlayerGrid");

        // ── Color Buttons Panel ───────────────────────────────────────────────
        var btnPanel = MakePanel(root.transform, "ButtonsPanel");
        SetHeight(btnPanel, 80);
        var btnHG = btnPanel.AddComponent<HorizontalLayoutGroup>();
        btnHG.childAlignment    = TextAnchor.MiddleCenter;
        btnHG.childForceExpandWidth  = false;
        btnHG.childForceExpandHeight = false;
        btnHG.childControlWidth  = false;
        btnHG.childControlHeight = false;
        btnHG.spacing = 12;

        // ── Moves Text ───────────────────────────────────────────────────────
        var movesTxt = MakeTMPText(root.transform, "MovesText", "Moves: 0", 18, false);
        SetHeight(movesTxt.gameObject, 28);

        // ── Win Panel (overlay) ───────────────────────────────────────────────
        var winPanel = MakeImage(canvasGO.transform, "WinPanel", new Color(0,0,0,0.75f));
        StretchFill(winPanel.GetComponent<RectTransform>());
        winPanel.SetActive(false);

        var winVG = winPanel.AddComponent<VerticalLayoutGroup>();
        winVG.childAlignment    = TextAnchor.MiddleCenter;
        winVG.childForceExpandWidth  = true;
        winVG.childForceExpandHeight = false;
        winVG.childControlWidth  = true;
        winVG.childControlHeight = false;
        winVG.spacing = 20;
        winVG.padding = new RectOffset(40, 40, 0, 0);

        MakeTMPText(winPanel.transform, "WinTitle", "LEVEL COMPLETE!", 36, true);
        var nextBtn = MakeButton(winPanel.transform, "NextLevelButton", "NEXT LEVEL", 200, 60);
        var retryBtn = MakeButton(winPanel.transform, "RetryButton", "RETRY", 200, 60);

        // ── Prefabs ──────────────────────────────────────────────────────────
        var tilePrefab   = CreateTilePrefab();
        var colorBtnPrefab = CreateColorButtonPrefab();

        // ── PuzzleGrid components ─────────────────────────────────────────────
        var tGrid = targetGridGO.AddComponent<PuzzleGrid>();
        var tGridLG = targetGridGO.GetComponentInChildren<GridLayoutGroup>();
        SetPrivateField(tGrid, "layoutGroup", tGridLG);
        SetPrivateField(tGrid, "tilePrefab",  tilePrefab);

        var pGrid = playerGridGO.AddComponent<PuzzleGrid>();
        var pGridLG = playerGridGO.GetComponentInChildren<GridLayoutGroup>();
        SetPrivateField(pGrid, "layoutGroup", pGridLG);
        SetPrivateField(pGrid, "tilePrefab",  tilePrefab);

        // ── PuzzleUI ──────────────────────────────────────────────────────────
        var uiComp = canvasGO.AddComponent<PuzzleUI>();
        SetPrivateField(uiComp, "levelText",         levelTxt.GetComponent<TMP_Text>());
        SetPrivateField(uiComp, "movesText",         movesTxt.GetComponent<TMP_Text>());
        SetPrivateField(uiComp, "backButton",        backBtn.GetComponent<Button>());
        SetPrivateField(uiComp, "restartButton",     restartBtn.GetComponent<Button>());
        SetPrivateField(uiComp, "buttonsContainer",  btnPanel.transform);
        SetPrivateField(uiComp, "colorButtonPrefab", colorBtnPrefab);
        SetPrivateField(uiComp, "winPanel",          winPanel);
        SetPrivateField(uiComp, "nextLevelButton",   nextBtn.GetComponent<Button>());
        SetPrivateField(uiComp, "retryButton",       retryBtn.GetComponent<Button>());
        // Button listeners are wired at runtime in PuzzleUI.Awake() —
        // AddListener here would create non-serialized runtime delegates.

        // ── GameManager ───────────────────────────────────────────────────────
        var managerGO = new GameObject("PuzzleGameManager");
        var mgr = managerGO.AddComponent<PuzzleGameManager>();
        mgr.levels      = levels;
        mgr.targetGrid  = tGrid;
        mgr.playerGrid  = pGrid;
        mgr.ui          = uiComp;

        // ── Event System ──────────────────────────────────────────────────────
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        EditorUtility.SetDirty(canvasGO);
        EditorUtility.SetDirty(managerGO);
    }

    // ── Prefab creation ───────────────────────────────────────────────────────
    static Image CreateTilePrefab()
    {
        var go = new GameObject("TilePrefab");
        var img = go.AddComponent<Image>();
        img.color = Color.gray;
        // Slightly rounded look via built-in sprite
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1;

        string path = $"{PrefabFolder}/TilePrefab.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<Image>();
    }

    static ColorButtonUI CreateColorButtonPrefab()
    {
        var go = new GameObject("ColorButtonPrefab");
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(56, 56);

        // Outer circle image (the colored part)
        var img = go.AddComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        img.color  = Color.white;

        var btn = go.AddComponent<Button>();
        var btnColors = btn.colors;
        btnColors.highlightedColor = new Color(1,1,1,0.9f);
        btnColors.pressedColor     = new Color(0.7f,0.7f,0.7f,1f);
        btn.colors = btnColors;
        btn.targetGraphic = img;

        var cbui = go.AddComponent<ColorButtonUI>();
        SetPrivateField(cbui, "colorImage", img);

        string path = $"{PrefabFolder}/ColorButtonPrefab.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<ColorButtonUI>();
    }

    // ── UI helpers ────────────────────────────────────────────────────────────
    static GameObject MakeGridContainer(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(278, 210); // will be overwritten at runtime
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth  = 278;
        le.preferredHeight = 210;
        le.minHeight = 60;

        // Inner GridLayoutGroup child
        var inner = new GameObject("Grid");
        inner.transform.SetParent(go.transform, false);
        var irt = inner.AddComponent<RectTransform>();
        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
        irt.pivot     = new Vector2(0.5f, 0.5f);
        irt.sizeDelta = Vector2.zero;
        var lg = inner.AddComponent<GridLayoutGroup>();
        lg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        lg.constraintCount = 5;
        lg.cellSize = new Vector2(50,50);
        lg.spacing  = new Vector2(4,4);
        lg.childAlignment = TextAnchor.MiddleCenter;

        return go;
    }

    static GameObject MakePanel(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static GameObject MakeImage(Transform parent, string name, Color color)
    {
        var go = MakePanel(parent, name);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    static GameObject MakeTMPText(Transform parent, string name, string text, int size, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        return go;
    }

    static GameObject MakeButton(Transform parent, string name, string label, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth  = w;
        le.preferredHeight = h;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 22;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return go;
    }

    static void StretchFill(RectTransform rt)
    {
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    static void SetHeight(GameObject go, float h)
    {
        var le = go.GetOrAddComponent<LayoutElement>();
        le.minHeight       = h;
        le.preferredHeight = h;
    }

    static T GetOrAddComponent<T>(this GameObject go) where T : Component =>
        go.GetComponent<T>() ?? go.AddComponent<T>();

    // Reflection helper to set [SerializeField] private fields
    static void SetPrivateField(object target, string fieldName, object value)
    {
        var type  = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (field != null) { field.SetValue(target, value); return; }
            type = type.BaseType;
        }
        Debug.LogWarning($"[SetupPuzzleGame] Field '{fieldName}' not found on {target.GetType().Name}");
    }
}
