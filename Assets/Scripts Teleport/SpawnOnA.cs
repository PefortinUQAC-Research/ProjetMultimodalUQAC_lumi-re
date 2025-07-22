using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnCubeOnA : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference buttonAAction;

    [Header("Spawn Settings")]
    [Tooltip("Distance devant la caméra où le cube apparaîtra")]
    public float spawnDistance = 2f;
    [Tooltip("Taille du cube")]
    public Vector3 cubeSize = Vector3.one;

    private void OnEnable()
    {
        buttonAAction.action.Enable();
        buttonAAction.action.performed += OnButtonAPressed;
    }

    private void OnDisable()
    {
        buttonAAction.action.performed -= OnButtonAPressed;
        buttonAAction.action.Disable();
    }

    private void OnButtonAPressed(InputAction.CallbackContext context)
    {
        // Calcule la position devant la caméra principale
        Transform cam = Camera.main.transform;
        Vector3 spawnPos = cam.position + cam.forward * spawnDistance;

        // Crée un cube primitif
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = spawnPos;

        // Ajuste sa taille
        cube.transform.localScale = cubeSize;

        // (Optionnel) Ajoute un Rigidbody pour que le cube puisse interagir physiquement
        cube.AddComponent<Rigidbody>();
    }
}
