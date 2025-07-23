using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageToggleButton : MonoBehaviour
{
    [SerializeField] private Button toggleButton;
    [SerializeField] private Text buttonText;
    [SerializeField] private TextMeshProUGUI buttonTMPText;

    private void Start()
    {
        if (toggleButton == null) toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(() => LocalizationManager.Instance.ToggleLanguage());
        UpdateButton();
        LocalizationManager.Instance.OnLanguageChanged += _ => UpdateButton();
    }

    private void UpdateButton()
    {
        string label = LocalizationManager.Instance.GetLocalizedText("language_toggle");
        if (buttonText != null) buttonText.text = label;
        else buttonTMPText.text = label;
    }
}
