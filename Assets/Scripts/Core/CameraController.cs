using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera targetCamera;

    [Header("Orbit (Left/Right)")]
    [SerializeField] private float distance = 14f;
    [SerializeField] private float baseYawDeg = 180f;

    [Header("Vertical States")]
    [SerializeField] private float state0Height = 3f;
    [SerializeField] private float state0PitchDeg = 0f;

    [SerializeField] private float state1Height = 10f;
    [SerializeField] private float state1PitchDeg = -25f;

    [SerializeField] private float state2Height = 1f;
    [SerializeField] private float state2PitchDeg = +15f;

    [Header("Transition")]
    [SerializeField] private bool smooth = true;
    [SerializeField] private float smoothSpeed = 10f;

    private int yawIndex = 0; // 0..3
    private int vState = 0;   // 0,1,2

    public int YawIndex => yawIndex;

    private Vector3 targetPos;
    private Quaternion targetRot;

    private void Reset()
    {
        targetCamera = Camera.main;
    }

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        RecomputeTarget();
        Snap();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // ← → : yaw（あなたが設定した「逆方向」仕様を維持）
        if (kb.leftArrowKey.wasPressedThisFrame)
        {
            yawIndex = (yawIndex + 1) % 4;
            RecomputeTarget();
        }
        if (kb.rightArrowKey.wasPressedThisFrame)
        {
            yawIndex = (yawIndex + 3) % 4;
            RecomputeTarget();
        }

        // 上下の状態遷移（あなたの仕様）
        if (kb.upArrowKey.wasPressedThisFrame)
        {
            if (vState == 0) vState = 1;
            else if (vState == 2) vState = 0;
            RecomputeTarget();
        }

        if (kb.downArrowKey.wasPressedThisFrame)
        {
            if (vState == 0) vState = 2;
            else if (vState == 1) vState = 0;
            RecomputeTarget();
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        if (!smooth)
        {
            Snap();
            return;
        }

        targetCamera.transform.position =
            Vector3.Lerp(targetCamera.transform.position, targetPos, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        targetCamera.transform.rotation =
            Quaternion.Slerp(targetCamera.transform.rotation, targetRot, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
    }

    private void RecomputeTarget()
    {
        if (cameraPivot == null || targetCamera == null) return;

        float yaw = baseYawDeg + yawIndex * 90f;

        float h, pitch;
        switch (vState)
        {
            case 1: h = state1Height; pitch = state1PitchDeg; break;
            case 2: h = state2Height; pitch = state2PitchDeg; break;
            default: h = state0Height; pitch = state0PitchDeg; break;
        }

        Vector3 pivot = cameraPivot.position;
        Vector3 pivotAtHeight = new Vector3(pivot.x, h, pivot.z);

        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 offset = yawRot * new Vector3(0f, 0f, -distance);

        targetPos = pivotAtHeight + offset;
        targetRot = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void Snap()
    {
        targetCamera.transform.SetPositionAndRotation(targetPos, targetRot);
    }
}
