using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tetris3D/Piece Definition")]
public class PieceDefinition : ScriptableObject
{
    public string id;

    [Tooltip("ローカルセル（pivotLocal基準の整数座標）")]
    public List<Vector3Int> localCells = new();

    [Tooltip("回転中心（ローカル）")]
    public Vector3Int pivotLocal = Vector3Int.zero;

    public Color color = Color.red;

    [Header("Difficulty / Spawn")]
    [Min(1)] public int complexity = 1;     // 1=簡単, 2=中, 3=難...
    [Min(0.01f)] public float weight = 1f;  // 同complexity内での出現比
}
