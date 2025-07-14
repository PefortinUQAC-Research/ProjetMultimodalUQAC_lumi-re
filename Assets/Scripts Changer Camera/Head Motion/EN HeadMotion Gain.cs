using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EN_HeadMotion_Gain : MonoBehaviour
{
    [Header("Global Volume Reference")]
    public Volume volume;
    
    [Header("VR Camera Reference")]
    public Camera vrCamera;
    
    [Header("Script Control")]
    public bool enableScript = true;
    
    [Header("Gain Settings")]
    [Range(0f, 100f)] public float lowGainValue = 1.0f;
    [Range(0f, 100f)] public float highGainValue = 2.0f;
    [Range(0.1f, 10f)] public float gainIncreaseSpeed = 3f;
    [Range(0.1f, 10f)] public float gainDecreaseSpeed = 1f;
    
    [Header("Gain Color")]
    public Color lowGainColor = Color.white;
    public Color highGainColor = Color.white;
    
    [Header("Movement Detection")]
    [Range(0f, 1f)] public float movementThreshold = 0.1f;
    [Range(1f, 50f)] public float movementMultiplier = 10f;
    [Range(0.1f, 2f)] public float movementSampleRate = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private LiftGammaGain liftGammaGain;
    private Vector3 lastHeadPosition;
    private Quaternion lastHeadRotation;
    private float currentGainValue;
    private float targetGainValue;
    private Color currentGainColor;
    private Color targetGainColor;
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
        
        currentGainValue = lowGainValue;
        targetGainValue = lowGainValue;
        currentGainColor = lowGainColor;
        targetGainColor = lowGainColor;
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
        
        // Get LiftGammaGain component
        if (!volume.profile.TryGet(out liftGammaGain))
        {
            Debug.LogError("LiftGammaGain is not enabled in the Volume Profile.");
            enabled = false;
            return;
        }
        
        // Enable LiftGammaGain
        liftGammaGain.active = true;
        liftGammaGain.gain.overrideState = true;
    }
    
    void Update()
    {
        if (!enableScript || liftGammaGain == null || vrCamera == null)
            return;
        
        // Sample movement at specified rate
        if (Time.time - lastSampleTime >= movementSampleRate)
        {
            SampleMovement();
            lastSampleTime = Time.time;
        }
        
        UpdateGain();
        ApplyGain();
        
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
            
            // Clamp to [0, 1] range for gain calculation
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
    
    void UpdateGain()
    {
        if (currentMovementIntensity > 0f)
        {
            // Calculate target gain based on movement intensity
            targetGainValue = Mathf.Lerp(lowGainValue, highGainValue, currentMovementIntensity);
            targetGainColor = Color.Lerp(lowGainColor, highGainColor, currentMovementIntensity);
            
            // Increase gain quickly when moving
            currentGainValue = Mathf.Lerp(currentGainValue, targetGainValue, 
                                        Time.deltaTime * gainIncreaseSpeed);
            currentGainColor = Color.Lerp(currentGainColor, targetGainColor, 
                                        Time.deltaTime * gainIncreaseSpeed);
        }
        else
        {
            // Decrease gain gradually when not moving
            targetGainValue = lowGainValue;
            targetGainColor = lowGainColor;
            currentGainValue = Mathf.Lerp(currentGainValue, targetGainValue, 
                                        Time.deltaTime * gainDecreaseSpeed);
            currentGainColor = Color.Lerp(currentGainColor, targetGainColor, 
                                        Time.deltaTime * gainDecreaseSpeed);
        }
    }
    
    void ApplyGain()
    {
        if (liftGammaGain != null)
        {
            // Unity applies an internal offset to gain values
            // We need to subtract 1 to get the actual desired values
            float adjustedGainValue = currentGainValue - 1f;
            
            Vector4 gainVector = new Vector4(
                currentGainColor.r * adjustedGainValue,
                currentGainColor.g * adjustedGainValue,
                currentGainColor.b * adjustedGainValue,
                adjustedGainValue
            );
            
            liftGammaGain.gain.value = gainVector;
        }
    }
    
    void ShowDebugInfo()
    {
        Debug.Log($"VR Head Gain Controller Debug:\n" +
                  $"Current Gain Value: {currentGainValue:F2}\n" +
                  $"Target Gain Value: {targetGainValue:F2}\n" +
                  $"Current Gain Color: {currentGainColor}\n" +
                  $"Movement Intensity: {currentMovementIntensity:F2}\n" +
                  $"Is Moving: {currentMovementIntensity > 0f}");
    }
    
    // Public methods for external control
    public void SetGainRange(float low, float high)
    {
        lowGainValue = low;
        highGainValue = high;
    }
    
    public void SetGainColors(Color low, Color high)
    {
        lowGainColor = low;
        highGainColor = high;
    }
    
    public float GetCurrentGainValue()
    {
        return currentGainValue;
    }
    
    public Color GetCurrentGainColor()
    {
        return currentGainColor;
    }
    
    public float GetMovementIntensity()
    {
        return currentMovementIntensity;
    }
    
    public bool IsHeadMoving()
    {
        return currentMovementIntensity > 0f;
    }
    
    public void ResetGain()
    {
        currentGainValue = lowGainValue;
        targetGainValue = lowGainValue;
        currentGainColor = lowGainColor;
        targetGainColor = lowGainColor;
        currentMovementIntensity = 0f;
    }
}