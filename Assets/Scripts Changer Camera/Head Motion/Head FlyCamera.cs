using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    [Tooltip("Vitesse de déplacement normale")]
    public float mainSpeed = 10.0f;
    [Tooltip("Accélération quand Maj est maintenu")]
    public float shiftAdd = 25.0f;
    [Tooltip("Vitesse de rotation de la souris")]
    public float camSens = 0.2f;
    
    private Vector3 lastMouse = new Vector3(255, 255, 255);
    private float totalRun = 1.0f;

    void Update()
    {
        // Rotation avec bouton droit de la souris
        if (Input.GetMouseButton(1))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMouse;
            Vector3 rot = new Vector3(-mouseDelta.y * camSens, mouseDelta.x * camSens, 0);
            transform.eulerAngles += rot;
        }
        lastMouse = Input.mousePosition;

        // Mouvement avec WASD
        Vector3 input = new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0,
            Input.GetAxis("Vertical")
        );

        // Variation de vitesse si Maj est enfoncé
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            totalRun += Time.deltaTime;
            input *= totalRun * shiftAdd;
            input = Vector3.ClampMagnitude(input, mainSpeed * shiftAdd);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            input *= mainSpeed;
        }

        // Déplacement relativisé par le framerate
        input *= Time.deltaTime;
        transform.Translate(input);
    }
}
