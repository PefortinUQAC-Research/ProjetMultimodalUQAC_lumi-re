using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LiftChanger : MonoBehaviour
{
    [Header("Reference to the Global Volume")]
    public Volume volume;

    [Header("Activation")]
    public bool enableScript = true;

    public enum Mode { Linear, Cycle, Fixed }
    public Mode mode = Mode.Linear;

    [Header("Lift Parameters (LiftGammaGain)")]
    [Range(-1f, 2f)] public float fixedLiftValue = 0f;
    [Range(-1f, 2f)] public float minLiftValue = 0f;
    [Range(-1f, 2f)] public float maxLiftValue = 0f;
    [Min(0f)] public float duration = 60f;

    private LiftGammaGain liftGammaGain;
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

        if (!volume.profile.TryGet(out liftGammaGain))
        {
            Debug.LogError("LiftGammaGain is not enabled in the Volume Profile.");
            enabled = false;
            return;
        }

        liftGammaGain.active = true;
        liftGammaGain.lift.overrideState = true;
    }

    void Update()
    {
        if (!enableScript || liftGammaGain == null || transitionComplete) return;

        timer += Time.deltaTime;
        Vector4 lift = liftGammaGain.lift.value;
        float t = 0f;
        float value = 0f;

        switch (mode)
        {
            case Mode.Linear:
                t = Mathf.Clamp01(timer / duration);
                lift.w = Mathf.Lerp(minLiftValue, maxLiftValue, t);
                if (t >= 1f) transitionComplete = true;
                break;

            case Mode.Cycle:
                if (duration > 0f)
                {
                    t = Mathf.PingPong(timer, duration) / duration;
                    lift.w = Mathf.Lerp(minLiftValue, maxLiftValue, t);
                }
                break;

            case Mode.Fixed:
                value = fixedLiftValue;
                break;
        }

        liftGammaGain.lift.value = lift;
    }
}
