using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorAdjustmentsChanger : MonoBehaviour
{
    [Header("Reference to the Global Volume")]
    public Volume volume;

    [Header("Activation")]
    public bool enableScript = true;
    public bool caramelDanced = false;

    public enum Mode { Fixed, Cycle, Linear } // Nouveau mode

    [Header("Post Exposure")]
    public Mode postExposureMode = Mode.Fixed;
    public float fixedPostExposure = 0.5f;
    public float startPostExposure = 0f;
    public float endPostExposure = 1f;

    [Header("Contrast")]
    public Mode contrastMode = Mode.Fixed;
    [Range(-100f, 100f)] public float fixedContrast = 0f;
    [Range(-100f, 100f)] public float startContrast = -50f;
    [Range(-100f, 100f)] public float endContrast = 50f;

    [Header("Color Filter")]
    public Mode colorFilterMode = Mode.Fixed;
    [ColorUsage(true, true)] public Color fixedColorFilter = Color.white;
    [ColorUsage(true, true)] public Color startColorFilter = Color.black;
    [ColorUsage(true, true)] public Color endColorFilter = Color.white;

    [Header("Hue Shift")]
    public Mode hueShiftMode = Mode.Fixed;
    [Range(-180f, 180f)] public float fixedHueShift = 0f;
    [Range(-180f, 180f)] public float startHueShift = -90f;
    [Range(-180f, 180f)] public float endHueShift = 90f;

    [Header("Saturation")]
    public Mode saturationMode = Mode.Fixed;
    [Range(-100f, 100f)] public float fixedSaturation = 0f;
    [Range(-100f, 100f)] public float startSaturation = -50f;
    [Range(-100f, 100f)] public float endSaturation = 50f;

    [Header("Cycle Duration (seconds)")]
    [Min(0.01f)] public float duration = 5f;

    private ColorAdjustments colorAdjustments;
    private float timer = 0f;

    // Linear flags
    private bool linearPostExposureDone = false;
    private bool linearContrastDone = false;
    private bool linearColorFilterDone = false;
    private bool linearHueShiftDone = false;
    private bool linearSaturationDone = false;

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

        if (!volume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("ColorAdjustments is not enabled in the Volume Profile.");
            enabled = false;
            return;
        }

        colorAdjustments.active = true;
    }

    void Update()
    {
        if (!enableScript || colorAdjustments == null) return;

        timer += Time.deltaTime;
        float tCycle = (duration > 0f) ? Mathf.PingPong(timer, duration) / duration : 0f;
        float tLinear = Mathf.Clamp01(timer / duration);

        // Post Exposure
        switch (postExposureMode)
        {
            case Mode.Fixed:
                colorAdjustments.postExposure.value = fixedPostExposure;
                break;
            case Mode.Cycle:
                colorAdjustments.postExposure.value = Mathf.Lerp(startPostExposure, endPostExposure, tCycle);
                break;
            case Mode.Linear:
                if (!linearPostExposureDone)
                {
                    colorAdjustments.postExposure.value = Mathf.Lerp(startPostExposure, endPostExposure, tLinear);
                    if (tLinear >= 1f) linearPostExposureDone = true;
                }
                break;
        }

        // Contrast
        switch (contrastMode)
        {
            case Mode.Fixed:
                colorAdjustments.contrast.value = fixedContrast;
                break;
            case Mode.Cycle:
                colorAdjustments.contrast.value = Mathf.Lerp(startContrast, endContrast, tCycle);
                break;
            case Mode.Linear:
                if (!linearContrastDone)
                {
                    colorAdjustments.contrast.value = Mathf.Lerp(startContrast, endContrast, tLinear);
                    if (tLinear >= 1f) linearContrastDone = true;
                }
                break;
        }

        // Color Filter
        if (caramelDanced)
        {
            float hue = Mathf.Repeat(timer / duration, 1f);
            colorAdjustments.colorFilter.value = Color.HSVToRGB(hue, 1f, 1f);
        }
        else
        {
            switch (colorFilterMode)
            {
                case Mode.Fixed:
                    colorAdjustments.colorFilter.value = fixedColorFilter;
                    break;
                case Mode.Cycle:
                    colorAdjustments.colorFilter.value = Color.Lerp(startColorFilter, endColorFilter, tCycle);
                    break;
                case Mode.Linear:
                    if (!linearColorFilterDone)
                    {
                        colorAdjustments.colorFilter.value = Color.Lerp(startColorFilter, endColorFilter, tLinear);
                        if (tLinear >= 1f) linearColorFilterDone = true;
                    }
                    break;
            }
        }

        // Hue Shift
        switch (hueShiftMode)
        {
            case Mode.Fixed:
                colorAdjustments.hueShift.value = fixedHueShift;
                break;
            case Mode.Cycle:
                colorAdjustments.hueShift.value = Mathf.Lerp(startHueShift, endHueShift, tCycle);
                break;
            case Mode.Linear:
                if (!linearHueShiftDone)
                {
                    colorAdjustments.hueShift.value = Mathf.Lerp(startHueShift, endHueShift, tLinear);
                    if (tLinear >= 1f) linearHueShiftDone = true;
                }
                break;
        }

        // Saturation
        switch (saturationMode)
        {
            case Mode.Fixed:
                colorAdjustments.saturation.value = fixedSaturation;
                break;
            case Mode.Cycle:
                colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, endSaturation, tCycle);
                break;
            case Mode.Linear:
                if (!linearSaturationDone)
                {
                    colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, endSaturation, tLinear);
                    if (tLinear >= 1f) linearSaturationDone = true;
                }
                break;
        }
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
        linearPostExposureDone = false;
        linearContrastDone = false;
        linearColorFilterDone = false;
        linearHueShiftDone = false;
        linearSaturationDone = false;
    }
}
