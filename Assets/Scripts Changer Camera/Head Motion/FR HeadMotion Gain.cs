using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FR_HeadMotion_Gain : MonoBehaviour
{
    [Header("Référence au Volume Global")]
    public Volume volume;
    
    [Header("Référence à la Caméra VR")]
    public Camera vrCamera;
    
    [Header("Contrôle du Script")]
    public bool activerScript = true;
    
    [Header("Réglages du Gain")]
    [Range(0f, 100f)] public float gainFaible = 1.0f;
    [Range(0f, 100f)] public float gainElevé = 2.0f;
    [Range(0.1f, 10f)] public float vitesseAugmentationGain = 3f;
    [Range(0.1f, 10f)] public float vitesseDiminutionGain = 1f;
    
    [Header("Couleur du Gain")]
    public Color couleurGainFaible = Color.white;
    public Color couleurGainElevé = Color.white;
    
    [Header("Détection du Mouvement")]
    [Range(0f, 1f)] public float seuilMouvement = 0.1f;
    [Range(1f, 50f)] public float multiplicateurMouvement = 10f;
    [Range(0.1f, 2f)] public float intervalleEchantillonnage = 0.1f;
    
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
        
        currentGainValue = gainFaible;
        targetGainValue = gainFaible;
        currentGainColor = couleurGainFaible;
        targetGainColor = couleurGainFaible;
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
        if (!activerScript || liftGammaGain == null || vrCamera == null)
            return;
        
        // Sample movement at specified rate
        if (Time.time - lastSampleTime >= intervalleEchantillonnage)
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
        if (totalMovement > seuilMouvement)
        {
            // Apply multiplier to amplify the movement value
            currentMovementIntensity = totalMovement * multiplicateurMouvement;
            
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
            targetGainValue = Mathf.Lerp(gainFaible, gainElevé, currentMovementIntensity);
            targetGainColor = Color.Lerp(couleurGainFaible, couleurGainElevé, currentMovementIntensity);
            
            // Increase gain quickly when moving
            currentGainValue = Mathf.Lerp(currentGainValue, targetGainValue, 
                                        Time.deltaTime * vitesseAugmentationGain);
            currentGainColor = Color.Lerp(currentGainColor, targetGainColor, 
                                        Time.deltaTime * vitesseAugmentationGain);
        }
        else
        {
            // Decrease gain gradually when not moving
            targetGainValue = gainFaible;
            targetGainColor = couleurGainFaible;
            currentGainValue = Mathf.Lerp(currentGainValue, targetGainValue, 
                                        Time.deltaTime * vitesseDiminutionGain);
            currentGainColor = Color.Lerp(currentGainColor, targetGainColor, 
                                        Time.deltaTime * vitesseDiminutionGain);
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
        gainFaible = low;
        gainElevé = high;
    }
    
    public void SetGainColors(Color low, Color high)
    {
        couleurGainFaible = low;
        couleurGainElevé = high;
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
        currentGainValue = gainFaible;
        targetGainValue = gainFaible;
        currentGainColor = couleurGainFaible;
        targetGainColor = couleurGainFaible;
        currentMovementIntensity = 0f;
    }
}