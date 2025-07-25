using UnityEngine;
using UnityEngine.SceneManagement;

public class SubmitSceneTeleporter : MonoBehaviour
{
    [Header("Configuration")]
    public string targetSceneName = "MaSceneSuivante";
    public Vector3 fallbackPosition = new Vector3(0, 1.5f, 0); // Position de secours si aucune position n'est sauvegardée

    // Appelée par le bouton Submit (via OnClick)
    public void TeleportToScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(targetSceneName);
    }

    // Appelé automatiquement une fois la nouvelle scène chargée
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // On téléporte le XR Origin (taggé "Player") à la position sauvegardée
        GameObject xrOrigin = GameObject.FindWithTag("Player");
        
        if (xrOrigin != null)
        {
            Vector3 targetPosition = GetSavedPosition();
            xrOrigin.transform.position = targetPosition;
            Debug.Log($"XR Origin téléporté à la position: {targetPosition}");
            
            // Restaurer aussi la rotation du Camera Offset si disponible
            Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
            if (cameraOffset != null)
            {
                Quaternion targetRotation = GetSavedRotation();
                cameraOffset.rotation = targetRotation;
                Debug.Log($"Camera Offset rotation restaurée: {targetRotation.eulerAngles}");
            }
            else
            {
                Debug.LogWarning("Camera Offset non trouvé pour restaurer la rotation");
            }
        }
        else
        {
            Debug.LogWarning("XR Origin non trouvé (tag 'Player' manquant dans la scène cible)");
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