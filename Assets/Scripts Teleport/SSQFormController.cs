using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class SSQFormController : MonoBehaviour
{
    [Header("Références UI")]
    public GameObject formCanvas;
    public GameObject[] pages;

    [Header("Groupes de questions")]
    public ToggleGroup[] questionGroups; // Un ToggleGroup par question (16 au total)

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

        string json = JsonUtility.ToJson(new QuestionDataWrapper(results), true);
        string path = Path.Combine(Application.persistentDataPath, "ssq_formulaire.json");
        File.WriteAllText(path, json);
        Debug.Log("Réponses enregistrées dans : " + path);

        formCanvas.SetActive(false);
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
