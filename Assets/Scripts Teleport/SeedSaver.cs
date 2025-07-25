using UnityEngine;

/// <summary>
/// Composant pour sauvegarder et charger la seed du labyrinthe.
/// Ce script doit être attaché à un GameObject avec le tag "SeedSaver" (ou le tag configuré dans MazeGenerator).
/// </summary>
public class SeedSaver : MonoBehaviour
{
    [SerializeField]
    private int _savedSeed = 0; // Seed sauvegardée (0 = aucune seed sauvegardée)

    [SerializeField]
    private Vector3 _savedCameraPosition = Vector3.zero; // Position de la caméra sauvegardée

    [SerializeField]
    private Quaternion _savedCameraRotation = Quaternion.identity; // Rotation de la caméra sauvegardée

    [SerializeField]
    private bool _persistBetweenScenes = true; // Si true, l'objet persiste entre les scènes

    void Awake()
    {
        if (_persistBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Sauvegarde une nouvelle seed.
    /// </summary>
    public void SaveSeed(int seed)
    {
        _savedSeed = seed;
    }

    /// <summary>
    /// Récupère la seed sauvegardée.
    /// </summary>
    /// <returns>La seed sauvegardée, ou 0 si aucune seed n'est sauvegardée.</returns>
    public int GetSavedSeed()
    {
        return _savedSeed;
    }

    /// <summary>
    /// Sauvegarde la position de la caméra.
    /// </summary>
    public void SaveCameraPosition(Vector3 position)
    {
        _savedCameraPosition = position;
        Debug.Log($"Position de la caméra sauvegardée: {position}");
    }

    /// <summary>
    /// Sauvegarde la rotation de la caméra.
    /// </summary>
    public void SaveCameraRotation(Quaternion rotation)
    {
        _savedCameraRotation = rotation;
        Debug.Log($"Rotation de la caméra sauvegardée: {rotation.eulerAngles}");
    }

    /// <summary>
    /// Sauvegarde la position et la rotation de la caméra en une seule fois.
    /// </summary>
    public void SaveCameraTransform(Vector3 position, Quaternion rotation)
    {
        SaveCameraPosition(position);
        SaveCameraRotation(rotation);
    }

    /// <summary>
    /// Récupère la position de la caméra sauvegardée.
    /// </summary>
    /// <returns>La position de la caméra sauvegardée.</returns>
    public Vector3 GetSavedCameraPosition()
    {
        return _savedCameraPosition;
    }

    /// <summary>
    /// Récupère la rotation de la caméra sauvegardée.
    /// </summary>
    /// <returns>La rotation de la caméra sauvegardée.</returns>
    public Quaternion GetSavedCameraRotation()
    {
        return _savedCameraRotation;
    }

    /// <summary>
    /// Vérifie s'il y a une position de caméra sauvegardée.
    /// </summary>
    /// <returns>True s'il y a une position sauvegardée (différente de Vector3.zero), false sinon.</returns>
    public bool HasSavedCameraPosition()
    {
        return _savedCameraPosition != Vector3.zero;
    }

    /// <summary>
    /// Vérifie s'il y a une rotation de caméra sauvegardée.
    /// </summary>
    /// <returns>True s'il y a une rotation sauvegardée (différente de Quaternion.identity), false sinon.</returns>
    public bool HasSavedCameraRotation()
    {
        return _savedCameraRotation != Quaternion.identity;
    }

    /// <summary>
    /// Efface la seed sauvegardée.
    /// </summary>
    public void ClearSeed()
    {
        _savedSeed = 0;
    }

    /// <summary>
    /// Efface la position de la caméra sauvegardée.
    /// </summary>
    public void ClearCameraPosition()
    {
        _savedCameraPosition = Vector3.zero;
    }

    /// <summary>
    /// Efface la rotation de la caméra sauvegardée.
    /// </summary>
    public void ClearCameraRotation()
    {
        _savedCameraRotation = Quaternion.identity;
    }

    /// <summary>
    /// Efface toutes les données sauvegardées.
    /// </summary>
    public void ClearAllData()
    {
        ClearSeed();
        ClearCameraPosition();
        ClearCameraRotation();
    }

    /// <summary>
    /// Vérifie s'il y a une seed sauvegardée.
    /// </summary>
    /// <returns>True s'il y a une seed sauvegardée, false sinon.</returns>
    public bool HasSavedSeed()
    {
        return _savedSeed != 0;
    }
}