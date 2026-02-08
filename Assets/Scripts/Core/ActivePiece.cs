using System.Collections.Generic;
using UnityEngine;

public class ActivePiece : MonoBehaviour
{
    public PieceDefinition CurrentDefinition { get; private set; }

    private GridManager grid;
    private GameObject blockPrefab;

    private Vector3Int pivotCell;               // グリッド座標（整数）
    private readonly List<Vector3Int> localCells = new();
    private readonly List<Transform> blocks = new();

    public Vector3Int PivotCell => pivotCell;

    public void Init(GridManager grid, GameObject blockPrefab, PieceDefinition def, Vector3Int spawnPivot)
    {
        this.grid = grid;
        this.blockPrefab = blockPrefab;

        CurrentDefinition = def;

        pivotCell = spawnPivot;
        localCells.Clear();
        localCells.AddRange(def.localCells);

        BuildBlocks(def);
        SyncWorld();
    }

    // 現在のピースが占有するセル（ワールド=グリッド）
    public List<Vector3Int> GetWorldCells()
    {
        var world = new List<Vector3Int>(localCells.Count);
        foreach (var c in localCells)
            world.Add(pivotCell + c);
        return world;
    }

    public bool TryMove(Vector3Int delta)
    {
        var nextPivot = pivotCell + delta;
        if (!grid.CanPlace(ComposeWorldCells(nextPivot, localCells)))
            return false;

        pivotCell = nextPivot;
        SyncWorld();
        return true;
    }

    public bool TryRotate(Axis axis, TurnDir dir)
    {
        // 90度回転（整数格子）：X/Y/Z軸回りのローカルセル回転
        var rotated = new List<Vector3Int>(localCells.Count);
        foreach (var c in localCells)
            rotated.Add(RotateCell90(c, axis, dir));

        if (!grid.CanPlace(ComposeWorldCells(pivotCell, rotated)))
            return false; // 壁キック無し：めり込むなら回転不可

        localCells.Clear();
        localCells.AddRange(rotated);
        SyncWorld();
        return true;
    }

    private static List<Vector3Int> ComposeWorldCells(Vector3Int pivot, List<Vector3Int> locals)
    {
        var w = new List<Vector3Int>(locals.Count);
        foreach (var c in locals) w.Add(pivot + c);
        return w;
    }

    private void BuildBlocks(PieceDefinition def)
    {
        // 既存ブロック破棄
        foreach (var t in blocks) if (t) Destroy(t.gameObject);
        blocks.Clear();

        // ローカルセル数だけブロック生成
        for (int i = 0; i < localCells.Count; i++)
        {
            var go = Instantiate(blockPrefab, transform);
            go.name = $"Block_{i}";
            blocks.Add(go.transform);

            // 色適用（Prefab No.1固定問題の対策）
            var r = go.GetComponent<Renderer>();
            ApplyColor(r, def.color);
        }
    }

    private void SyncWorld()
    {
        // grid側に「セル→ワールド座標」変換がある前提
        // なければ：gridOrigin + cell * cellSize を自前で計算する必要あり
        for (int i = 0; i < blocks.Count; i++)
        {
            var cell = pivotCell + localCells[i];
            blocks[i].position = grid.CellToWorld(cell);
        }
    }

    private static void ApplyColor(Renderer r, Color c)
    {
        if (r == null) return;

        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        // URP Lit / Built-in 両対応（どちらか効けばOK）
        mpb.SetColor("_BaseColor", c);
        mpb.SetColor("_Color", c);

        r.SetPropertyBlock(mpb);
    }

    private static Vector3Int RotateCell90(Vector3Int c, Axis axis, TurnDir dir)
    {
        // 右ねじ（CW）/逆（CCW）の定義はゲーム内で統一されていればOK
        // ここでは「+90度」をCWとして扱う（必要なら入れ替え可能）
        int s = (dir == TurnDir.CW) ? 1 : -1;

        return axis switch
        {
            Axis.X => new Vector3Int(c.x, -s * c.z, s * c.y),
            Axis.Y => new Vector3Int(s * c.z, c.y, -s * c.x),
            Axis.Z => new Vector3Int(-s * c.y, s * c.x, c.z),
            _ => c
        };
    }
}
