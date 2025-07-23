using System.Collections.Generic;
using UnityEngine;

// Énumération pour les langues supportées
public enum Language
{
    French,
    English
}

// Entrée de traduction (FR + EN)
public class TranslationEntry
{
    public string french;
    public string english;
    
    public TranslationEntry(string fr, string en)
    {
        french  = fr;
        english = en;
    }
}

// Gestionnaire principal de localisation (Singleton)
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [SerializeField] private Language currentLanguage = Language.French;
    public System.Action<Language> OnLanguageChanged;

    // Dictionnaire codé en dur avec toutes les paires clé → (fr, en)
    private Dictionary<string, TranslationEntry> _translations = new Dictionary<string, TranslationEntry>()
    {
        { "general_discomfort",        new TranslationEntry("1. Gêne générale",                   "1. General discomfort") },
        { "fatigue",                   new TranslationEntry("2. Fatigue",                          "2. Fatigue") },
        { "headache",                  new TranslationEntry("3. Mal de tête",                      "3. Headache") },
        { "eye_strain",                new TranslationEntry("4. Fatigue oculaire",                 "4. Eye strain") },
        { "difficulty_focusing",       new TranslationEntry("5. Difficulté à faire la mise au point","5. Difficulty focusing") },
        { "salivation_increasing",     new TranslationEntry("6. Augmentation de la salivation",    "6. Salivation increasing") },
        { "sweating",                  new TranslationEntry("7. Transpiration",                     "7. Sweating") },
        { "nausea",                    new TranslationEntry("8. Nausée",                            "8. Nausea") },
        { "difficulty_concentrating",  new TranslationEntry("9. Difficulté à se concentrer",        "9. Difficulty concentrating") },
        { "fullness_of_head",          new TranslationEntry("10. Sensation de lourdeur dans la tête","10. Fullness of the Head") },
        { "blurred_vision",            new TranslationEntry("11. Vision floue",                     "11. Blurred vision") },
        { "dizziness_eyes_open",       new TranslationEntry("12. Étourdissements yeux ouverts",     "12. Dizziness with eyes open") },
        { "dizziness_eyes_closed",     new TranslationEntry("13. Étourdissements yeux fermés",      "13. Dizziness with eyes closed") },
        { "vertigo",                   new TranslationEntry("14. Vertige",                          "14. Vertigo") },
        { "stomach_awareness",         new TranslationEntry("15. Sensation d'estomac",              "15. Stomach awareness") },
        { "burping",                   new TranslationEntry("16. Rots",                             "16. Burping") },
        { "toggle_none",     new TranslationEntry("Aucun",     "None") },
        { "toggle_slight",   new TranslationEntry("Léger",     "Slight") },
        { "toggle_moderate", new TranslationEntry("Modéré",    "Moderate") },
        { "toggle_severe",   new TranslationEntry("Sévère",    "Severe") },
        { "next",     new TranslationEntry("Suivant",     "Next") },
        { "previous",   new TranslationEntry("Précédent",     "Previous") },
        { "submit",   new TranslationEntry("Sauvegarder",     "Submit") },
        { "language_toggle", new TranslationEntry("Français", "English") },
        { "form_title", new TranslationEntry("Questionnaire d'évaluation des symptômes","Symptom Assessment Questionnaire") }
    };

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Applique la langue initiale
        ApplyLocalization();
    }

    public Language CurrentLanguage
    {
        get => currentLanguage;
        set
        {
            if (currentLanguage == value) return;
            currentLanguage = value;
            ApplyLocalization();
            OnLanguageChanged?.Invoke(currentLanguage);
        }
    }

    // Retourne le texte localisé pour une clé
    public string GetLocalizedText(string key)
    {
        if (!_translations.TryGetValue(key, out var entry))
            return key; // clé introuvable → retourne la clé brute

        return currentLanguage == Language.French
            ? entry.french
            : entry.english;
    }

    // Inverse la langue et notifie
    public void ToggleLanguage()
    {
        CurrentLanguage = (currentLanguage == Language.French)
            ? Language.English
            : Language.French;
    }

    // Rafraîchit TOUS les composants LocalizedTextComponent de la scène
    private void ApplyLocalization()
    {
        foreach (var comp in FindObjectsOfType<LocalizedTextComponent>())
            comp.UpdateText();
    }
}
