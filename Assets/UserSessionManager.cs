using UnityEngine;
using System;

/// <summary>
/// Gère les sessions utilisateur, y compris les vibrations, les types de murs et la durée des sessions.
/// </summary>
public class UserSessionManager : MonoBehaviour
{
    public static UserSessionManager Instance { get; private set; } // Instance unique (singleton)

    public string UserId { get; private set; } // Identifiant unique de l'utilisateur
    public string[] PossibleVibrations = { "Aucun retour", "Vibration aléatoire", "Vibration rythmique", "Vibration dépendante de la vitesse" }; // Types de vibrations possibles
    public string VibrationType { get; private set; } // Type de vibration sélectionné
    public MazeCell.WallType WallType = MazeCell.WallType.BlackAndWhite; // Type de mur sélectionné

    public DateTime StartTime { get; private set; } // Heure de début de la session
    public DateTime EndTime { get; private set; } // Heure de fin de la session
    public TimeSpan Duration { get; private set; } // Durée totale de la session

    public DateTime QuestionnaireStartTime { get; private set; } // Heure de début du questionnaire
    public TimeSpan QuestionnaireDuration { get; private set; } // Durée totale des questionnaires
    public bool QuestionnaireStarted { get; private set; } = false; // Indique si un questionnaire est en cours

    private void Awake()
    {
        // Assure qu'une seule instance existe
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre les scènes

        // Génère ou récupère un identifiant utilisateur unique
        if (PlayerPrefs.HasKey("UserID"))
        {
            UserId = PlayerPrefs.GetString("UserID");
        }
        else
        {
            UserId = Guid.NewGuid().ToString("N").Substring(0, 8);
            PlayerPrefs.SetString("UserID", UserId);
        }

        Debug.Log("UserSessionManager initialisé - User ID: " + UserId);
        StartSession();
    }

    /// <summary>
    /// Démarre une nouvelle session utilisateur.
    /// </summary>
    public void StartSession()
    {
        VibrationType = PossibleVibrations[0]; // Sélectionne un type de vibration aléatoire

        // Configure les retours haptiques pour tous les objets VRHaptics
        VRHaptics[] haptics = FindObjectsByType<VRHaptics>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var haptic in haptics)
        {
            haptic.SetHapticFeedback();
        }

        StartTime = DateTime.Now;
        Duration = TimeSpan.Zero;
    }

    /// <summary>
    /// Démarre un questionnaire.
    /// </summary>
    public void StartQuestionnaire()
    {
        if (QuestionnaireStarted)
        {
            Debug.LogWarning("Le questionnaire a déjà été commencé !");
            return;
        }

        QuestionnaireStartTime = DateTime.Now;
        QuestionnaireStarted = true;
        Time.timeScale = 0; // Met le jeu en pause
        Debug.Log("Questionnaire commencé");
    }

    /// <summary>
    /// Termine le questionnaire en cours.
    /// </summary>
    public void EndQuestionnaire()
    {
        QuestionnaireDuration += DateTime.Now - QuestionnaireStartTime;
        QuestionnaireStarted = false;
        Time.timeScale = 1; // Reprend le jeu
        Debug.Log($"Questionnaire terminé - Durée: {QuestionnaireDuration.TotalSeconds} secondes");
    }

    /// <summary>
    /// Termine la session utilisateur.
    /// </summary>
    public void EndSession()
    {
        // Retire le type de vibration utilisé de la liste
        PossibleVibrations = Array.FindAll(PossibleVibrations, v => v != VibrationType);

        EndTime = DateTime.Now;
        Duration = EndTime - StartTime - QuestionnaireDuration;
        QuestionnaireDuration = TimeSpan.Zero;

        if (PossibleVibrations.Length == 0) Application.Quit(); // Quitte l'application si tous les types de vibrations ont été utilisés

        Debug.Log($"Session terminée - Durée: {Duration.TotalSeconds} secondes sans les questionnaires");
    }
}