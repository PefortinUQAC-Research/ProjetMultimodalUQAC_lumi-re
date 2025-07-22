using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class VRLaserPointer : MonoBehaviour
{
    [Header("Laser Settings")]
    public LineRenderer laserLine;
    public Transform laserOrigin; // Point d'origine du laser (bout du contrôleur)
    public LayerMask interactionLayers = -1; // Couches avec lesquelles le laser peut interagir
    public float maxLaserDistance = 30f;
    
    [Header("Visual Settings")]
    public Color laserColor = Color.red;
    public float laserWidth = 0.02f;
    public Material laserMaterial;
    
    [Header("Hit Point Indicator")]
    public GameObject hitPointPrefab; // Optionnel: un petit objet pour marquer le point visé
    private GameObject currentHitPoint;
    
    [Header("Input Settings")]
    public UnityEngine.XR.InputDevice inputDevice; // Le contrôleur VR
    [Range(0.1f, 2.0f)]
    public float clickCooldown = 0.3f; // Délai entre les clics en secondes
    
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    private Camera playerCamera;
    private GraphicRaycaster[] graphicRaycasters;
    private EventSystem eventSystem;
    
    // Variables pour l'UI
    private GameObject currentUITarget;
    private Toggle currentToggle;
    private Button currentButton;
    
    // Variables pour le délai de clic
    private float lastClickTime = 0f;
    private bool wasButtonPressed = false;
    
    void Start()
    {
        // Configuration du LineRenderer
        SetupLaserLine();
        
        // Récupérer le XRRayInteractor si présent
        rayInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        
        // Trouver la caméra principale (généralement celle du joueur VR)
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
        
        // Récupérer tous les GraphicRaycaster dans la scène
        graphicRaycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        
        // Récupérer l'EventSystem
        eventSystem = EventSystem.current;
        if (eventSystem == null)
            eventSystem = FindFirstObjectByType<EventSystem>();
        
        // Créer le point d'impact si un prefab est défini
        if (hitPointPrefab != null)
        {
            currentHitPoint = Instantiate(hitPointPrefab);
            currentHitPoint.SetActive(false);
        }
        
        // Configurer le contrôleur VR (côté droit)
        SetupVRInput();
    }
    
    void SetupVRInput()
    {
        // Obtenir le contrôleur droit
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
        
        if (rightHandDevices.Count > 0)
        {
            inputDevice = rightHandDevices[0];
        }
    }
    
    void SetupLaserLine()
    {
        if (laserLine == null)
        {
            // Créer automatiquement un LineRenderer si non assigné
            GameObject laserObject = new GameObject("Laser Line");
            laserObject.transform.SetParent(transform);
            laserLine = laserObject.AddComponent<LineRenderer>();
        }
        
        // Configuration du LineRenderer
        laserLine.material = laserMaterial;
        
        // Créer un gradient avec la couleur du laser
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(laserColor, 0.0f), new GradientColorKey(laserColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        laserLine.colorGradient = gradient;
        
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;
        laserLine.positionCount = 2;
        laserLine.useWorldSpace = true;
        
        // Désactiver l'ombre et la réception d'ombre
        laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        laserLine.receiveShadows = false;
    }
    
    void Update()
    {
        UpdateLaser();
        CheckForInput();
    }
    
    void UpdateLaser()
    {
        Vector3 startPosition = laserOrigin != null ? laserOrigin.position : transform.position;
        Vector3 direction = laserOrigin != null ? laserOrigin.forward : transform.forward;
        
        // Reset les variables UI
        currentUITarget = null;
        currentToggle = null;
        currentButton = null;
        
        // D'abord, vérifier les objets 3D
        RaycastHit hit;
        Vector3 endPosition;
        bool hitSomething = false;
        
        if (Physics.Raycast(startPosition, direction, out hit, maxLaserDistance, interactionLayers))
        {
            endPosition = hit.point;
            hitSomething = true;
        }
        else
        {
            endPosition = startPosition + direction * maxLaserDistance;
        }
        
        // Ensuite, vérifier les éléments UI
        CheckUIRaycast(startPosition, direction, ref endPosition, ref hitSomething);
        
        // Mettre à jour le point d'impact
        if (currentHitPoint != null)
        {
            if (hitSomething)
            {
                currentHitPoint.SetActive(true);
                currentHitPoint.transform.position = endPosition;
                
                if (hit.normal != Vector3.zero)
                    currentHitPoint.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
            else
            {
                currentHitPoint.SetActive(false);
            }
        }
        
        // Mettre à jour les positions du LineRenderer
        laserLine.SetPosition(0, startPosition);
        laserLine.SetPosition(1, endPosition);
    }
    
    void CheckUIRaycast(Vector3 startPosition, Vector3 direction, ref Vector3 endPosition, ref bool hitSomething)
    {
        // Créer un ray depuis la position du laser
        Ray ray = new Ray(startPosition, direction);
        
        // Pour chaque GraphicRaycaster dans la scène
        foreach (var raycaster in graphicRaycasters)
        {
            if (raycaster == null || !raycaster.enabled) continue;
            
            // Convertir le ray en coordonnées d'écran pour l'UI
            Canvas canvas = raycaster.GetComponent<Canvas>();
            if (canvas == null) continue;
            
            // Pour un Canvas en World Space, nous devons faire un raycast direct
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                // Créer les données de pointeur pour l'UI
                PointerEventData pointerData = new PointerEventData(eventSystem);
                
                // Convertir la position du ray en position d'écran relative au canvas
                Vector3 worldPoint;
                if (RaycastToCanvasWorldSpace(ray, canvas, out worldPoint))
                {
                    // Convertir la position mondiale en position locale du canvas
                    Vector2 localPoint;
                    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRect, 
                        RectTransformUtility.WorldToScreenPoint(playerCamera, worldPoint), 
                        playerCamera, 
                        out localPoint
                    );
                    
                    pointerData.position = RectTransformUtility.WorldToScreenPoint(playerCamera, worldPoint);
                    
                    // Effectuer le raycast UI
                    List<RaycastResult> results = new List<RaycastResult>();
                    raycaster.Raycast(pointerData, results);
                    
                    if (results.Count > 0)
                    {
                        var result = results[0];
                        currentUITarget = result.gameObject;
                        
                        // Vérifier si c'est un Toggle
                        Toggle toggle = currentUITarget.GetComponent<Toggle>();
                        if (toggle == null)
                        {
                            // Vérifier dans les parents
                            toggle = currentUITarget.GetComponentInParent<Toggle>();
                        }
                        
                        // Vérifier si c'est un Button
                        Button button = currentUITarget.GetComponent<Button>();
                        if (button == null)
                        {
                            // Vérifier dans les parents
                            button = currentUITarget.GetComponentInParent<Button>();
                        }
                        
                        if (toggle != null)
                        {
                            currentToggle = toggle;
                            endPosition = result.worldPosition;
                            hitSomething = true;
                            break;
                        }
                        else if (button != null && button.interactable)
                        {
                            currentButton = button;
                            endPosition = result.worldPosition;
                            hitSomething = true;
                            break;
                        }
                    }
                }
            }
        }
    }
    
    bool RaycastToCanvasWorldSpace(Ray ray, Canvas canvas, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null) return false;
        
        // Créer un plan basé sur le canvas
        Plane canvasPlane = new Plane(-canvasRect.forward, canvasRect.position);
        
        float distance;
        if (canvasPlane.Raycast(ray, out distance))
        {
            hitPoint = ray.GetPoint(distance);
            
            // Vérifier si le point est dans les limites du canvas
            Vector3 localPoint = canvasRect.InverseTransformPoint(hitPoint);
            Rect rect = canvasRect.rect;
            
            if (localPoint.x >= rect.xMin && localPoint.x <= rect.xMax &&
                localPoint.y >= rect.yMin && localPoint.y <= rect.yMax)
            {
                return true;
            }
        }
        
        return false;
    }
    
    void CheckForInput()
    {
        // Vérifier si le bouton de sélection est pressé
        bool selectPressed = false;
        
        if (inputDevice.isValid)
        {
            // Vérifier le trigger ou le bouton de sélection
            inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out selectPressed);
            
            // Alternative: vérifier le bouton A ou X
            if (!selectPressed)
            {
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out selectPressed);
            }
        }
        
        // Pour les tests en éditeur, permettre le clic souris
        if (!selectPressed && Application.isEditor)
        {
            selectPressed = Input.GetMouseButtonDown(0);
        }
        
        // Gérer le délai de clic : détecter le passage de "non-pressé" à "pressé"
        bool shouldClick = false;
        
        if (selectPressed && !wasButtonPressed)
        {
            // Le bouton vient d'être pressé (transition de false à true)
            if (Time.time - lastClickTime >= clickCooldown)
            {
                shouldClick = true;
                lastClickTime = Time.time;
            }
        }
        
        // Mettre à jour l'état du bouton pour la prochaine frame
        wasButtonPressed = selectPressed;
        
        // Si on doit cliquer et qu'on vise un toggle ou un button
        if (shouldClick)
        {
            if (currentToggle != null)
            {
                // Activer/désactiver le toggle
                currentToggle.isOn = !currentToggle.isOn;
                
                // Déclencher l'événement du toggle
                currentToggle.onValueChanged.Invoke(currentToggle.isOn);
                
                Debug.Log($"Toggle {currentToggle.name} activé : {currentToggle.isOn}");
            }
            else if (currentButton != null && currentButton.interactable)
            {
                // Déclencher le clic du button
                currentButton.onClick.Invoke();
                
                Debug.Log($"Button {currentButton.name} cliqué");
            }
        }
    }
    
    // Méthode publique pour activer/désactiver le laser
    public void SetLaserActive(bool active)
    {
        laserLine.enabled = active;
        if (currentHitPoint != null)
        {
            currentHitPoint.SetActive(active && currentHitPoint.activeSelf);
        }
    }
    
    // Méthode pour changer la couleur du laser
    public void SetLaserColor(Color newColor)
    {
        laserColor = newColor;
        
        // Créer un nouveau gradient avec la nouvelle couleur
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(newColor, 0.0f), new GradientColorKey(newColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        laserLine.colorGradient = gradient;
    }
    
    void OnDisable()
    {
        if (currentHitPoint != null)
        {
            currentHitPoint.SetActive(false);
        }
    }
    
    // Méthode de debug pour visualiser ce qui est détecté
    void OnDrawGizmosSelected()
    {
        if (laserOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(laserOrigin.position, laserOrigin.forward * maxLaserDistance);
        }
    }
}