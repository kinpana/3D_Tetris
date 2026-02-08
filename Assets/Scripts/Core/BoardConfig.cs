using UnityEngine;

public class BoardConfig : MonoBehaviour
{
    [Min(1)] public int width = 10;   // X
    [Min(1)] public int depth = 10;   // Z
    [Min(1)] public int height = 20;  // Y
    [Min(0.1f)] public float cellSize = 1f;
}