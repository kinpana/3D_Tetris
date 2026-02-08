using TMPro;
using UnityEngine;

public class RotationGuideUI : MonoBehaviour
{
    [SerializeField] private CameraController cam;
    [SerializeField] private TextMeshProUGUI text;

    private void Update()
    {
        if (cam == null || text == null) return;

        int yaw = cam.YawIndex & 3;

        // 表示は「入力→実際に回る軸」を出す
        string axis1 = ((yaw & 1) == 1) ? "Z" : "X";
        string axis3 = ((yaw & 1) == 1) ? "X" : "Z";

        text.text =
            $"Move: WASD (camera-relative)\n" +
            $"Rotate: 1={axis1}, 2=Y, 3={axis3}\n" +
            $"Shift + 1/2/3 = reverse";
    }
}
