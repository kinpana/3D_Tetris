using UnityEngine;
using UnityEngine.InputSystem;

public class PieceInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlow flow;
    [SerializeField] private CameraController camController; // yawIndex参照

    [Header("Repeat (optional)")]
    [SerializeField] private float moveRepeatDelay = 0.12f;

    private float moveTimer;

    private void Update()
    {
        if (flow == null) return;

        var active = flow.GetActivePiece();
        if (active == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // HOLD（C）: 既存の挙動を維持
        if (kb.cKey.wasPressedThisFrame)
        {
            flow.TryHold();
            return;
        }

        int yaw = camController != null ? (camController.YawIndex & 3) : 0;

        // Shiftで逆回転
        bool inverse = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
        TurnDir dir = inverse ? TurnDir.CCW : TurnDir.CW;

        // 回転（1/2/3）: カメラyawに応じてX/Zを入れ替え（2はY固定）
        if (kb.digit1Key.wasPressedThisFrame) active.TryRotate(MapAxisByYawForKey(1, yaw), dir);
        if (kb.digit2Key.wasPressedThisFrame) active.TryRotate(Axis.Y, dir);
        if (kb.digit3Key.wasPressedThisFrame) active.TryRotate(MapAxisByYawForKey(3, yaw), dir);

        // 移動（WASD）: カメラ相対
        bool pressed =
            kb.wKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame ||
            kb.sKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;

        bool held =
            kb.wKey.isPressed || kb.aKey.isPressed ||
            kb.sKey.isPressed || kb.dKey.isPressed;

        if (pressed)
        {
            Vector3Int d = GetMoveDeltaCameraRelative(kb, yaw);
            if (d != Vector3Int.zero) active.TryMove(d);
            moveTimer = 0f;
        }
        else if (held)
        {
            moveTimer += Time.deltaTime;
            if (moveTimer >= moveRepeatDelay)
            {
                moveTimer = 0f;
                Vector3Int d = GetMoveDeltaCameraRelative(kb, yaw);
                if (d != Vector3Int.zero) active.TryMove(d);
            }
        }
        else
        {
            moveTimer = 0f;
        }
    }

    private static Axis MapAxisByYawForKey(int key, int yaw)
    {
        // 2はY固定。それ以外は yaw が奇数のとき X/Z 入れ替え。
        bool swapXZ = (yaw & 1) == 1;

        if (key == 1) return swapXZ ? Axis.Z : Axis.X;
        if (key == 3) return swapXZ ? Axis.X : Axis.Z;
        return Axis.Y;
    }

    private static Vector3Int GetMoveDeltaCameraRelative(Keyboard kb, int yaw)
    {
        // yaw=0 で W=+Z, D=+X になる定義（必要ならここを反転して調整）
        Vector3Int forward, right;
        switch (yaw & 3)
        {
            case 0: forward = new Vector3Int(0, 0, +1); right = new Vector3Int(+1, 0, 0); break;
            case 1: forward = new Vector3Int(+1, 0, 0); right = new Vector3Int(0, 0, -1); break;
            case 2: forward = new Vector3Int(0, 0, -1); right = new Vector3Int(-1, 0, 0); break;
            default: forward = new Vector3Int(-1, 0, 0); right = new Vector3Int(0, 0, +1); break;
        }

        if (kb.wKey.isPressed) return forward;
        if (kb.sKey.isPressed) return -forward;
        if (kb.dKey.isPressed) return right;
        if (kb.aKey.isPressed) return -right;
        return Vector3Int.zero;
    }
}
