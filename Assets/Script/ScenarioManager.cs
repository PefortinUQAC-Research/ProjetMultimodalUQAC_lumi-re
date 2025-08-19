using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;

    public enum TempScenario { Neutral = 0, Warm = 1, Cold = 2 }

    [Header("White Balance Temperatures")]
    public float neutralTemperature = 0f;
    public float warmTemperature = 60f;
    public float coldTemperature = -60f;

    private Queue<TempScenario> queue = new Queue<TempScenario>();
    private TempScenario? current = null;
    private bool hasAppliedOnce = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        RefillQueueShuffled();

        // Si on démarre directement dans la BasicScene, applique tout de suite.
#if UNITY_6000_0_OR_NEWER
        var volume = FindFirstObjectByType<Volume>();
#else
        var volume = FindObjectOfType<Volume>();
#endif
        if (volume) EnsureApplied(volume);
    }

    private void RefillQueueShuffled()
    {
        var list = new List<TempScenario> { TempScenario.Neutral, TempScenario.Warm, TempScenario.Cold };
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
        queue.Clear();
        foreach (var s in list) queue.Enqueue(s);
    }

    private TempScenario Next()
    {
        if (queue.Count == 0) RefillQueueShuffled();
        var s = queue.Dequeue();
        current = s;
        return s;
    }

    public void EnsureApplied(Volume v)
    {
        if (!v || !v.profile) return;

        if (!hasAppliedOnce)
        {
            var s = current ?? Next();
            ApplyToVolume(v, s);
            hasAppliedOnce = true;
        }
        else if (current.HasValue)
        {
            ApplyToVolume(v, current.Value);
        }
    }

    public void AdvanceAndApply(Volume v)
    {
        if (!v || !v.profile) return;
        var s = Next();
        ApplyToVolume(v, s);
        hasAppliedOnce = true;
    }

    private void ApplyToVolume(Volume v, TempScenario s)
{
    if (!v.profile.TryGet<WhiteBalance>(out var wb))
    {
        Debug.LogWarning("[ScenarioManager] WhiteBalance manquant dans le Global Volume.");
        return;
    }

    // Activer l’override sur le paramètre (pas sur le composant)
    wb.active = true;                          // facultatif mais utile
    wb.temperature.overrideState = true;

    wb.temperature.value =
        s == TempScenario.Neutral ? neutralTemperature :
        s == TempScenario.Warm    ? warmTemperature    :
                                    coldTemperature;

    // (Optionnel) s’assurer que le tint ne dérive pas
    wb.tint.overrideState = true;
    wb.tint.value = 0f;

    Debug.Log($"[ScenarioManager] Scénario appliqué: {s}");
}

}
