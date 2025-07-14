using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DistanceChanger : MonoBehaviour
{
    [Header("Reference to the Global Volume")]
    public Volume volume;

    [Header("Activation")]
    public bool enableScript = true;

    public enum Mode { Linear, Cycle, Fixed }
    public Mode mode = Mode.Linear;

    [Header("Panini Projection Distance")]
    [Range(0f, 1f)] public float fixedDistance = 0f;
    [Range(0f, 1f)] public float minDistance = 0f;
    [Range(0f, 1f)] public float maxDistance = 0f;
    [Min(0f)] public float duration = 60f;

    private PaniniProjection paniniProjection;
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

        if (!volume.profile.TryGet(out paniniProjection))
        {
            Debug.LogError("PaniniProjection is not enabled in the Volume Profile.");
            enabled = false;
            return;
        }

        paniniProjection.active = true;
        paniniProjection.distance.overrideState = true;
    }

    void Update()
    {
        if (!enableScript || paniniProjection == null || transitionComplete) return;

        timer += Time.deltaTime;
        float t = 0f;
        float value = 0f;

        switch (mode)
        {
            case Mode.Linear:
                t = Mathf.Clamp01(timer / duration);
                paniniProjection.distance.value = Mathf.Lerp(minDistance, maxDistance, t);
                if (t >= 1f) transitionComplete = true;
                break;

            case Mode.Cycle:
                if (duration > 0f)
                {
                    t = Mathf.PingPong(timer, duration) / duration;
                    paniniProjection.distance.value = Mathf.Lerp(minDistance, maxDistance, t);
                }
                break;

            case Mode.Fixed:
                value = fixedDistance;
                break;
        }
    }
}