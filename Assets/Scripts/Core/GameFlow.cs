using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class GameFlow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager grid;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private ActivePiece activePiecePrefab;
    [SerializeField] private Transform activePieceParent;

    [SerializeField] private HudController hud;                 // Optional
    [SerializeField] private NextPreviewRenderer nextPreview;   // Optional
    [SerializeField] private NextPreviewRenderer holdPreview;   // Optional

    [Header("Piece Set (all available from start)")]
    [SerializeField] private List<PieceDefinition> pieces = new();

    [Header("Spawn")]
    [SerializeField] private Vector3Int spawnPivot = new Vector3Int(4, 19, 4);

    [Header("Level / Speed")]
    [SerializeField] private int startLevel = 1;

    [Tooltip("Level increases after this many cleared layers.")]
    [SerializeField] private int levelUpEveryClears = 2;

    [Tooltip("Fall interval by level (seconds). Smaller = faster.")]
    [SerializeField] private AnimationCurve fallIntervalByLevel =
        new AnimationCurve(
            new Keyframe(1, 0.90f),
            new Keyframe(5, 0.65f),
            new Keyframe(10, 0.40f),
            new Keyframe(20, 0.20f)
        );

    [SerializeField] private float fallIntervalMin = 0.12f;

    [Header("Scoring (optional)")]
    [SerializeField] private bool enableScoring = true;
    [SerializeField] private int scorePer1 = 100;
    [SerializeField] private int scorePer2 = 300;
    [SerializeField] private int scorePer3 = 500;
    [SerializeField] private int scorePer4Plus = 800;

    [Header("Game Over UI (optional)")]
    [SerializeField] private GameObject gameOverOverlay;

    private ActivePiece activePiece;
    private float fallTimer;

    private int level;
    private int layersClearedTotal;
    private int score;
    private bool gameOver;

    // NEXT
    private PieceDefinition nextDef;

    // HOLD
    private PieceDefinition holdDef;
    private bool holdUsedThisTurn;

    public ActivePiece GetActivePiece() => activePiece;
    public int Level => level;
    public int LayersClearedTotal => layersClearedTotal;
    public int Score => score;
    public bool IsGameOver => gameOver;

    private void Start()
    {
        level = Mathf.Max(1, startLevel);
        layersClearedTotal = 0;
        score = 0;
        gameOver = false;
        fallTimer = 0f;

        ShowGameOver(false);

        if (!ValidateRefs())
        {
            gameOver = true;
            ShowGameOver(true);
            return;
        }

        // Reserve NEXT before first spawn
        nextDef = PickWeightedPieceAll();
        nextPreview?.Render(nextDef);

        // Clear HOLD UI
        holdDef = null;
        holdUsedThisTurn = false;
        holdPreview?.Clear();

        NotifyHud();
        SpawnNewPiece();
    }

    private void Update()
    {
        // ★ゲームオーバーでもRで復帰できるようにする
        var kb = Keyboard.current;
        if (kb != null && kb.rKey.wasPressedThisFrame)
        {
            Restart();
            return;
        }

        if (gameOver) return;
        if (activePiece == null) return;

        fallTimer += Time.deltaTime;
        if (fallTimer >= CurrentFallInterval())
        {
            fallTimer = 0f;
            StepFall();
        }
    }


    private float CurrentFallInterval()
    {
        float v = fallIntervalByLevel != null ? fallIntervalByLevel.Evaluate(level) : 0.9f;
        return Mathf.Max(fallIntervalMin, v);
    }

    private void StepFall()
    {
        if (activePiece.TryMove(new Vector3Int(0, -1, 0)))
            return;

        LockPiece();
    }

    private void LockPiece()
    {
        grid.PlaceCells(activePiece.GetWorldCells());

        Destroy(activePiece.gameObject);
        activePiece = null;

        int cleared = grid.ClearFullLayers();
        if (cleared > 0)
        {
            layersClearedTotal += cleared;

            if (enableScoring)
            {
                score += cleared switch
                {
                    1 => scorePer1,
                    2 => scorePer2,
                    3 => scorePer3,
                    _ => scorePer4Plus
                };
            }

            int targetLevel = 1 + (layersClearedTotal / Mathf.Max(1, levelUpEveryClears));
            if (targetLevel > level) level = targetLevel;
        }

        // HOLDはロック後に解禁
        holdUsedThisTurn = false;

        NotifyHud();
        SpawnNewPiece();
    }

    private void SpawnNewPiece()
    {
        if (gameOver) return;
        if (!ValidateRefs())
        {
            gameOver = true;
            ShowGameOver(true);
            return;
        }

        // Use reserved nextDef; if null, pick now
        var def = nextDef != null ? nextDef : PickWeightedPieceAll();

        // Reserve next
        nextDef = PickWeightedPieceAll();
        nextPreview?.Render(nextDef);

        activePiece = Instantiate(activePiecePrefab, Vector3.zero, Quaternion.identity, activePieceParent);
        activePiece.name = "ActivePiece";
        activePiece.Init(grid, blockPrefab, def, spawnPivot);

        if (!grid.CanPlace(activePiece.GetWorldCells()))
        {
            Destroy(activePiece.gameObject);
            activePiece = null;
            gameOver = true;
            ShowGameOver(true);
        }

        NotifyHud();
    }

    private void SpawnSpecificPiece(PieceDefinition def)
    {
        if (def == null || gameOver) return;

        activePiece = Instantiate(activePiecePrefab, Vector3.zero, Quaternion.identity, activePieceParent);
        activePiece.name = "ActivePiece";
        activePiece.Init(grid, blockPrefab, def, spawnPivot);

        if (!grid.CanPlace(activePiece.GetWorldCells()))
        {
            Destroy(activePiece.gameObject);
            activePiece = null;
            gameOver = true;
            ShowGameOver(true);
        }
    }

    // Cキーから呼ぶ
    public void TryHold()
    {
        if (gameOver) return;
        if (activePiece == null) return;
        if (holdUsedThisTurn) return;

        var cur = activePiece.CurrentDefinition;
        if (cur == null) return;

        holdUsedThisTurn = true;

        Destroy(activePiece.gameObject);
        activePiece = null;

        if (holdDef == null)
        {
            holdDef = cur;
            holdPreview?.Render(holdDef);

            // nextDefを消費して次をスポーン
            SpawnNewPiece();
        }
        else
        {
            var temp = holdDef;
            holdDef = cur;
            holdPreview?.Render(holdDef);

            // Holdから取り出したものをスポーン（NEXTはズラさない）
            SpawnSpecificPiece(temp);
        }

        NotifyHud();
    }

    public void Restart()
    {
        grid.ClearAll();

        if (activePiece != null)
        {
            Destroy(activePiece.gameObject);
            activePiece = null;
        }

        level = Mathf.Max(1, startLevel);
        layersClearedTotal = 0;
        score = 0;
        gameOver = false;
        fallTimer = 0f;

        holdDef = null;
        holdUsedThisTurn = false;
        holdPreview?.Clear();

        nextDef = PickWeightedPieceAll();
        nextPreview?.Render(nextDef);

        ShowGameOver(false);
        NotifyHud();

        SpawnNewPiece();
    }

    private PieceDefinition PickWeightedPieceAll()
    {
        float total = 0f;
        var candidates = new List<PieceDefinition>();

        foreach (var p in pieces)
        {
            if (p == null) continue;

            float w = Mathf.Max(0f, p.weight);
            if (w <= 0f) continue;

            candidates.Add(p);
            total += w;
        }

        if (candidates.Count == 0 || total <= 0f) return null;

        float r = Random.Range(0f, total);
        float acc = 0f;
        foreach (var c in candidates)
        {
            acc += Mathf.Max(0f, c.weight);
            if (r <= acc) return c;
        }
        return candidates[candidates.Count - 1];
    }

    private bool ValidateRefs()
    {
        if (grid == null || blockPrefab == null || activePiecePrefab == null)
        {
            Debug.LogError("GameFlow: Missing references (grid / blockPrefab / activePiecePrefab).");
            return false;
        }
        if (pieces == null || pieces.Count == 0)
        {
            Debug.LogError("GameFlow: No PieceDefinitions assigned.");
            return false;
        }
        return true;
    }

    private void NotifyHud()
    {
        if (hud == null) return;

        hud.SetScore(score);
        hud.SetLevel(level);
        hud.SetClears(layersClearedTotal);
    }

    private void ShowGameOver(bool show)
    {
        if (gameOverOverlay != null) gameOverOverlay.SetActive(show);
    }
}
