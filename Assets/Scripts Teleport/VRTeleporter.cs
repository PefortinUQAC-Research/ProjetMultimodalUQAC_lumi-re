using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRTeleporter : MonoBehaviour
{
    [Header("Configuration")]
    public string targetSceneName = "BlackScene";
    public float teleportDelay = 0.5f; // Délai avant téléportation (optionnel)
    

    
    private bool hasTeleported = false; // Évite les téléportations multiples
    
    void OnTriggerEnter(Collider other)
    {
        // Vérifier si c'est bien une manette VR qui entre dans le trigger
        if (!hasTeleported && IsVRController(other))
        {
            Debug.Log("Manette VR détectée dans le cube - Téléportation en cours...");
            SavePlayerPosition();
            StartCoroutine(TeleportWithDelay());
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
        
        // Attendre le délai spécifié
        yield return new WaitForSeconds(teleportDelay);
        
        // Charger la nouvelle scène
        SceneManager.LoadScene(targetSceneName);
    }
    
    // Méthode pour réinitialiser si nécessaire
    public void ResetTeleporter()
    {
        hasTeleported = false;
    }
}