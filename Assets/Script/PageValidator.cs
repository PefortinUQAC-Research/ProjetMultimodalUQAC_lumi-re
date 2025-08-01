using UnityEngine;
using UnityEngine.UI;

public class PageValidator : MonoBehaviour
{
    [Header("Bouton Next")]
    public Button nextButton; // Bouton Next à assigner une seule fois

    private GameObject[] questionContainers;

    void Start()
    {
        // Trouve automatiquement tous les Q*_Container dans cette page
        questionContainers = FindContainers();

        // Désactive le bouton au début
        CheckQuestions();

        // Ajoute un listener à chaque Toggle trouvé
        foreach (var container in questionContainers)
        {
            var toggles = container.GetComponentsInChildren<Toggle>();
            foreach (var toggle in toggles)
            {
                toggle.onValueChanged.AddListener(delegate { CheckQuestions(); });
            }
        }
    }

    GameObject[] FindContainers()
    {
        // Cherche tous les enfants dont le nom contient "_Container"
        var containers = new System.Collections.Generic.List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.name.EndsWith("_Container"))
                containers.Add(child.gameObject);
        }

        return containers.ToArray();
    }

    void CheckQuestions()
    {
        // Vérifie si chaque container a au moins un toggle coché
        foreach (var container in questionContainers)
        {
            var toggles = container.GetComponentsInChildren<Toggle>();
            bool answered = false;

            foreach (var toggle in toggles)
            {
                if (toggle.isOn)
                {
                    answered = true;
                    break;
                }
            }

            if (!answered)
            {
                nextButton.interactable = false;
                return;
            }
        }

        // Si toutes les questions sont répondues
        nextButton.interactable = true;
    }
}
