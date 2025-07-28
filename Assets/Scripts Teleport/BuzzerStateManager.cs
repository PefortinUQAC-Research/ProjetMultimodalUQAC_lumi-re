using UnityEngine;
using UnityEngine.SceneManagement;

public class BuzzerStateManager : MonoBehaviour
{
    [Header("État des Buzzers")]
    public bool BuzzerStart = true;
    public bool BuzzerMid = true;
    public bool BuzzerEnd = true;
    
    private void Awake()
    {
        // Empêcher la destruction de cet objet entre les scènes
        DontDestroyOnLoad(gameObject);
        
        // S'assurer qu'il n'y a qu'une seule instance
        if (FindObjectsByType<BuzzerStateManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        // S'abonner à l'événement de chargement de scène
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Se désabonner de l'événement pour éviter les erreurs
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Appliquer l'état des buzzers quand une scène est chargée
        ApplyBuzzerStates();
    }
    
    private void ApplyBuzzerStates()
    {
        // Gérer BuzzerStart
        GameObject buzzerStart = GameObject.FindWithTag("BuzzerStart");
        if (buzzerStart != null)
        {
            buzzerStart.SetActive(BuzzerStart);
            Debug.Log($"BuzzerStart défini à: {BuzzerStart}");
        }
        
        // Gérer BuzzerMid
        GameObject buzzerMid = GameObject.FindWithTag("BuzzerMid");
        if (buzzerMid != null)
        {
            buzzerMid.SetActive(BuzzerMid);
            Debug.Log($"BuzzerMid défini à: {BuzzerMid}");
        }
        
        // Gérer BuzzerEnd
        GameObject buzzerEnd = GameObject.FindWithTag("BuzzerEnd");
        if (buzzerEnd != null)
        {
            buzzerEnd.SetActive(BuzzerEnd);
            Debug.Log($"BuzzerEnd défini à: {BuzzerEnd}");
        }
    }
    
    // Méthode pour désactiver un buzzer spécifique
    public void DisableBuzzer(string buzzerTag)
    {
        switch (buzzerTag)
        {
            case "BuzzerStart":
                BuzzerStart = false;
                Debug.Log("BuzzerStart désactivé dans le manager");
                break;
            case "BuzzerMid":
                BuzzerMid = false;
                Debug.Log("BuzzerMid désactivé dans le manager");
                break;
            case "BuzzerEnd":
                BuzzerEnd = false;
                Debug.Log("BuzzerEnd désactivé dans le manager");
                break;
            default:
                Debug.LogWarning($"Tag de buzzer non reconnu: {buzzerTag}");
                break;
        }
    }
    
    // Méthodes pour réactiver des buzzers si nécessaire
    public void EnableBuzzer(string buzzerTag)
    {
        switch (buzzerTag)
        {
            case "BuzzerStart":
                BuzzerStart = true;
                break;
            case "BuzzerMid":
                BuzzerMid = true;
                break;
            case "BuzzerEnd":
                BuzzerEnd = true;
                break;
        }
        ApplyBuzzerStates(); // Appliquer immédiatement les changements
    }
    
    // Méthode pour réinitialiser tous les buzzers
    public void ResetAllBuzzers()
    {
        BuzzerStart = true;
        BuzzerMid = true;
        BuzzerEnd = true;
        ApplyBuzzerStates();
        Debug.Log("Tous les buzzers ont été réinitialisés");
    }
}