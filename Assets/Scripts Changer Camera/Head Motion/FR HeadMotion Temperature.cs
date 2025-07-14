using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FR_HeadMotion_Temperature : MonoBehaviour
{
    [Header("Référence au Volume Globale")]
    public Volume volume;
    
    [Header("Référence à la Caméra VR")]
    public Camera vrCamera;
    
    [Header("Contrôle du Script")]
    public bool activerScript = true;
    
    [Header("Temperature Settings")]
    [Range(-100f, 100f)] public float températureFroide = -20f;
    [Range(-100f, 100f)] public float températureChaude = 50f;
    [Range(0.1f, 10f)] public float vitesseChauffe = 3f;
    [Range(0.1f, 10f)] public float vitesseRefroidissement = 1f;
    
    [Header("Détection du Mouvement")]
    [Range(0f, 1f)] public float seuilMouvement = 0.1f;
    [Range(1f, 50f)] public float multiplicateurMouvement = 10f;
    [Range(0.1f, 2f)] public float intervalleEchantillonnage = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private WhiteBalance whiteBalance;
    private Vector3 lastHeadPosition;
    private Quaternion lastHeadRotation;
    private float currentTemperature;
    private float targetTemperature;
    private float currentMovementIntensity = 0f;
    private float lastSampleTime;
    
    void Start()
    {
        InitializeComponents();
        
        if (vrCamera != null)
        {
            lastHeadPosition = vrCamera.transform.position;
            lastHeadRotation = vrCamera.transform.rotation;
        }
        
        currentTemperature = températureFroide;
        targetTemperature = températureFroide;
        lastSampleTime = Time.time;
    }
    
    void InitializeComponents()
    {
        // Find VR Camera if not assigned
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
            if (vrCamera == null)
            {
                vrCamera = FindFirstObjectByType<Camera>();
                if (vrCamera == null)
                {
                    Debug.LogError("No VR Camera found! Please assign a VR Camera.");
                    enabled = false;
                    return;
                }
            }
        }
        
        // Find Global Volume if not assigned
        if (volume == null)
        {
            volume = FindFirstObjectByType<Volume>();
            if (volume == null)
            {
                Debug.LogError("No Global Volume found! Please assign a Global Volume.");
                enabled = false;
                return;
            }
        }
        
        // Get WhiteBalance component
        if (!volume.profile.TryGet(out whiteBalance))
        {
            Debug.LogError("WhiteBalance is not enabled in the Volume Profile.");
            enabled = false;
            return;
        }
        
        // Enable WhiteBalance
        whiteBalance.active = true;
        whiteBalance.temperature.overrideState = true;
    }
    
    void Update()
    {
        if (!activerScript || whiteBalance == null || vrCamera == null)
            return;
        
        // Sample movement at specified rate
        if (Time.time - lastSampleTime >= intervalleEchantillonnage)
        {
            SampleMovement();
            lastSampleTime = Time.time;
        }
        
        UpdateTemperature();
        ApplyTemperature();
        
        if (showDebugInfo)
            ShowDebugInfo();
    }
    
    void SampleMovement()
    {
        Vector3 currentHeadPosition = vrCamera.transform.position;
        Quaternion currentHeadRotation = vrCamera.transform.rotation;
        
        // Calculate position movement
        float positionMovement = Vector3.Distance(currentHeadPosition, lastHeadPosition);
        
        // Calculate rotation movement (convert to degrees and normalize)
        float rotationMovement = Quaternion.Angle(currentHeadRotation, lastHeadRotation) / 180f;
        
        // Combine position and rotation movement
        float totalMovement = positionMovement + rotationMovement;
        
        // Apply threshold to filter out small movements
        if (totalMovement > seuilMouvement)
        {
            // Apply multiplier to amplify the movement value
            currentMovementIntensity = totalMovement * multiplicateurMouvement;
            
            // Clamp to [0, 1] range for temperature calculation
            currentMovementIntensity = Mathf.Clamp01(currentMovementIntensity);
        }
        else
        {
            currentMovementIntensity = 0f;
        }
        
        // Update last known positions
        lastHeadPosition = currentHeadPosition;
        lastHeadRotation = currentHeadRotation;
    }
    
    void UpdateTemperature()
    {
        if (currentMovementIntensity > 0f)
        {
            // Calculate target temperature based on movement intensity
            targetTemperature = Mathf.Lerp(températureFroide, températureChaude, currentMovementIntensity);
            
            // Heat up quickly when moving
            currentTemperature = Mathf.Lerp(currentTemperature, targetTemperature, 
                                          Time.deltaTime * vitesseChauffe);
        }
        else
        {
            // Cool down gradually when not moving
            targetTemperature = températureFroide;
            currentTemperature = Mathf.Lerp(currentTemperature, targetTemperature, 
                                          Time.deltaTime * vitesseRefroidissement);
        }
    }
    
    void ApplyTemperature()
    {
        if (whiteBalance != null)
        {
            whiteBalance.temperature.value = currentTemperature;
        }
    }
    
    void ShowDebugInfo()
    {
        Debug.Log($"VR Head Temperature Controller Debug:\n" +
                  $"Current Temperature: {currentTemperature:F2}\n" +
                  $"Target Temperature: {targetTemperature:F2}\n" +
                  $"Movement Intensity: {currentMovementIntensity:F2}\n" +
                  $"Is Moving: {currentMovementIntensity > 0f}");
    }
    
    // Public methods for external control
    public void SetTemperatureRange(float cold, float hot)
    {
        températureFroide = cold;
        températureChaude = hot;
    }
    
    public float GetCurrentTemperature()
    {
        return currentTemperature;
    }
    
    public float GetMovementIntensity()
    {
        return currentMovementIntensity;
    }
    
    public bool IsHeadMoving()
    {
        return currentMovementIntensity > 0f;
    }
    
    public void ResetTemperature()
    {
        currentTemperature = températureFroide;
        targetTemperature = températureFroide;
        currentMovementIntensity = 0f;
    }
}