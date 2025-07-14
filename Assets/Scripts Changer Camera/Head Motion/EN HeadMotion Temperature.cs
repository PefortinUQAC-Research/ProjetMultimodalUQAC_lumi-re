using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EN_HeadMotion_Temperature : MonoBehaviour
{
    [Header("Global Volume Reference")]
    public Volume volume;
    
    [Header("VR Camera Reference")]
    public Camera vrCamera;
    
    [Header("Script Control")]
    public bool enableScript = true;
    
    [Header("Temperature Settings")]
    [Range(-100f, 100f)] public float coldTemperature = -20f;
    [Range(-100f, 100f)] public float hotTemperature = 50f;
    [Range(0.1f, 10f)] public float heatingSpeed = 3f;
    [Range(0.1f, 10f)] public float coolingSpeed = 1f;
    
    [Header("Movement Detection")]
    [Range(0f, 1f)] public float movementThreshold = 0.1f;
    [Range(1f, 50f)] public float movementMultiplier = 10f;
    [Range(0.1f, 2f)] public float movementSampleRate = 0.1f;
    
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
        
        currentTemperature = coldTemperature;
        targetTemperature = coldTemperature;
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
        if (!enableScript || whiteBalance == null || vrCamera == null)
            return;
        
        // Sample movement at specified rate
        if (Time.time - lastSampleTime >= movementSampleRate)
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
        if (totalMovement > movementThreshold)
        {
            // Apply multiplier to amplify the movement value
            currentMovementIntensity = totalMovement * movementMultiplier;
            
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
            targetTemperature = Mathf.Lerp(coldTemperature, hotTemperature, currentMovementIntensity);
            
            // Heat up quickly when moving
            currentTemperature = Mathf.Lerp(currentTemperature, targetTemperature, 
                                          Time.deltaTime * heatingSpeed);
        }
        else
        {
            // Cool down gradually when not moving
            targetTemperature = coldTemperature;
            currentTemperature = Mathf.Lerp(currentTemperature, targetTemperature, 
                                          Time.deltaTime * coolingSpeed);
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
        coldTemperature = cold;
        hotTemperature = hot;
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
        currentTemperature = coldTemperature;
        targetTemperature = coldTemperature;
        currentMovementIntensity = 0f;
    }
}