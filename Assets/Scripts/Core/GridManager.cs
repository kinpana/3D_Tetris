using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private BoardConfig config;         // Width/Depth/Height を持つ想定
    [SerializeField] private Transform gridOrigin;       // GridOrigin をドラッグ（角基準でOK）
    [SerializeField] private Transform blocksParent;     // 固定ブロックの親（Blocks など）
    [SerializeField] private GameObject blockPrefab;     // No.1 など 1x1x1

    [Header("Cell")]
    [SerializeField] private float cellSize = 1f;

    private Transform[,,] occupied;   // 固定ブロックの実体
        // セル色

    public int Width  => config != null ? config.Width  : 10;
    public int Depth  => config != null ? config.Depth  : 10;
    public int Height => config != null ? config.Height : 20;

    private void Awake()
    {
        Allocate();
    }

    private void OnValidate()
    {
        // Edit中でも配列を更新したい場合
        if (!Application.isPlaying) Allocate();
    }

    private void Allocate()
    {
        occupied = new Transform[Width, Height, Depth];
        
    }

    public void ClearAll()
    {
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        for (int z = 0; z < Depth; z++)
        {
            if (occupied[x, y, z] != null)
                Destroy(occupied[x, y, z].gameObject);

            occupied[x, y, z] = null;
           
        }
    }

    public bool InBounds(Vector3Int c)
    {
        return (0 <= c.x && c.x < Width) &&
               (0 <= c.y && c.y < Height) &&
               (0 <= c.z && c.z < Depth);
    }

    public bool IsOccupied(Vector3Int c)
    {
        if (!InBounds(c)) return true; // 範囲外は“埋まってる扱い”にして弾く
        return occupied[c.x, c.y, c.z] != null;
    }

    public bool CanPlace(System.Collections.Generic.List<Vector3Int> cells)
    {
        foreach (var c in cells)
        {
            if (!InBounds(c)) return false;
            if (occupied[c.x, c.y, c.z] != null) return false;
        }
        return true;
    }

    public Vector3 CellToWorld(Vector3Int cell)
    {
        Vector3 o = gridOrigin != null ? gridOrigin.position : Vector3.zero;
        return o + new Vector3(cell.x * cellSize, cell.y * cellSize, cell.z * cellSize);
    }

    // ロック：セル群を固定ブロック化（色も保存）
    public void PlaceCells(System.Collections.Generic.List<Vector3Int> cells, Color color)
    {
        foreach (var c in cells)
        {
            if (!InBounds(c)) continue;

            // 既にあるなら何もしない（安全）
            if (occupied[c.x, c.y, c.z] != null) continue;

            var go = Instantiate(blockPrefab, CellToWorld(c), Quaternion.identity, blocksParent);
            go.name = $"Locked_{c.x}_{c.y}_{c.z}";

            ApplyColor(go, color);

            occupied[c.x, c.y, c.z] = go.transform;
         
        }
    }

    private static void ApplyColor(GameObject go, Color c)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;

        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", c);
        
        r.SetPropertyBlock(mpb);
    }

    // 必要なら後で層消しをここに実装（現段階では省略）
    public int ClearFullLayers()
    {
        // 既存実装があるならそれを使ってOK。なければ後で追加。
        return 0;
    }
}
