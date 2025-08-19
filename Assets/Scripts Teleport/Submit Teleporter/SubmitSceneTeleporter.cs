using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;


public class SubmitSceneTeleporter : MonoBehaviour
{
    [Header("Configuration")]
    public string targetSceneName = "BasicScene";
    public Vector3 fallbackPosition = new Vector3(0, 1.5f, 0); // Position de secours si aucune position n'est sauvegardée
    
    [Header("Dernier Formulaire")]
    [Tooltip("Cochez cette case si c'est le dernier formulaire. Cela va reset la seed et téléporter au début du labyrinthe.")]
    public bool isLastForm = false;
    public Vector3 mazeStartPosition = new Vector3(0, 1.5f, 0); // Position de début du labyrinthe

    [Header("Fondu")]
    public Volume globalVolume;
    public float fadeDuration = 1.0f;

    private IEnumerator FadeAndTeleport()
    {
        if (globalVolume != null && globalVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
        {
            colorAdjustments.colorFilter.overrideState = true;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float lerp = t / fadeDuration;
                colorAdjustments.colorFilter.value = new Color(1f - lerp, 1f - lerp, 1f - lerp, 1f);
                yield return null;
            }

            colorAdjustments.colorFilter.value = Color.black;
        }

        // Si c'est le dernier formulaire, reset la seed avant de charger la scène
        if (isLastForm)
        {
            ResetSeed();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(targetSceneName);
    }

    // Appelée par le bouton Submit (via OnClick)
    public void TeleportToScene()
    {
        StartCoroutine(FadeAndTeleport());
    }

    // Appelé automatiquement une fois la nouvelle scène chargée
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    SceneManager.sceneLoaded -= OnSceneLoaded;

    GameObject xrOrigin = GameObject.FindWithTag("Player");

    if (xrOrigin != null)
    {
        Vector3 targetPosition;

        if (isLastForm)
        {
            // TP au début du labyrinthe (position définie)
            targetPosition = mazeStartPosition;
            xrOrigin.transform.position = targetPosition;

            // Reset la rotation
            Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
            if (cameraOffset != null)
                cameraOffset.rotation = Quaternion.identity;

            // *** Remettre les buzzers ***
            var stepBuzzers = GameObject.FindGameObjectsWithTag("StepBuzzer");
            foreach (var go in stepBuzzers)
            {
                if (!go.activeSelf) go.SetActive(true);

                var state = go.GetComponent<BuzzerStateManager>();
                if (state != null)
                {
                    state.BuzzerStart = true;
                    state.BuzzerMid   = true;
                    state.BuzzerEnd   = true;
                }
            }
            Debug.Log($"Dernier formulaire terminé - XR Origin au début du labyrinthe: {targetPosition}. " +
                      $"Buzzers réinitialisés sur {stepBuzzers.Length} objet(s) tag 'StepBuzzer'.");

            // === Appliquer le scénario suivant (on avance la rotation) ===
#if UNITY_6000_0_OR_NEWER
            var volume = FindFirstObjectByType<Volume>();
#else
            var volume = FindObjectOfType<Volume>();
#endif
            if (volume && ScenarioManager.Instance != null)
            {
                ScenarioManager.Instance.AdvanceAndApply(volume);
            }
        }
        else
        {
            // Comportement normal : utilise la position/rotation sauvegardées
            targetPosition = GetSavedPosition();
            xrOrigin.transform.position = targetPosition;

            Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
            if (cameraOffset != null)
            {
                Quaternion targetRotation = GetSavedRotation();
                cameraOffset.rotation = targetRotation;
            }

            Debug.Log($"XR Origin téléporté à la position sauvegardée: {targetPosition}");

            // === Première arrivée dans le Maze ou retour non-final : ne PAS avancer, juste s'assurer que le filtre courant est appliqué ===
#if UNITY_6000_0_OR_NEWER
            var volume = FindFirstObjectByType<Volume>();
#else
            var volume = FindObjectOfType<Volume>();
#endif
            if (volume && ScenarioManager.Instance != null)
            {
                ScenarioManager.Instance.EnsureApplied(volume);
            }
        }
    }
    else
    {
        Debug.LogWarning("XR Origin non trouvé (tag 'Player' manquant dans la scène cible)");
    }
}



    /// <summary>
    /// Reset la seed pour générer un nouveau labyrinthe
    /// </summary>
    private void ResetSeed()
    {
        GameObject seedSaverObject = GameObject.FindWithTag("SeedSaver");
        
        if (seedSaverObject != null)
        {
            SeedSaver seedSaver = seedSaverObject.GetComponent<SeedSaver>();
            
            if (seedSaver != null)
            {
                // Reset les données du SeedSaver
                ResetSeedSaverData(seedSaver);
                Debug.Log("Seed réinitialisée pour générer un nouveau labyrinthe");
            }
            else
            {
                Debug.LogWarning("Composant SeedSaver non trouvé sur l'objet SeedSaver");
            }
        }
        else
        {
            Debug.LogWarning("GameObject avec tag 'SeedSaver' non trouvé pour reset la seed");
        }
    }

    /// <summary>
    /// Régénère le maze et réactive les buzzers après avoir terminé le dernier formulaire
    /// </summary>
    private void RegenerateMazeAndBuzzers()
    {
        // 1. Régénérer le maze
        RegenerateMaze();
        
        // 2. Réactiver tous les buzzers
        ReactivateBuzzers();
    }

    /// <summary>
    /// Régénère le maze avec une nouvelle seed
    /// </summary>
    private void RegenerateMaze()
    {
        // Chercher le générateur de maze (adaptez selon votre implémentation)
        // Exemples de tags/noms possibles pour votre générateur de maze :
        GameObject mazeGenerator = GameObject.FindWithTag("MazeGenerator");
        
        if (mazeGenerator == null)
        {
            // Essayer d'autres noms/tags possibles
            mazeGenerator = GameObject.Find("MazeGenerator");
            if (mazeGenerator == null)
            {
                mazeGenerator = GameObject.Find("Maze Generator");
            }
        }
        
        if (mazeGenerator != null)
        {
            // Option 1: Si votre générateur a une méthode publique pour régénérer
            var generator = mazeGenerator.GetComponent<MonoBehaviour>();
            if (generator != null)
            {
                // Essayer d'appeler une méthode de régénération (adaptez selon votre code)
                System.Reflection.MethodInfo regenerateMethod = generator.GetType().GetMethod("GenerateNewMaze");
                if (regenerateMethod == null)
                {
                    regenerateMethod = generator.GetType().GetMethod("RegenerateMaze");
                }
                if (regenerateMethod == null)
                {
                    regenerateMethod = generator.GetType().GetMethod("GenerateMaze");
                }
                
                if (regenerateMethod != null)
                {
                    regenerateMethod.Invoke(generator, null);
                    Debug.Log("Nouveau maze généré avec succès");
                }
                else
                {
                    Debug.LogWarning("Aucune méthode de génération de maze trouvée. Veuillez adapter le code selon votre implémentation.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Générateur de maze non trouvé. Vérifiez le tag ou le nom de votre générateur de maze.");
        }
    }

    /// <summary>
    /// Réactive tous les buzzers dans la scène
    /// </summary>
    private void ReactivateBuzzers()
    {
        // Chercher tous les buzzers (adaptez selon votre implémentation)
        GameObject[] buzzers = GameObject.FindGameObjectsWithTag("Buzzer");
        
        if (buzzers.Length == 0)
        {
            // Essayer d'autres méthodes si le tag "Buzzer" n'existe pas
            buzzers = GameObject.FindObjectsOfType<GameObject>()
                .Where(go => go.name.ToLower().Contains("buzzer"))
                .ToArray();
        }
        
        if (buzzers.Length > 0)
        {
            foreach (GameObject buzzer in buzzers)
            {
                // Réactiver le GameObject
                buzzer.SetActive(true);
                
                // Si les buzzers ont un composant spécifique pour les réactiver
                var buzzerComponent = buzzer.GetComponent<MonoBehaviour>();
                if (buzzerComponent != null)
                {
                    // Essayer d'appeler une méthode de réactivation si elle existe
                    System.Reflection.MethodInfo resetMethod = buzzerComponent.GetType().GetMethod("ResetBuzzer");
                    if (resetMethod == null)
                    {
                        resetMethod = buzzerComponent.GetType().GetMethod("Reactivate");
                    }
                    if (resetMethod == null)
                    {
                        resetMethod = buzzerComponent.GetType().GetMethod("Reset");
                    }
                    
                    if (resetMethod != null)
                    {
                        resetMethod.Invoke(buzzerComponent, null);
                    }
                }
                
                Debug.Log($"Buzzer réactivé: {buzzer.name}");
            }
            
            Debug.Log($"{buzzers.Length} buzzers réactivés avec succès");
        }
        else
        {
            Debug.LogWarning("Aucun buzzer trouvé dans la scène. Vérifiez les tags ou noms de vos buzzers.");
        }
    }

    private void ResetSeedSaverData(SeedSaver seedSaver)
    {
        seedSaver.ClearAllData();
    }

    private IEnumerator WaitAndRespawnBuzzers()
    {
        // Laisser 1–2 frames pour que les objets/manager existent
        yield return null;
        yield return null;

        var mgr = Object.FindObjectOfType<BuzzerStateManager>();
        if (mgr != null)
        {
            // Remet les trois à true et applique immédiatement
            mgr.ResetAllBuzzers();
            Debug.Log("Buzzers réinitialisés via BuzzerStateManager.");
        }
        else
        {
            Debug.LogWarning("Aucun BuzzerStateManager trouvé dans la scène pour réactiver les buzzers.");
        }
    }


    /// <summary>
    /// Récupère la position sauvegardée depuis le SeedSaver, ou utilise la position de secours.
    /// </summary>
    /// <returns>La position où téléporter le joueur.</returns>
    private Vector3 GetSavedPosition()
    {
        // Cherche le GameObject avec le tag "SeedSaver"
        GameObject seedSaverObject = GameObject.FindWithTag("SeedSaver");
        
        if (seedSaverObject != null)
        {
            SeedSaver seedSaver = seedSaverObject.GetComponent<SeedSaver>();
            
            if (seedSaver != null && seedSaver.HasSavedCameraPosition())
            {
                Vector3 savedPosition = seedSaver.GetSavedCameraPosition();
                Debug.Log($"Position récupérée depuis SeedSaver: {savedPosition}");
                return savedPosition;
            }
            else
            {
                Debug.LogWarning("SeedSaver trouvé mais aucune position de caméra sauvegardée ou composant SeedSaver manquant");
            }
        }
        else
        {
            Debug.LogWarning("Aucun GameObject avec le tag 'SeedSaver' trouvé, utilisation de la position de secours");
        }
        
        // Utilise la position de secours si aucune position sauvegardée n'est trouvée
        Debug.Log($"Utilisation de la position de secours: {fallbackPosition}");
        return fallbackPosition;
    }

    /// <summary>
    /// Récupère la rotation sauvegardée depuis le SeedSaver, ou utilise la rotation par défaut.
    /// </summary>
    /// <returns>La rotation où orienter la caméra du joueur.</returns>
    private Quaternion GetSavedRotation()
    {
        // Cherche le GameObject avec le tag "SeedSaver"
        GameObject seedSaverObject = GameObject.FindWithTag("SeedSaver");
        
        if (seedSaverObject != null)
        {
            SeedSaver seedSaver = seedSaverObject.GetComponent<SeedSaver>();
            
            if (seedSaver != null && seedSaver.HasSavedCameraRotation())
            {
                Quaternion savedRotation = seedSaver.GetSavedCameraRotation();
                Debug.Log($"Rotation récupérée depuis SeedSaver: {savedRotation.eulerAngles}");
                return savedRotation;
            }
            else
            {
                Debug.LogWarning("SeedSaver trouvé mais aucune rotation de caméra sauvegardée");
            }
        }
        else
        {
            Debug.LogWarning("Aucun GameObject avec le tag 'SeedSaver' trouvé pour la rotation");
        }
        
        // Utilise la rotation par défaut si aucune rotation sauvegardée n'est trouvée
        Debug.Log("Utilisation de la rotation par défaut (identity)");
        return Quaternion.identity;
    }
}