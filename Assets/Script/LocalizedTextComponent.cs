using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalizedTextComponent : MonoBehaviour
{
    [SerializeField] private string localizationKey;

    private Text uiText;
    private TextMeshProUGUI tmpText;

    private void Awake()
    {
        // Récupère soit un Text Unity UI, soit un TextMeshProUGUI
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TextMeshProUGUI>();

        if (uiText == null && tmpText == null)
            Debug.LogError($"{gameObject.name} requires a Text or TextMeshProUGUI component for localization.");
    }

    private void Start()
    {
        // Met à jour le texte au démarrage
        UpdateText();

        // S'abonne aux changements de langue
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDestroy()
    {
        // Se désabonne pour éviter les fuites mémoire
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(Language newLanguage)
    {
        UpdateText();
    }

    // Met à jour le contenu du composant UI avec la traduction
    public void UpdateText()
    {
        if (string.IsNullOrEmpty(localizationKey) || LocalizationManager.Instance == null)
            return;

        string localized = LocalizationManager.Instance.GetLocalizedText(localizationKey);

        if (uiText != null)
            uiText.text = localized;
        else if (tmpText != null)
            tmpText.text = localized;
    }

    // Permet de modifier la clé depuis le code
    public void SetLocalizationKey(string key)
    {
        localizationKey = key;
        UpdateText();
    }

    // Propriété pour lire/écrire la clé depuis l'Inspector ou le code
    public string LocalizationKey
    {
        get => localizationKey;
        set => SetLocalizationKey(value);
    }
}
