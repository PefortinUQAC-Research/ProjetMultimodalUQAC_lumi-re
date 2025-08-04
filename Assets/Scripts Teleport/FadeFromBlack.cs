using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FadeFromBlack : MonoBehaviour
{
    public float fadeDuration = 1.0f;
    private Volume globalVolume;
    private ColorAdjustments colorAdjustments;

    private void Start()
    {
        globalVolume = FindFirstObjectByType<Volume>();
        if (globalVolume == null)
        {
            Debug.LogWarning("Global Volume introuvable dans la scène.");
            return;
        }

        if (!globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            Debug.LogWarning("ColorAdjustments introuvable dans le profil du Volume.");
            return;
        }

        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.colorFilter.value = Color.black;

        StartCoroutine(FadeToWhite());
    }

    private IEnumerator FadeToWhite()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerpValue = t / fadeDuration;
            colorAdjustments.colorFilter.value = new Color(lerpValue, lerpValue, lerpValue, 1f);
            yield return null;
        }

        colorAdjustments.colorFilter.value = Color.white;
        colorAdjustments.colorFilter.overrideState = false;
    }
}
