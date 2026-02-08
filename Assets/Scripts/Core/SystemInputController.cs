using UnityEngine;
using UnityEngine.InputSystem;

public class SystemInputController : MonoBehaviour
{
    [SerializeField] private GameFlow flow;

    private bool paused;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        if (kb.rKey.wasPressedThisFrame)
        {
            // ポーズ中でも戻したいなら先に解除
            paused = false;
            Time.timeScale = 1f;

            flow.Restart();
        }
    }
}
