using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class VRTeleporter : MonoBehaviour
{
    [Header("Configuration")]
    public string targetSceneName = "BlackScene";
    public float teleportDelay = 0.5f; // Délai avant téléportation (optionnel)

    [Header("Fondu")]
    public Volume globalVolume;
    public float fadeDuration = 1.0f;

    

    
    private bool hasTeleported = false; // Évite les téléportations multiples
    
    void OnTriggerEnter(Collider other)
    {
        // Vérifier si c'est bien une manette VR qui entre dans le trigger
        if (!hasTeleported && IsVRController(other))
        {
            Debug.Log("Manette VR détectée dans le cube - Téléportation en cours...");
            SavePlayerPosition();
            DisableBuzzerInManager(); // Nouvelle fonction pour désactiver le buzzer
            StartCoroutine(TeleportWithDelay());
        }
    }
    
    private void DisableBuzzerInManager()
    {
        // Trouver le BuzzerStateManager
        GameObject buzzerManager = GameObject.FindWithTag("StepBuzzer");
        if (buzzerManager != null)
        {
            BuzzerStateManager stateManager = buzzerManager.GetComponent<BuzzerStateManager>();
            if (stateManager != null)
            {
                // Récupérer le tag de ce buzzer et le désactiver dans le manager
                string buzzerTag = gameObject.tag;
                stateManager.DisableBuzzer(buzzerTag);
                Debug.Log($"Buzzer avec le tag '{buzzerTag}' désactivé dans le manager");
            }
            else
            {
                Debug.LogWarning("Composant BuzzerStateManager non trouvé sur l'objet avec le tag 'StepBuzzer'");
            }
        }
        else
        {
            Debug.LogWarning("Aucun GameObject avec le tag 'StepBuzzer' trouvé");
        }
    }
    
    private void SavePlayerPosition()
    {
        // Chercher le XR Origin pour la position et le Camera Offset pour la rotation
        GameObject xrOrigin = GameObject.FindWithTag("Player");
        
        if (xrOrigin != null)
        {
            // Position du XR Origin
            Vector3 xrOriginPosition = xrOrigin.transform.position;
            
            // Chercher le Camera Offset dans les enfants du XR Origin
            Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
            
            if (cameraOffset != null)
            {
                // Rotation du Camera Offset (orientation du joueur)
                Quaternion cameraOffsetRotation = cameraOffset.rotation;
                
                // Sauvegarder dans le SeedSaver
                GameObject seedSaverObject = GameObject.FindWithTag("SeedSaver");
                if (seedSaverObject != null)
                {
                    SeedSaver seedSaver = seedSaverObject.GetComponent<SeedSaver>();
                    if (seedSaver != null)
                    {
                        seedSaver.SaveCameraTransform(xrOriginPosition, cameraOffsetRotation);
                        Debug.Log($"Position du XR Origin sauvegardée: {xrOriginPosition}");
                        Debug.Log($"Rotation du Camera Offset sauvegardée: {cameraOffsetRotation.eulerAngles}");
                    }
                    else
                    {
                        Debug.LogWarning("Composant SeedSaver non trouvé sur l'objet avec le tag 'SeedSaver'");
                    }
                }
                else
                {
                    Debug.LogWarning("Aucun GameObject avec le tag 'SeedSaver' trouvé");
                }
            }
            else
            {
                Debug.LogWarning("Camera Offset non trouvé dans les enfants du XR Origin");
            }
        }
        else
        {
            Debug.LogWarning("XR Origin non trouvé (tag 'Player' manquant)");
        }
    }
    
    private bool IsVRController(Collider collider)
    {
        // Méthode 1 : Vérifier par tag
        if (collider.CompareTag("Controller") || collider.CompareTag("Hand"))
        {
            return true;
        }
        
        // Méthode 2 : Vérifier par nom d'objet (adapte selon ton setup VR)
        string objName = collider.name.ToLower();
        if (objName.Contains("controller") || objName.Contains("hand") || 
            objName.Contains("left") || objName.Contains("right"))
        {
            return true;
        }
        
        // Méthode 3 : Vérifier si l'objet parent contient des composants VR
        Transform parent = collider.transform;
        while (parent != null)
        {
            if (parent.name.ToLower().Contains("controller") || 
                parent.name.ToLower().Contains("hand") ||
                parent.name.ToLower().Contains("xr"))
            {
                return true;
            }
            parent = parent.parent;
        }
        
        return false;
    }
    
    private IEnumerator TeleportWithDelay()
    {
        hasTeleported = true;

        if (globalVolume != null && globalVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
        {
            colorAdjustments.colorFilter.overrideState = true;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float lerpValue = 1f - (t / fadeDuration);  // de 1 à 0

                // Couleur du filtre : blanc à noir en RGB
                colorAdjustments.colorFilter.value = new Color(lerpValue, lerpValue, lerpValue, 1f);
                yield return null;
            }

            // Assure noir complet à la fin
            colorAdjustments.colorFilter.value = Color.black;
        }

        yield return new WaitForSeconds(teleportDelay);

        SceneManager.LoadScene(targetSceneName);
    }
    
    // Méthode pour réinitialiser si nécessaire
    public void ResetTeleporter()
    {
        hasTeleported = false;
    }

    private void Start()
    {
        if (globalVolume == null)
        {
            globalVolume = FindFirstObjectByType<Volume>();
            if (globalVolume == null)
            {
                Debug.LogWarning("Aucun Global Volume trouvé dans la scène.");
            } else {
                Debug.Log("Global Volume trouvé dans la scène.");
            }
        }
    }

}