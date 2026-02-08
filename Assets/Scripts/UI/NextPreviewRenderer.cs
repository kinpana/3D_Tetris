using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextPreviewRenderer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform root;   // NextPreviewRoot
    [SerializeField] private Image cellPrefab;     // 1セル分の四角（UI Image）
    [SerializeField] private float cellSize = 18f; // 1セルの表示サイズ（px）
    [SerializeField] private float gap = 2f;

    private readonly List<Image> spawned = new();

    public void Render(PieceDefinition def)
    {
        Clear();

        if (def == null || root == null || cellPrefab == null) return;

        // 3DセルをXZに投影（上から見た形）
        // ここでは y を無視して (x,z) を使う
        var cells = def.localCells;

        // bounds計算（投影平面で）
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;

        foreach (var c in cells)
        {
            minX = Mathf.Min(minX, c.x);
            maxX = Mathf.Max(maxX, c.x);
            minZ = Mathf.Min(minZ, c.z);
            maxZ = Mathf.Max(maxZ, c.z);
        }

        int w = maxX - minX + 1;
        int h = maxZ - minZ + 1;

        // 中央寄せ：全体サイズ
        float totalW = w * cellSize + (w - 1) * gap;
        float totalH = h * cellSize + (h - 1) * gap;

        // 左上基準で配置し、rootの中心に来るようにオフセット
        Vector2 origin = new Vector2(-totalW / 2f, totalH / 2f);

        foreach (var c in cells)
        {
            int x = c.x - minX;
            int z = c.z - minZ;

            var img = Instantiate(cellPrefab, root);
            img.name = "NextCell";
            img.color = def.color;
            spawned.Add(img);

            var rt = img.rectTransform;
            rt.sizeDelta = new Vector2(cellSize, cellSize);

            // UIはY上が+なので、Zを下方向に並べる
            float px = origin.x + x * (cellSize + gap) + cellSize / 2f;
            float py = origin.y - z * (cellSize + gap) - cellSize / 2f;

            rt.anchoredPosition = new Vector2(px, py);
        }
    }

    public void Clear()
    {
        foreach (var s in spawned) if (s) Destroy(s.gameObject);
        spawned.Clear();
    }
}
