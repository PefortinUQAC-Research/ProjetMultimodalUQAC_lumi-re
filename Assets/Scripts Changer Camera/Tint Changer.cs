using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TintChanger : MonoBehaviour
{
    [Header("Reference to the Global Volume")]
    public Volume volume;

    [Header("Activation")]
    public bool enableScript = true;

    public enum Mode { Fixed, Cycle, Linear } // Ajout du mode Linear
    public Mode mode = Mode.Fixed;

    [Header("Tint Parameters (WhiteBalance)")]
    [Range(-100, 100)] public int fixedTintValue = 0;
    [Range(-100, 100)] public int startTintValue = -50;
    [Range(-100, 100)] public int endTintValue = 50;
    [Min(0f)] public float duration = 5f;

    private WhiteBalance whiteBalance;
    private float timer = 0f;
    private bool linearDone = false;

    void Start()
    {
        // Auto-detect the volume if not manually assigned
        if (volume == null)
        {
            volume = FindFirstObjectByType<Volume>();
            if (volume == null)
            {
                Debug.LogError("No Volume found or assigned!");
                enabled = false;
                return;
            }
        }

        // Try to get the WhiteBalance override from the volume profile
        if (!volume.profile.TryGet(out whiteBalance))
        {
            Debug.LogError("WhiteBalance is not enabled in the Volume Profile.");
            enabled = false;
            return;
        }

        whiteBalance.active = true;
    }

    void Update()
    {
        if (!enableScript || whiteBalance == null) return;

        switch (mode)
        {
            case Mode.Fixed:
                whiteBalance.tint.value = fixedTintValue;
                break;

            case Mode.Cycle:
                if (duration > 0f)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.PingPong(timer, duration) / duration;
                    float value = Mathf.Lerp(startTintValue, endTintValue, t);
                    whiteBalance.tint.value = value;
                }
                break;

            case Mode.Linear:
                if (!linearDone && duration > 0f)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / duration);
                    float value = Mathf.Lerp(startTintValue, endTintValue, t);
                    whiteBalance.tint.value = value;

                    if (t >= 1f)
                        linearDone = true;
                }
                break;
        }
    }

    void OnEnable()
    {
        // Reset timer and flag when script is re-enabled
        timer = 0f;
        linearDone = false;
    }

    void OnValidate()
    {
        // Reset Linear mode if mode is changed in editor
        if (mode != Mode.Linear)
            linearDone = false;
    }
}
