using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;

public class SSQFormController : MonoBehaviour
{
    [Header("Références UI")]
    public GameObject formCanvas;
    public GameObject[] pages;

    [Header("Groupes de questions")]
    public ToggleGroup[] questionGroups; // Un ToggleGroup par question (16 au total)

    [Header("Sauvegarde")]
    public string fileName = "SSQForm_Default";

    private int currentPage = 0;

    void Start()
    {
        formCanvas.SetActive(true);
        ShowPage(0);
    }

    void ShowPage(int index)
    {
        currentPage = index;
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
            ShowPage(currentPage + 1);
    }

    public void PrevPage()
    {
        if (currentPage > 0)
            ShowPage(currentPage - 1);
    }

    public void SendAndClose()
    {
        Debug.Log("SendAndClose() appelé");

        Dictionary<string, string> results = new Dictionary<string, string>();

        for (int i = 0; i < questionGroups.Length; i++)
        {
            ToggleGroup group = questionGroups[i];
            Toggle activeToggle = GetActiveToggle(group);

            if (activeToggle != null)
            {
                string questionKey = "Q" + (i + 1);
                string selectedAnswer = activeToggle.name; // Le nom du toggle sélectionné
                results[questionKey] = selectedAnswer;
            }
            else
            {
                Debug.LogWarning("Aucune réponse sélectionnée pour la question Q" + (i + 1));
            }
        }

        // Récupérer le scénario courant et la date/heure
        string scenarioText = GetCurrentScenarioText();
        string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        
        // Construire le nom de fichier final
        string finalFileName = $"{fileName}_{scenarioText}_{dateTime}";

        string json = JsonUtility.ToJson(new QuestionDataWrapper(results), true);
        string path = Path.Combine(Application.persistentDataPath, finalFileName + ".json");
        File.WriteAllText(path, json);
        Debug.Log("Réponses enregistrées dans : " + path);

        formCanvas.SetActive(false);
    }

    private string GetCurrentScenarioText()
    {
        // Chercher le GameObject avec le tag "StepScenario"
        GameObject stepScenarioObject = GameObject.FindGameObjectWithTag("StepScenario");
        
        if (stepScenarioObject != null)
        {
            ScenarioManager scenarioManager = stepScenarioObject.GetComponent<ScenarioManager>();
            if (scenarioManager != null)
            {
                int currentScenario = scenarioManager.CurrentScenarioIndex;
                
                switch (currentScenario)
                {
                    case 0: return "Neutral";
                    case 1: return "Warm";
                    case 2: return "Cold";
                    default: return "Unknown";
                }
            }
            else
            {
                Debug.LogWarning("ScenarioManager non trouvé sur le GameObject avec le tag 'StepScenario'");
            }
        }
        else
        {
            Debug.LogWarning("Aucun GameObject trouvé avec le tag 'StepScenario'");
        }
        
        return "NoScenario";
    }

    Toggle GetActiveToggle(ToggleGroup group)
    {
        foreach (Toggle t in group.GetComponentsInChildren<Toggle>())
        {
            if (t.isOn)
                return t;
        }
        return null;
    }

    [System.Serializable]
    public class QuestionDataWrapper
    {
        public List<string> questions = new List<string>();
        public List<string> answers = new List<string>();

        public QuestionDataWrapper(Dictionary<string, string> dict)
        {
            foreach (var kv in dict)
            {
                questions.Add(kv.Key);
                answers.Add(kv.Value);
            }
        }
    }
}