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
            StartCoroutine(TeleportWithDelay());
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