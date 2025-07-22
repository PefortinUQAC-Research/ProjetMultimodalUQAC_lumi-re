using System.IO;
using UnityEngine;

public class v2SSQFormController : MonoBehaviour
{
    public void SendAndClose()
    {
        string path = Path.Combine(Application.persistentDataPath, "test.json");
        File.WriteAllText(path, "{\"message\":\"hello world\"}");
        Debug.Log($"Fichier JSON écrit à : {path}");
    }
}
