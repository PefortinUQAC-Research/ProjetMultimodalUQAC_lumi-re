using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TemperatureChanger : MonoBehaviour
{
    [Header("Reference to the Global Volume")]
    public Volume volume;

    [Header("Activation")]
    public bool enableScript = true;

    public enum Mode { Linear, Cycle, Fixed }
    public Mode mode = Mode.Fixed;


    [Header("White Balance Temperature")]
    [Range(-100f, 100f)] public float fixedTemperature = 0f;
    [Range(-100f, 100f)] public float minTemperature = 0f;
    [Range(-100f, 100f)] public float maxTemperature = 0f;
    [Min(0f)] public float duration = 60f;

    private WhiteBalance whiteBalance;
    private float timer = 0f;
    private bool transitionComplete = false;

    void Start()
    {
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

        if (!volume.profile.TryGet(out whiteBalance))
        {
            Debug.LogError("WhiteBalance is not enabled in the Volume Profile.");
            enabled = false;
            return;
        }

        whiteBalance.active = true;
        whiteBalance.temperature.overrideState = true;
    }

    void Update()
    {
        if (!enableScript || whiteBalance == null || transitionComplete) return;

        timer += Time.deltaTime;
        float t = 0f;
        float value = 0f;

        switch (mode)
        {
            case Mode.Linear:
                t = Mathf.Clamp01(timer / duration);
                value = Mathf.Lerp(minTemperature, maxTemperature, t);
                if (t >= 1f) transitionComplete = true;
                break;

            case Mode.Cycle:
                t = Mathf.PingPong(timer, duration) / duration;
                value = Mathf.Lerp(minTemperature, maxTemperature, t);
                break;
                
            case Mode.Fixed:
                value = fixedTemperature;
                break;
        }

        whiteBalance.temperature.value = value;
    }
}
