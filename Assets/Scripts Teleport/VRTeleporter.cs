using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRTeleporter : MonoBehaviour
{
    [Header("Configuration")]
    public string targetSceneName = "BlackScene";
    public float teleportDelay = 0.5f;
    
    private bool hasTeleported = false;
    
    void OnTriggerEnter(Collider other)
    {
        // Vérifier si c'est une manette VR avec le tag "Controller"
        if (!hasTeleported && other.CompareTag("Controller"))
        {
            Debug.Log($"Manette VR détectée: {other.name} - Téléportation en cours...");
            StartCoroutine(TeleportWithDelay());
        }
    }
    
    private IEnumerator TeleportWithDelay()
    {
        hasTeleported = true;
        
        yield return new WaitForSeconds(teleportDelay);
        
        SceneManager.LoadScene(targetSceneName);
    }
    
    public void ResetTeleporter()
    {
        hasTeleported = false;
    }
}