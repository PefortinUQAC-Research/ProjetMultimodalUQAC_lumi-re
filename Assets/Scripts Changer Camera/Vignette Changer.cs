using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteChanger : MonoBehaviour
{
    [Header("Reference to the Global Volume")]
    public Volume volume;

    [Header("Activation")]
    public bool enableScript = true;

    public enum Mode { Fixed, Cycle, Linear }

    [Header("Cycle Settings")]
    [Min(0.01f)] public float duration = 5f;

    private Vignette vignette;
    private float timer = 0f;

    [Header("Color (default: Black)")]
    public Mode colorMode = Mode.Fixed;
    public Color fixedColor = Color.black;
    public Color startColor = Color.black;
    public Color endColor = Color.white;

    [Header("Center (default: X=0.5, Y=0.5)")]
    public Mode centerMode = Mode.Fixed;
    public Vector2 fixedCenter = new Vector2(0.5f, 0.5f);
    public Vector2 startCenter = new Vector2(0.4f, 0.4f);
    public Vector2 endCenter = new Vector2(0.6f, 0.6f);

    [Header("Intensity (default: 0.0)")]
    public Mode intensityMode = Mode.Fixed;
    [Range(0f, 1f)] public float fixedIntensity = 0f;
    [Range(0f, 1f)] public float startIntensity = 0f;
    [Range(0f, 1f)] public float endIntensity = 1f;

    [Header("Smoothness (default: 0.2)")]
    public Mode smoothnessMode = Mode.Fixed;
    [Range(0.01f, 1f)] public float fixedSmoothness = 0.2f;
    [Range(0.01f, 1f)] public float startSmoothness = 0.1f;
    [Range(0.01f, 1f)] public float endSmoothness = 1f;

    [Header("Rounded (default: Off)")]
    public bool rounded = false;

    // Linear mode flags
    private bool linearColorDone = false;
    private bool linearCenterDone = false;
    private bool linearIntensityDone = false;
    private bool linearSmoothnessDone = false;

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

        if (!volume.profile.TryGet(out vignette))
        {
            Debug.LogError("Vignette is not enabled in the Volume Profile.");
            enabled = false;
            return;
        }

        vignette.active = true;
    }

    void Update()
    {
        if (!enableScript || vignette == null) return;

        timer += Time.deltaTime;
        float tCycle = Mathf.PingPong(timer, duration) / duration;
        float tLinear = Mathf.Clamp01(timer / duration);

        // Color
        switch (colorMode)
        {
            case Mode.Fixed:
                vignette.color.value = fixedColor;
                break;
            case Mode.Cycle:
                vignette.color.value = Color.Lerp(startColor, endColor, tCycle);
                break;
            case Mode.Linear:
                if (!linearColorDone)
                {
                    vignette.color.value = Color.Lerp(startColor, endColor, tLinear);
                    if (tLinear >= 1f) linearColorDone = true;
                }
                break;
        }

        // Center
        switch (centerMode)
        {
            case Mode.Fixed:
                vignette.center.value = fixedCenter;
                break;
            case Mode.Cycle:
                vignette.center.value = Vector2.Lerp(startCenter, endCenter, tCycle);
                break;
            case Mode.Linear:
                if (!linearCenterDone)
                {
                    vignette.center.value = Vector2.Lerp(startCenter, endCenter, tLinear);
                    if (tLinear >= 1f) linearCenterDone = true;
                }
                break;
        }

        // Intensity
        switch (intensityMode)
        {
            case Mode.Fixed:
                vignette.intensity.value = fixedIntensity;
                break;
            case Mode.Cycle:
                vignette.intensity.value = Mathf.Lerp(startIntensity, endIntensity, tCycle);
                break;
            case Mode.Linear:
                if (!linearIntensityDone)
                {
                    vignette.intensity.value = Mathf.Lerp(startIntensity, endIntensity, tLinear);
                    if (tLinear >= 1f) linearIntensityDone = true;
                }
                break;
        }

        // Smoothness
        switch (smoothnessMode)
        {
            case Mode.Fixed:
                vignette.smoothness.value = fixedSmoothness;
                break;
            case Mode.Cycle:
                vignette.smoothness.value = Mathf.Lerp(startSmoothness, endSmoothness, tCycle);
                break;
            case Mode.Linear:
                if (!linearSmoothnessDone)
                {
                    vignette.smoothness.value = Mathf.Lerp(startSmoothness, endSmoothness, tLinear);
                    if (tLinear >= 1f) linearSmoothnessDone = true;
                }
                break;
        }

        // Rounded
        vignette.rounded.value = rounded;
    }

    void OnEnable()
    {
        timer = 0f;
        ResetLinearFlags();
    }

    void OnValidate()
    {
        ResetLinearFlags();
    }

    private void ResetLinearFlags()
    {
        linearColorDone = false;
        linearCenterDone = false;
        linearIntensityDone = false;
        linearSmoothnessDone = false;
    }
}
