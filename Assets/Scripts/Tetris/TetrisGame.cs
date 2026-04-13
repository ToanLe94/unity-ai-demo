using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Complete Tetris game — attaches to any GameObject in the scene and builds
/// its own Canvas + UI at runtime.  No prefabs or external assets required.
///
/// Controls:
///   Left / Right Arrow  : move (with DAS/ARR)
///   Down Arrow          : soft drop
///   Space               : hard drop
///   X  / Up Arrow       : rotate clockwise
///   Z                   : rotate counter-clockwise
///   C  / Left Shift     : hold
///   R                   : restart
/// </summary>
public class TetrisGame : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // Constants & Static Data
    // ─────────────────────────────────────────────────────────────────────────

    const int COLS       = 10;
    const int ROWS_VIS   = 20;              // visible playfield rows
    const int ROWS_SPAWN = 4;               // hidden buffer rows above
    const int ROWS_ALL   = ROWS_VIS + ROWS_SPAWN; // total grid height

    const float CELL      = 42f;            // canvas pixel size of each grid cell
    const float MINI_CELL = 26f;            // cell size for hold / preview panels
    const float LOCK_DELAY = 0.5f;          // seconds before piece locks
    const int   MAX_LOCK_RESETS = 15;       // max lock-delay resets per piece

    const float DAS = 0.133f;              // Delayed Auto Shift – initial delay
    const float ARR = 0.033f;              // Auto Repeat Rate

    // Points per lines cleared × current level  (Tetris Guideline)
    static readonly int[] LINE_PTS = { 0, 100, 300, 500, 800 };

    // Gravity: seconds-per-row for levels 1–15+ (Tetris Guideline)
    static readonly float[] GRAVITY = {
        1.000f, 0.793f, 0.618f, 0.473f, 0.355f,
        0.262f, 0.190f, 0.135f, 0.094f, 0.064f,
        0.043f, 0.028f, 0.018f, 0.011f, 0.007f
    };

    // Piece colours: I, O, T, S, Z, J, L
    static readonly Color32[] PC = {
        new Color32( 50, 210, 230, 255),  // I – cyan
        new Color32(230, 210,   0, 255),  // O – yellow
        new Color32(160,  50, 240, 255),  // T – purple
        new Color32( 50, 200,  50, 255),  // S – green
        new Color32(220,  50,  50, 255),  // Z – red
        new Color32( 50,  80, 230, 255),  // J – blue
        new Color32(230, 150,  20, 255),  // L – orange
    };

    static readonly Color32 C_EMPTY  = new Color32( 18,  18,  28, 255);
    static readonly Color32 C_BORDER = new Color32( 60,  60,  80, 255);
    static readonly Color32 C_BG     = new Color32( 10,  10,  18, 255);

    // ── SRS Piece Shapes ────────────────────────────────────────────────────
    // [pieceType][rotation][blockIndex]{col-offset, row-offset}
    // col-offset: right; row-offset: down.  All fit inside a 4×4 bounding box.
    static readonly int[][][][] SHAPES = {
        // 0 – I
        new[]{ new[]{ new[]{0,1},new[]{1,1},new[]{2,1},new[]{3,1} },   // rot 0
               new[]{ new[]{2,0},new[]{2,1},new[]{2,2},new[]{2,3} },   // rot 1
               new[]{ new[]{0,2},new[]{1,2},new[]{2,2},new[]{3,2} },   // rot 2
               new[]{ new[]{1,0},new[]{1,1},new[]{1,2},new[]{1,3} } }, // rot 3
        // 1 – O (same for all rotations)
        new[]{ new[]{ new[]{1,0},new[]{2,0},new[]{1,1},new[]{2,1} },
               new[]{ new[]{1,0},new[]{2,0},new[]{1,1},new[]{2,1} },
               new[]{ new[]{1,0},new[]{2,0},new[]{1,1},new[]{2,1} },
               new[]{ new[]{1,0},new[]{2,0},new[]{1,1},new[]{2,1} } },
        // 2 – T
        new[]{ new[]{ new[]{1,0},new[]{0,1},new[]{1,1},new[]{2,1} },
               new[]{ new[]{1,0},new[]{1,1},new[]{2,1},new[]{1,2} },
               new[]{ new[]{0,1},new[]{1,1},new[]{2,1},new[]{1,2} },
               new[]{ new[]{1,0},new[]{0,1},new[]{1,1},new[]{1,2} } },
        // 3 – S
        new[]{ new[]{ new[]{1,0},new[]{2,0},new[]{0,1},new[]{1,1} },
               new[]{ new[]{1,0},new[]{1,1},new[]{2,1},new[]{2,2} },
               new[]{ new[]{1,1},new[]{2,1},new[]{0,2},new[]{1,2} },
               new[]{ new[]{0,0},new[]{0,1},new[]{1,1},new[]{1,2} } },
        // 4 – Z
        new[]{ new[]{ new[]{0,0},new[]{1,0},new[]{1,1},new[]{2,1} },
               new[]{ new[]{2,0},new[]{1,1},new[]{2,1},new[]{1,2} },
               new[]{ new[]{0,1},new[]{1,1},new[]{1,2},new[]{2,2} },
               new[]{ new[]{1,0},new[]{0,1},new[]{1,1},new[]{0,2} } },
        // 5 – J
        new[]{ new[]{ new[]{0,0},new[]{0,1},new[]{1,1},new[]{2,1} },
               new[]{ new[]{1,0},new[]{2,0},new[]{1,1},new[]{1,2} },
               new[]{ new[]{0,1},new[]{1,1},new[]{2,1},new[]{2,2} },
               new[]{ new[]{1,0},new[]{1,1},new[]{0,2},new[]{1,2} } },
        // 6 – L
        new[]{ new[]{ new[]{2,0},new[]{0,1},new[]{1,1},new[]{2,1} },
               new[]{ new[]{1,0},new[]{1,1},new[]{1,2},new[]{2,2} },
               new[]{ new[]{0,1},new[]{1,1},new[]{2,1},new[]{0,2} },
               new[]{ new[]{0,0},new[]{1,0},new[]{1,1},new[]{1,2} } },
    };

    // ── SRS Wall-Kick Tables ─────────────────────────────────────────────────
    // Indexed by KickIdx(fromRot, toRot).  Convention: +x right, +y UP (SRS).
    // When applying, we do col += kx, row -= ky  (since our row increases downward).
    static readonly Vector2Int[][] KICKS_JLSTZ = {
        new[]{ V(0,0),V(-1,0),V(-1, 1),V(0,-2),V(-1,-2) }, // 0→1
        new[]{ V(0,0),V( 1,0),V( 1,-1),V(0, 2),V( 1, 2) }, // 1→0
        new[]{ V(0,0),V( 1,0),V( 1,-1),V(0, 2),V( 1, 2) }, // 1→2
        new[]{ V(0,0),V(-1,0),V(-1, 1),V(0,-2),V(-1,-2) }, // 2→1
        new[]{ V(0,0),V( 1,0),V( 1, 1),V(0,-2),V( 1,-2) }, // 2→3
        new[]{ V(0,0),V(-1,0),V(-1,-1),V(0, 2),V(-1, 2) }, // 3→2
        new[]{ V(0,0),V(-1,0),V(-1,-1),V(0, 2),V(-1, 2) }, // 3→0
        new[]{ V(0,0),V( 1,0),V( 1, 1),V(0,-2),V( 1,-2) }, // 0→3
    };
    static readonly Vector2Int[][] KICKS_I = {
        new[]{ V(0,0),V(-2,0),V( 1,0),V(-2,-1),V( 1, 2) }, // 0→1
        new[]{ V(0,0),V( 2,0),V(-1,0),V( 2, 1),V(-1,-2) }, // 1→0
        new[]{ V(0,0),V(-1,0),V( 2,0),V(-1, 2),V( 2,-1) }, // 1→2
        new[]{ V(0,0),V( 1,0),V(-2,0),V( 1,-2),V(-2, 1) }, // 2→1
        new[]{ V(0,0),V( 2,0),V(-1,0),V( 2, 1),V(-1,-2) }, // 2→3
        new[]{ V(0,0),V(-2,0),V( 1,0),V(-2,-1),V( 1, 2) }, // 3→2
        new[]{ V(0,0),V( 1,0),V(-2,0),V( 1,-2),V(-2, 1) }, // 3→0
        new[]{ V(0,0),V(-1,0),V( 2,0),V(-1, 2),V( 2,-1) }, // 0→3
    };

    static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    private AudioSource m_src;


    /// <summary>Maps a rotation transition to an index into the kick tables.</summary>
    static int KickIdx(int from, int to)
    {
        if (from == 0 && to == 1) return 0; if (from == 1 && to == 0) return 1;
        if (from == 1 && to == 2) return 2; if (from == 2 && to == 1) return 3;
        if (from == 2 && to == 3) return 4; if (from == 3 && to == 2) return 5;
        if (from == 3 && to == 0) return 6; if (from == 0 && to == 3) return 7;
        return 0;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Game State
    // ─────────────────────────────────────────────────────────────────────────

    // grid[row, col]: -1 = empty, 0-6 = locked piece type.
    // Row 0 = top of hidden buffer; row ROWS_ALL-1 = bottom of playfield.
    int[,] grid = new int[ROWS_ALL, COLS];

    int pType, pRot;     // current piece type (0-6) and rotation (0-3)
    int pRow,  pCol;     // top-left corner of piece's 4×4 bounding box in grid coords

    List<int> bag  = new List<int>(); // 7-bag source
    List<int> next = new List<int>(); // preview queue

    int  hold     = -1;   // held piece type (-1 = empty)
    bool holdUsed = false; // can only hold once per piece drop

    int  score = 0, level = 1, totalLines = 0, piecesPlaced = 0;
    float gameTime = 0f;
    bool gameOver = false;
    bool b2b     = false; // back-to-back Tetris tracker

    float gravTimer;       // accumulated gravity time
    float lockTimer;       // accumulated lock-delay time
    bool  isLocking;       // piece is on floor, lock delay running
    int   lockResets;      // how many times lock delay has been reset this piece

    float dasL, dasR;      // DAS accumulators for left/right movement
    bool  softDrop;

    bool uiReady = false;

    // ─────────────────────────────────────────────────────────────────────────
    // UI References
    // ─────────────────────────────────────────────────────────────────────────

    Image[,]   gfxGrid;          // [visualRow, col]  (visualRow 0 = bottom)
    Image[][]  gfxNext;          // [previewIdx 0-2][block 0-3]
    Image[]    gfxHold = new Image[4];

    TextMeshProUGUI txtScore, txtLevel, txtLines, txtTime, txtPieces;
    GameObject      pnlGameOver;

    // ─────────────────────────────────────────────────────────────────────────
    // MonoBehaviour
    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        ConfigCamera();
        BuildUI();
        StartGame();
        uiReady = true;
        m_src = GameObject.FindFirstObjectByType<AudioSource>();

    }

    void Update()
    {
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R)) StartGame();
            DrawFrame();
            return;
        }
        gameTime += Time.deltaTime;
        HandleInput();
        TickGravity();
        DrawFrame();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Camera
    // ─────────────────────────────────────────────────────────────────────────

    void ConfigCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.04f, 0.07f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI Construction
    // ─────────────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        float gridW = CELL * COLS;
        float gridH = CELL * ROWS_VIS;
        float sideW = MINI_CELL * 4 + 16f;  // width of hold / next side panels

        // ── Canvas ──
        var cvGO = new GameObject("TetrisCanvas");
        var cv   = cvGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cvGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight  = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();

        // ── Fullscreen background ──
        var bg = NewImg(cvGO, "BG");
        bg.color = C_BG;
        StretchFull(bg.rectTransform);

        // ── Playfield border (slightly larger than grid) ──
        var borderImg = NewImg(cvGO, "FieldBorder");
        borderImg.color = C_BORDER;
        SetRect(borderImg.rectTransform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(gridW + 4, gridH + 4));

        // ── Playfield background ──
        var fieldImg = NewImg(cvGO, "Field");
        fieldImg.color = C_EMPTY;
        SetRect(fieldImg.rectTransform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(gridW, gridH));

        // ── Grid cell images ──
        // gfxGrid[vr, c]: vr=0 → bottom of visible grid; vr=ROWS_VIS-1 → top.
        gfxGrid = new Image[ROWS_VIS, COLS];
        for (int vr = 0; vr < ROWS_VIS; vr++)
            for (int c = 0; c < COLS; c++)
            {
                var img = NewImg(cvGO, "Cell");
                float x = c * CELL + CELL * 0.5f - gridW * 0.5f;
                float y = vr * CELL + CELL * 0.5f - gridH * 0.5f;
                SetRect(img.rectTransform,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(x, y), new Vector2(CELL - 1, CELL - 1));
                img.color = C_EMPTY;
                gfxGrid[vr, c] = img;
            }

        // ── Left panel – HOLD ──
        float leftX  = -gridW * 0.5f - 8f - sideW * 0.5f;
        float topY   =  gridH * 0.5f;

        NewLabel(cvGO, "HOLD", 14,
            new Vector2(leftX, topY - 2), new Vector2(sideW, 22));

        // Hold piece mini-panel background
        float holdPanH = MINI_CELL * 2 + 4;
        var holdBg = NewImg(cvGO, "HoldBG");
        holdBg.color = new Color32(30, 30, 46, 200);
        SetRect(holdBg.rectTransform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(leftX, topY - 26 - holdPanH * 0.5f),
            new Vector2(MINI_CELL * 4 + 4, holdPanH));

        for (int i = 0; i < 4; i++)
        {
            gfxHold[i] = NewImg(cvGO, "HoldCell");
            gfxHold[i].color = C_EMPTY;
            gfxHold[i].gameObject.SetActive(false);
        }

        // ── Right panel – NEXT ──
        float rightX = gridW * 0.5f + 8f + sideW * 0.5f;

        NewLabel(cvGO, "NEXT", 14,
            new Vector2(rightX, topY - 2), new Vector2(sideW, 22));

        gfxNext = new Image[3][];
        for (int n = 0; n < 3; n++)
        {
            float nyOff = topY - 26 - n * (MINI_CELL * 2 + 10) - (MINI_CELL * 2 + 4) * 0.5f;
            var nbg = NewImg(cvGO, $"NextBG{n}");
            nbg.color = new Color32(30, 30, 46, 200);
            SetRect(nbg.rectTransform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(rightX, nyOff),
                new Vector2(MINI_CELL * 4 + 4, MINI_CELL * 2 + 4));

            gfxNext[n] = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                gfxNext[n][i] = NewImg(cvGO, "NextCell");
                gfxNext[n][i].color = C_EMPTY;
                gfxNext[n][i].gameObject.SetActive(false);
            }
        }

        // ── Right panel: Level only (below next previews) ──
        float infoY = topY - 26 - 3 * (MINI_CELL * 2 + 10) - 20;
        NewLabel(cvGO, "LEVEL", 12, new Vector2(rightX, infoY),      new Vector2(sideW, 18));
        txtLevel = NewValueText(cvGO, "1",                            new Vector2(rightX, infoY - 22), new Vector2(sideW, 28));

        // ── Left panel: SCORE / LINES / TIME / PIECES (below hold) ──
        float statY = topY - 26 - (MINI_CELL * 2 + 4) - 16f; // start just below hold panel
        const float STAT_GAP = 52f;

        NewLabel(cvGO, "SCORE", 12, new Vector2(leftX, statY), new Vector2(sideW, 18));
        txtScore = NewValueText(cvGO, "0", new Vector2(leftX, statY - 22), new Vector2(sideW, 28));
        statY -= STAT_GAP;

        NewLabel(cvGO, "LINES", 12, new Vector2(leftX, statY), new Vector2(sideW, 18));
        txtLines = NewValueText(cvGO, "0", new Vector2(leftX, statY - 22), new Vector2(sideW, 28));
        statY -= STAT_GAP;

        NewLabel(cvGO, "TIME", 12, new Vector2(leftX, statY), new Vector2(sideW, 18));
        txtTime = NewValueText(cvGO, "0:00", new Vector2(leftX, statY - 22), new Vector2(sideW, 28));
        statY -= STAT_GAP;

        NewLabel(cvGO, "PIECES", 12, new Vector2(leftX, statY), new Vector2(sideW, 18));
        txtPieces = NewValueText(cvGO, "0", new Vector2(leftX, statY - 22), new Vector2(sideW, 28));

        // Controls hint
        NewLabel(cvGO, "Z/X: Rotate  C: Hold  Space: Drop  R: Restart", 9,
            new Vector2(0, -gridH * 0.5f - 14), new Vector2(700, 18));

        // ── Game-Over overlay ──
        pnlGameOver = new GameObject("GameOverOverlay");
        var goRT = pnlGameOver.AddComponent<RectTransform>();
        goRT.SetParent(cv.transform, false);
        StretchFull(goRT);
        var goBG = pnlGameOver.AddComponent<Image>();
        goBG.color = new Color(0, 0, 0, 0.72f);

        var goTitle = new GameObject("GOTitle").AddComponent<TextMeshProUGUI>();
        goTitle.rectTransform.SetParent(pnlGameOver.transform, false);
        SetRect(goTitle.rectTransform,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0, 40), new Vector2(600, 70));
        goTitle.text = "GAME OVER";
        goTitle.fontSize = 52;
        goTitle.fontStyle = FontStyles.Bold;
        goTitle.color = new Color32(220, 70, 70, 255);
        goTitle.alignment = TextAlignmentOptions.Center;

        var goSub = new GameObject("GOSub").AddComponent<TextMeshProUGUI>();
        goSub.rectTransform.SetParent(pnlGameOver.transform, false);
        SetRect(goSub.rectTransform,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0, -20), new Vector2(600, 36));
        goSub.text = "Press  R  to restart";
        goSub.fontSize = 22;
        goSub.color = Color.white;
        goSub.alignment = TextAlignmentOptions.Center;

        pnlGameOver.SetActive(false);
    }

    // ── UI helpers ───────────────────────────────────────────────────────────

    Image NewImg(GameObject parent, string name)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go.AddComponent<Image>();
    }

    void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
                 Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    TextMeshProUGUI NewLabel(GameObject parent, string text, float fontSize,
                             Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(text + "_Label");
        go.transform.SetParent(parent.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = new Color32(160, 160, 180, 255);
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    TextMeshProUGUI NewValueText(GameObject parent, string text,
                                 Vector2 pos, Vector2 size)
    {
        var go  = new GameObject("ValueText");
        go.transform.SetParent(parent.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Game Initialization & Bag
    // ─────────────────────────────────────────────────────────────────────────

    void StartGame()
    {
        // Clear grid
        for (int r = 0; r < ROWS_ALL; r++)
            for (int c = 0; c < COLS; c++)
                grid[r, c] = -1;

        bag.Clear(); next.Clear();
        hold = -1; holdUsed = false;
        score = 0; level = 1; totalLines = 0; piecesPlaced = 0; gameTime = 0f;
        gameOver = false; b2b = false;
        gravTimer = 0; lockTimer = 0;
        isLocking = false; lockResets = 0;
        dasL = 0; dasR = 0;

        // Prime the preview queue with 3 pieces
        RefillBag();
        while (next.Count < 3) next.Add(DequeueBag());

        if (pnlGameOver != null) pnlGameOver.SetActive(false);
        SpawnPiece();
    }

    void RefillBag()
    {
        // Fisher-Yates shuffle of 0-6
        var pool = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        while (pool.Count > 0)
        {
            int i = Random.Range(0, pool.Count);
            bag.Add(pool[i]);
            pool.RemoveAt(i);
        }
    }

    int DequeueBag()
    {
        if (bag.Count == 0) RefillBag();
        int t = bag[0]; bag.RemoveAt(0);
        return t;
    }

    /// <summary>Takes the front of the preview queue and appends a new piece.</summary>
    int PopNext()
    {
        int t = next[0]; next.RemoveAt(0);
        next.Add(DequeueBag());
        return t;
    }

    void SpawnPiece()
    {
        pType = PopNext();
        pRot  = 0;
        pCol  = 3;
        // Bounding box starts 2 rows above the visible area so the piece
        // enters cleanly (SRS spawn rule).
        pRow  = ROWS_SPAWN - 2;
        holdUsed  = false;
        isLocking = false;
        lockTimer = 0;
        lockResets = 0;

        // Top-out: new piece immediately overlaps placed blocks → game over
        if (Collides(pType, pRot, pRow, pCol))
        {
            gameOver = true;
            if (pnlGameOver != null) pnlGameOver.SetActive(true);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Input Handling
    // ─────────────────────────────────────────────────────────────────────────

    void HandleInput()
    {
        // Rotate clockwise (X or Up), counter-clockwise (Z)
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.UpArrow))
            TryRotate(+1);
        if (Input.GetKeyDown(KeyCode.Z))
            TryRotate(-1);

        // Hold
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftShift))
            TryHold();

        // Hard drop
        if (Input.GetKeyDown(KeyCode.Space))
            HardDrop();

        // Soft drop flag
        softDrop = Input.GetKey(KeyCode.DownArrow);

        // Lateral movement with DAS / ARR
        bool lHeld = Input.GetKey(KeyCode.LeftArrow);
        bool rHeld = Input.GetKey(KeyCode.RightArrow);

        if (Input.GetKeyDown(KeyCode.LeftArrow))  { Move(-1); dasL = 0; }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { Move( 1); dasR = 0; }

        if (lHeld) { dasL += Time.deltaTime; if (dasL > DAS) { dasL -= ARR; Move(-1); } }
        else dasL = 0;

        if (rHeld) { dasR += Time.deltaTime; if (dasR > DAS) { dasR -= ARR; Move( 1); } }
        else dasR = 0;
    }

    void Move(int dx)
    {
        if (Collides(pType, pRot, pRow, pCol + dx)) return;
        pCol += dx;
        OnActiveMove();
    }

    /// <summary>
    /// SRS rotation with wall kicks.
    /// dir = +1 (clockwise), -1 (counter-clockwise).
    /// </summary>
    void TryRotate(int dir)
    {
        int newRot = (pRot + dir + 4) % 4;
        var kicks  = (pType == 0) ? KICKS_I : KICKS_JLSTZ; // I has special kicks
        var table  = kicks[KickIdx(pRot, newRot)];

        foreach (var k in table)
        {
            // SRS +y is up; our grid row increases downward → subtract k.y
            int tc = pCol + k.x;
            int tr = pRow - k.y;
            if (!Collides(pType, newRot, tr, tc))
            {
                pCol = tc; pRow = tr; pRot = newRot;
                OnActiveMove();
                return;
            }
        }
        // All kicks failed – rotation blocked
    }

    void TryHold()
    {
        if (holdUsed) return;   // only once per piece drop
        holdUsed = true;

        int prev = hold;
        hold = pType;

        if (prev == -1)
            SpawnPiece();       // no previously held piece – spawn next
        else
        {
            // Swap current piece with held piece
            pType = prev; pRot = 0; pCol = 3; pRow = ROWS_SPAWN - 2;
            isLocking = false; lockTimer = 0; lockResets = 0;
        }
    }

    void HardDrop()
    {
        // Move piece straight to its ghost (landing) position
        int dropped = 0;
        while (!Collides(pType, pRot, pRow + 1, pCol)) { pRow++; dropped++; }
        score += dropped * 2;   // hard drop: 2 pts per cell
        LockPiece();
    }

    /// <summary>
    /// Called after any successful move or rotation while on the floor.
    /// Resets the lock-delay timer (up to MAX_LOCK_RESETS times).
    /// </summary>
    void OnActiveMove()
    {
        if (isLocking && lockResets < MAX_LOCK_RESETS)
        {
            lockTimer = 0;
            lockResets++;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gravity & Locking
    // ─────────────────────────────────────────────────────────────────────────

    void TickGravity()
    {
        float spd = GRAVITY[Mathf.Clamp(level - 1, 0, GRAVITY.Length - 1)];
        if (softDrop) spd = Mathf.Min(spd, 0.05f); // soft drop ~20 rows/sec

        gravTimer += Time.deltaTime;
        while (gravTimer >= spd)
        {
            gravTimer -= spd;
            bool fell = TryFall();
            if (softDrop && fell) score += 1; // soft drop: 1 pt per cell
        }

        // Lock delay: once piece cannot fall, count down then lock
        bool onFloor = Collides(pType, pRot, pRow + 1, pCol);
        if (onFloor)
        {
            if (!isLocking) 
            { 
                isLocking = true; 
                lockTimer = 0; 
                PlaySoundSFXResources(drop, transform.position);
            }
            lockTimer += Time.deltaTime;
            if (lockTimer >= LOCK_DELAY) LockPiece();
        }
        else
        {
            isLocking = false;
        }
    }

    bool TryFall()
    {
        if (Collides(pType, pRot, pRow + 1, pCol)) return false;
        pRow++;
        isLocking = false; // piece is airborne again
        return true;
    }

    void LockPiece()
    {
        // Stamp current piece onto the grid
        foreach (var b in SHAPES[pType][pRot])
        {
            int r = pRow + b[1], c = pCol + b[0];
            if (r >= 0 && r < ROWS_ALL && c >= 0 && c < COLS)
                grid[r, c] = pType;
        }
        piecesPlaced++;
        ClearLines();
        SpawnPiece();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Collision Detection
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if any block of the piece at (row, col) with rotation rot
    /// would overlap the grid boundaries or an already-placed cell.
    /// Blocks above the top of the grid (row &lt; 0) are allowed (spawn buffer).
    /// </summary>
    bool Collides(int type, int rot, int row, int col)
    {
        foreach (var b in SHAPES[type][rot])
        {
            int r = row + b[1];
            int c = col + b[0];
            if (c < 0 || c >= COLS) return true;     // side wall
            if (r >= ROWS_ALL)      return true;     // floor
            if (r < 0)              continue;        // above grid – OK
            if (grid[r, c] != -1)   return true;     // occupied cell
        }
        return false;
    }

    /// <summary>Returns the row where the current piece would land (ghost position).</summary>
    int GhostRow()
    {
        int gr = pRow;
        while (!Collides(pType, pRot, gr + 1, pCol)) gr++;
        return gr;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Line Clearing & Scoring
    // ─────────────────────────────────────────────────────────────────────────

    void ClearLines()
    {
        int cleared = 0;
        // Scan from bottom upward; when a full row is found, remove it and
        // shift everything above down by one.
        for (int r = ROWS_ALL - 1; r >= 0; r--)
        {
            bool full = true;
            for (int c = 0; c < COLS; c++)
                if (grid[r, c] == -1) { full = false; break; }

            if (!full) continue;

            // Shift rows above this one down
            for (int rr = r; rr > 0; rr--)
                for (int c = 0; c < COLS; c++)
                    grid[rr, c] = grid[rr - 1, c];
            for (int c = 0; c < COLS; c++) grid[0, c] = -1;

            r++;        // re-check same index after shift
            cleared++;
        }

        if (cleared == 0) return;

        // ── Scoring ──
        totalLines += cleared;
        level = totalLines / 10 + 1;

        bool isTetris = (cleared == 4);
        int  pts      = LINE_PTS[Mathf.Clamp(cleared, 0, 4)] * level;

        // Back-to-back Tetris bonus: +50 % (rounded down)
        if (isTetris && b2b) pts += pts / 2;

        score += pts;
        b2b    = isTetris;
        PlaySoundSFXResources(eat, transform.position);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Rendering  (called every Update)
    // ─────────────────────────────────────────────────────────────────────────

    void DrawFrame()
    {
        if (!uiReady) return;
        DrawGrid();
        DrawMiniPieces();
        DrawScoreUI();
    }

    /// <summary>
    /// Updates gfxGrid images to show: placed blocks → ghost → active piece.
    /// Visual row 0 = bottom of the screen; ROWS_VIS-1 = top.
    /// Grid row 0 = top of hidden buffer; ROWS_ALL-1 = bottom of playfield.
    /// Mapping: visualRow = ROWS_ALL - 1 - gridRow.
    /// </summary>
    void DrawGrid()
    {
        // 1. Placed blocks
        for (int gr = ROWS_SPAWN; gr < ROWS_ALL; gr++)
        {
            int vr = ROWS_ALL - 1 - gr; // visual row
            for (int c = 0; c < COLS; c++)
            {
                int t = grid[gr, c];
                gfxGrid[vr, c].color = (t >= 0) ? (Color)PC[t] : (Color)C_EMPTY;
            }
        }

        // 2. Ghost piece (semi-transparent, same colour as active)
        int ghostRow = GhostRow();
        foreach (var b in SHAPES[pType][pRot])
        {
            int gr = ghostRow + b[1];
            int gc = pCol     + b[0];
            int vr = ROWS_ALL - 1 - gr;
            if (vr >= 0 && vr < ROWS_VIS && gc >= 0 && gc < COLS)
            {
                Color32 pc  = PC[pType];
                gfxGrid[vr, gc].color = new Color32(pc.r, pc.g, pc.b, 65);
            }
        }

        // 3. Active piece (fully opaque, drawn last so it covers the ghost)
        foreach (var b in SHAPES[pType][pRot])
        {
            int gr = pRow + b[1];
            int gc = pCol + b[0];
            int vr = ROWS_ALL - 1 - gr;
            if (vr >= 0 && vr < ROWS_VIS && gc >= 0 && gc < COLS)
                gfxGrid[vr, gc].color = PC[pType];
        }
    }

    /// <summary>
    /// Renders the hold and preview mini-pieces by positioning their four
    /// Image cells within their respective background panels.
    /// </summary>
    void DrawMiniPieces()
    {
        float gridW = CELL * COLS;
        float gridH = CELL * ROWS_VIS;
        float sideW = MINI_CELL * 4 + 16f;

        float leftX  = -gridW * 0.5f - 8f - sideW * 0.5f;
        float rightX =  gridW * 0.5f + 8f + sideW * 0.5f;
        float topY   =  gridH * 0.5f;

        float holdCenterY = topY - 26 - (MINI_CELL * 2 + 4) * 0.5f;
        PlaceMiniPiece(gfxHold, hold, leftX, holdCenterY, hold != -1 && holdUsed);

        for (int n = 0; n < 3 && n < next.Count; n++)
        {
            float ny = topY - 26 - n * (MINI_CELL * 2 + 10) - (MINI_CELL * 2 + 4) * 0.5f;
            PlaceMiniPiece(gfxNext[n], next[n], rightX, ny, false);
        }
    }

    /// <summary>
    /// Positions four Image cells to display a Tetromino in a small panel.
    /// panelCenter is in Canvas anchoredPosition units (relative to canvas centre).
    /// dimmed=true tints the piece grey (used for "hold already used").
    /// </summary>
    void PlaceMiniPiece(Image[] cells, int type, float panCX, float panCY, bool dimmed)
    {
        if (type < 0)
        {
            foreach (var c in cells) c.gameObject.SetActive(false);
            return;
        }

        var blocks = SHAPES[type][0]; // always show rotation 0 in previews
        Color32 col = dimmed ? new Color32(100, 100, 110, 255) : PC[type];

        for (int i = 0; i < 4; i++)
        {
            int dc = blocks[i][0];
            int dr = blocks[i][1];

            // Centre the piece within the panel by offsetting from panel centre.
            // Panel size = MINI_CELL*4 wide, MINI_CELL*2 tall.
            float lx = panCX + (dc - 1.5f) * MINI_CELL; // dc 0-3 centred
            float ly = panCY + (0.5f - dr) * MINI_CELL; // dr 0-1 centred, y-flip

            cells[i].gameObject.SetActive(true);
            cells[i].color = col;
            var rt = cells[i].rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(MINI_CELL - 2, MINI_CELL - 2);
            rt.anchoredPosition = new Vector2(lx, ly);
        }
    }

    void DrawScoreUI()
    {
        if (txtScore)  txtScore.text  = score.ToString("N0");
        if (txtLevel)  txtLevel.text  = level.ToString();
        if (txtLines)  txtLines.text  = totalLines.ToString();
        if (txtPieces) txtPieces.text = piecesPlaced.ToString();
        if (txtTime)
        {
            int mins = (int)(gameTime / 60);
            int secs = (int)(gameTime % 60);
            txtTime.text = $"{mins}:{secs:D2}";
        }
    }
    public AudioClip drop, eat;
    public void PlaySoundSFXResources(AudioClip clip, Vector3 pos, bool is2DSFX = true, float volumn = 1, bool isStopPrevSFX = false)
    {
      
        AudioSource audio = GetAudioSourceFromPool();
        audio.gameObject.SetActive(true);
        audio.transform.parent = transform;
        audio.transform.position = pos;
        audio.volume = 1 ;
        audio.spatialBlend = is2DSFX ? 0 : 1;
        audio.clip = clip;
        audio.Play();
        StartCoroutine(IReturnToPoolAudio(clip.length, audio));
    }

    private AudioSource GetAudioSourceFromPool()
    {
        GameObject clone = new GameObject("one shot audio");
        AudioSource audio = clone.AddComponent<AudioSource>();
        return audio;
    }

    IEnumerator IReturnToPoolAudio(float delay, AudioSource src)
    {
        yield return new WaitForSeconds(delay);
        Destroy(src.gameObject);
    }

}
