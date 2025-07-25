using UnityEngine;
using UnityEngine.SceneManagement;

public class SubmitSceneTeleporter : MonoBehaviour
{
    [Header("Configuration")]
    public string targetSceneName = "MaSceneSuivante";
    public Vector3 targetPositionInNewScene = new Vector3(0, 1.5f, 0);

    // Appelée par le bouton Submit (via OnClick)
    public void TeleportToScene()
    {
        // Stocke la position dans une variable statique
        SceneTeleportData.nextPosition = targetPositionInNewScene;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(targetSceneName);
    }

    // Appelé automatiquement une fois la nouvelle scène chargée
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // On téléporte le XR Rig (taggé "Player") à la position choisie
        GameObject xrRig = GameObject.FindWithTag("Player");
        if (xrRig != null)
        {
            xrRig.transform.position = SceneTeleportData.nextPosition;
        }
        else
        {
            Debug.LogWarning("XR Rig non trouvé (tag 'Player' manquant dans la scène cible)");
        }
    }
}
