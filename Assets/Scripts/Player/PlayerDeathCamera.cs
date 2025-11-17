using System.Collections;
using UnityEngine;

public class PlayerDeathCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("The camera to zoom in. If not assigned, will find Main Camera.")]
    [SerializeField] private Camera targetCamera;
    
    [Tooltip("How much to zoom in (multiplier for camera size/field of view). Lower values = more zoom.")]
    [SerializeField] private float zoomAmount = 0.5f;
    
    [Tooltip("Duration of the zoom effect in seconds")]
    [SerializeField] private float zoomDuration = 1.5f;
    
    [Header("Time Settings")]
    [Tooltip("Final time scale when player dies (0 = paused, 0.5 = half speed, etc.)")]
    [SerializeField] private float targetTimeScale = 0.3f;
    
    [Tooltip("Duration to slow down time in seconds")]
    [SerializeField] private float timeSlowDuration = 1.0f;
    
    [Header("Timing")]
    [Tooltip("Delay before starting the effect (in seconds)")]
    [SerializeField] private float startDelay = 0.1f;

    [Header("Player Presentation")]
    [Tooltip("Should the player's movement be locked during the death effect?")]
    [SerializeField] private bool lockMovementDuringEffect = true;
    [Tooltip("Freeze the player's animation at the current frame when death occurs.")]
    [SerializeField] private bool freezeAnimationOnDeath = true;
    [Tooltip("Keep the hit flash + shake visuals active until the effect finishes.")]
    [SerializeField] private bool sustainHitEffect = true;
    
    private HealthSystem playerHealthSystem;
    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private FrameBasedPlayerAnimator playerAnimator;
    private float originalCameraSize;
    private float originalFieldOfView;
    private bool isOrthographic;
    private bool effectActive = false;
    private static bool isHandlingDeathTransition = false;
    
    public static bool IsHandlingDeathTransition => isHandlingDeathTransition;
    
    public bool debug = false;
    
    private void Awake()
    {
        // Find the player's HealthSystem
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerHealthSystem = playerObject.GetComponent<HealthSystem>();
            playerMovement = playerObject.GetComponent<PlayerMovement>();
            playerAnimator = playerObject.GetComponent<FrameBasedPlayerAnimator>();
            
            // Check if camera is a child of the player
            if (targetCamera == null)
            {
                targetCamera = playerObject.GetComponentInChildren<Camera>();
            }
        }
    }
    
    private void Start()
    {
        // Try to find camera if not already found
        if (targetCamera == null)
        {
            FindActiveCamera();
        }
        
        // Store original camera values if found
        StoreOriginalCameraValues();
    }
    
    private void FindActiveCamera()
    {
        // First try Main Camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        // If still null, try finding any active camera
        if (targetCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                if (cam.gameObject.activeInHierarchy && cam.enabled)
                {
                    targetCamera = cam;
                    break;
                }
            }
        }
        
        // Last resort: check if player has a camera child
        if (targetCamera == null && playerObject != null)
        {
            targetCamera = playerObject.GetComponentInChildren<Camera>();
        }
        
        if (debug)
        {
            if (targetCamera != null)
            {
                Debug.Log($"[PlayerDeathCamera] Found camera: {targetCamera.name}");
            }
            else
            {
                Debug.LogWarning("[PlayerDeathCamera] No camera found!");
            }
        }
    }
    
    private void StoreOriginalCameraValues()
    {
        if (targetCamera != null)
        {
            isOrthographic = targetCamera.orthographic;
            if (isOrthographic)
            {
                originalCameraSize = targetCamera.orthographicSize;
            }
            else
            {
                originalFieldOfView = targetCamera.fieldOfView;
            }
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to player death event
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnDeath += OnPlayerDeath;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from player death event
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnDeath -= OnPlayerDeath;
        }
    }
    
    private void OnPlayerDeath(HealthSystem healthSystem)
    {
        // Only trigger if it's the player and effect isn't already active
        if (healthSystem.CompareTag("Player") && !effectActive)
        {
            // Set flag immediately so HealthSystem knows we're handling the transition
            isHandlingDeathTransition = true;
            
            // Find camera again in case it changed (new level, etc.)
            if (targetCamera == null || !targetCamera.gameObject.activeInHierarchy)
            {
                FindActiveCamera();
                StoreOriginalCameraValues();
            }
            
            if (targetCamera == null)
            {
                if (debug) Debug.LogError("[PlayerDeathCamera] Cannot start effect - no camera found!");
                isHandlingDeathTransition = false;
                return;
            }
            
            if (debug) Debug.Log($"[PlayerDeathCamera] Player died, starting death camera effect with camera: {targetCamera.name}");
            StartCoroutine(DeathCameraEffect());
        }
    }
    
    private IEnumerator DeathCameraEffect()
    {
        effectActive = true;
        PreparePlayerForDeathEffect();
        
        if (debug) Debug.Log("[PlayerDeathCamera] Starting death camera effect coroutine");
        
        // Wait for initial delay
        if (startDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(startDelay);
        }
        
        // Start both effects simultaneously
        Coroutine zoomCoroutine = null;
        Coroutine timeCoroutine = null;
        
        if (targetCamera != null)
        {
            if (debug) Debug.Log("[PlayerDeathCamera] Starting zoom coroutine");
            zoomCoroutine = StartCoroutine(ZoomCamera());
        }
        else
        {
            if (debug) Debug.LogWarning("[PlayerDeathCamera] No camera found, skipping zoom");
        }
        
        if (debug) Debug.Log("[PlayerDeathCamera] Starting time slowdown coroutine");
        timeCoroutine = StartCoroutine(SlowTime());
        
        // Wait for both coroutines to complete
        if (zoomCoroutine != null)
        {
            yield return zoomCoroutine;
        }
        if (timeCoroutine != null)
        {
            yield return timeCoroutine;
        }
        
        if (debug) Debug.Log("[PlayerDeathCamera] Death camera effect completed");
        
        CleanupAfterDeathEffect();

        // Disable player now that effect is done
        if (playerObject != null)
        {
            if (debug) Debug.Log("[PlayerDeathCamera] Disabling player GameObject");
            playerObject.SetActive(false);
        }
        
        // Now trigger the game state change after effect completes
        if (GameState.Instance != null)
        {
            GameState.Instance.OnPlayerDeath();
        }
        else
        {
            if (debug) Debug.LogError("[PlayerDeathCamera] GameState.Instance is null!");
        }
        
        isHandlingDeathTransition = false;
    }

    private void PreparePlayerForDeathEffect()
    {
        if (lockMovementDuringEffect && playerMovement != null)
        {
            playerMovement.CanMove = false;
        }

        // Freeze animation at current frame
        if (freezeAnimationOnDeath && playerAnimator != null)
        {
            playerAnimator.FreezeAnimationAtCurrentFrame();
            if (debug) Debug.Log("[PlayerDeathCamera] Frozen player animation at current frame");
        }

        if (sustainHitEffect && playerHealthSystem != null)
        {
            playerHealthSystem.BeginDeathHitEffect();
        }
    }

    private void CleanupAfterDeathEffect()
    {
        if (sustainHitEffect && playerHealthSystem != null)
        {
            playerHealthSystem.EndDeathHitEffect();
        }
    }
    
    private IEnumerator ZoomCamera()
    {
        if (targetCamera == null || !targetCamera.gameObject.activeInHierarchy)
        {
            if (debug) Debug.LogWarning("[PlayerDeathCamera] Camera is null or inactive, cannot zoom");
            yield break;
        }
        
        // Get current values at start of zoom (in case camera changed)
        float startSize = isOrthographic ? targetCamera.orthographicSize : targetCamera.fieldOfView;
        float targetSize = startSize * zoomAmount;
        
        if (debug) Debug.Log($"[PlayerDeathCamera] Starting zoom from {startSize} to {targetSize} (camera: {targetCamera.name})");
        
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            if (targetCamera == null || !targetCamera.gameObject.activeInHierarchy)
            {
                if (debug) Debug.LogWarning("[PlayerDeathCamera] Camera became null/inactive during zoom");
                yield break;
            }
            
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so zoom continues even when time is slowed
            float t = elapsed / zoomDuration;
            
            // Smooth easing curve (ease-in-out)
            t = t * t * (3f - 2f * t);
            
            if (isOrthographic)
            {
                targetCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            }
            else
            {
                targetCamera.fieldOfView = Mathf.Lerp(startSize, targetSize, t);
            }
            
            yield return null;
        }
        
        // Ensure we reach the target value
        if (targetCamera != null && targetCamera.gameObject.activeInHierarchy)
        {
            if (isOrthographic)
            {
                targetCamera.orthographicSize = targetSize;
            }
            else
            {
                targetCamera.fieldOfView = targetSize;
            }
        }
    }
    
    private IEnumerator SlowTime()
    {
        float elapsed = 0f;
        float originalTimeScale = Time.timeScale;
        
        if (debug) Debug.Log($"[PlayerDeathCamera] Starting time slowdown from {originalTimeScale} to {targetTimeScale}");
        
        while (elapsed < timeSlowDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time for the timer itself
            float t = elapsed / timeSlowDuration;
            
            // Smooth easing curve (ease-in-out)
            t = t * t * (3f - 2f * t);
            
            Time.timeScale = Mathf.Lerp(originalTimeScale, targetTimeScale, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Adjust fixed delta time to match
            
            yield return null;
        }
        
        // Ensure we reach the target time scale
        Time.timeScale = targetTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    
    // Public method to reset camera and time (useful for respawn)
    public void ResetDeathCamera()
    {
        if (targetCamera != null)
        {
            if (isOrthographic)
            {
                targetCamera.orthographicSize = originalCameraSize;
            }
            else
            {
                targetCamera.fieldOfView = originalFieldOfView;
            }
        }
        
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        effectActive = false;
        
        StopAllCoroutines();
        
        if (debug) Debug.Log("[PlayerDeathCamera] Death camera effect reset");
    }
    
    private void OnDestroy()
    {
        // Reset time scale when script is destroyed
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}

