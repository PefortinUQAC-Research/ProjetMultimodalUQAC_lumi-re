using UnityEngine;

public class SSQFormController : MonoBehaviour
{
    [Header("Références UI")]
    public GameObject formCanvas;      // ton Canvas_SSQFormulaire
    public GameObject[] pages;         // tableau de tes 4 pages

    private int currentPage = 0;

    void Start()
    {
        // Affiche le Canvas et la première page
        formCanvas.SetActive(true);
        ShowPage(0);
    }

    // Active uniquement la page d’indice index
    void ShowPage(int index)
    {
        currentPage = index;
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);
    }

    // Méthode à appeler sur les boutons “Next”
    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
            ShowPage(currentPage + 1);
    }

    // Méthode à appeler sur les boutons “Previous”
    public void PrevPage()
    {
        if (currentPage > 0)
            ShowPage(currentPage - 1);
    }

    // Méthode à appeler sur le bouton “Submit” (page 4)
    public void SendAndClose()
    {
        // Ici tu peux récupérer/vérifier les réponses si besoin
        formCanvas.SetActive(false);
    }
}
