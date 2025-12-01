using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeathCamera : MonoBehaviour
{
    #region Variables

    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float zoomAmount = 0.5f;
    [SerializeField] private float zoomDuration = 1.5f;
    [SerializeField] private bool centerOnPlayer = true;
    
    [Header("Time Settings")]
    [Tooltip("Final time scale when player dies (0 = paused, 0.5 = half speed, etc.)")]
    [SerializeField] private float targetTimeScale = 0.3f;
    [SerializeField] private float timeSlowDuration = 1.0f;
    
    [Header("Timing")]
    [SerializeField] private float startDelay = 0.1f;

    [Header("Player Presentation")]
    [SerializeField] private bool lockMovementDuringEffect = true;
    [SerializeField] private bool freezeAnimationOnDeath = true;
    [SerializeField] private bool sustainHitEffect = true;
    
    [Header("Vignette Effect")]
    [Tooltip("Image component for the vignette effect. Should be a full-screen sprite that fades in during death.")]
    [SerializeField] private Image vignetteSprite;
    [SerializeField] private float vignetteMaxAlpha = 0.8f;
    
    // references
    private HealthSystem playerHealthSystem;
    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private FrameBasedPlayerAnimator playerAnimator;
    private PlayerAttack playerAttack;
    private PlayerCombat playerCombat;
    
    // Store original enabled states to restore later
    private bool originalPlayerAttackEnabled = true;
    private bool originalPlayerCombatEnabled = true;

    // camera elements
    private float originalCameraSize;
    private float originalFieldOfView;
    private bool isOrthographic;
    private Vector3 originalCameraPosition;
    private bool effectActive = false;
    private static bool isHandlingDeathTransition = false;
    
    // vignette elements
    private float originalVignetteAlpha = 0f;
    
    public static bool IsHandlingDeathTransition => isHandlingDeathTransition;

    #endregion

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
            playerAttack = playerObject.GetComponent<PlayerAttack>();
            playerCombat = playerObject.GetComponent<PlayerCombat>();
            
            // Check if camera is a child of the player
            if (targetCamera == null)
            {
                targetCamera = playerObject.GetComponentInChildren<Camera>();
            }
        }
    }
    
    private void Start()
    {
        // Try to find camera
        if (targetCamera == null)
        {
            FindActiveCamera();
        }
        
        // Store original camera values
        StoreOriginalCameraValues();
    }
    
    private void FindActiveCamera()
    {
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
            originalCameraPosition = targetCamera.transform.position;
        }
        
        // Store original vignette alpha
        if (vignetteSprite != null)
        {
            originalVignetteAlpha = vignetteSprite.color.a;
            // Ensure vignette starts invisible
            Color vignetteColor = vignetteSprite.color;
            vignetteColor.a = 0f;
            vignetteSprite.color = vignetteColor;
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to player death event
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnDeath += OnPlayerDeath;
        }
        
        // Subscribe to game state changes to hide vignette when leaving Results screen
        GameEvents.OnGameStateChanged += OnGameStateChanged;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from player death event
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnDeath -= OnPlayerDeath;
        }
        
        // Unsubscribe from game state changes
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
    }
    
    private void OnGameStateChanged(GameState.GameStates newState)
    {
        // Hide vignette when leaving the Results screen
        if (newState != GameState.GameStates.Results && vignetteSprite != null)
        {
            HideVignette();
        }
    }
    
    private void HideVignette()
    {
        if (vignetteSprite != null)
        {
            Color vignetteColor = vignetteSprite.color;
            vignetteColor.a = 0f;
            vignetteSprite.color = vignetteColor;
            
            // Optionally disable the GameObject to ensure it's hidden
            if (vignetteSprite.gameObject != null)
            {
                vignetteSprite.gameObject.SetActive(false);
            }
            
            if (debug) Debug.Log("[PlayerDeathCamera] Vignette hidden");
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
            else
            {
                // Update camera position in case it moved
                originalCameraPosition = targetCamera.transform.position;
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
    
    // Public method to trigger death camera effect (for boss defeat on final floor)
    public void TriggerDeathCameraEffect()
    {
        if (effectActive)
        {
            if (debug) Debug.LogWarning("[PlayerDeathCamera] Effect already active, ignoring trigger");
            return;
        }
        
        // Set flag immediately
        isHandlingDeathTransition = true;
        
        // Find camera again in case it changed
        if (targetCamera == null || !targetCamera.gameObject.activeInHierarchy)
        {
            FindActiveCamera();
            StoreOriginalCameraValues();
        }
        else
        {
            // Update camera position in case it moved
            originalCameraPosition = targetCamera.transform.position;
        }
        
        if (targetCamera == null)
        {
            if (debug) Debug.LogError("[PlayerDeathCamera] Cannot start effect - no camera found!");
            isHandlingDeathTransition = false;
            return;
        }
        
        if (debug) Debug.Log("[PlayerDeathCamera] Triggering death camera effect for final boss defeat");
        StartCoroutine(DeathCameraEffect());
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
        
        // Start all effects simultaneously
        Coroutine zoomCoroutine = null;
        Coroutine timeCoroutine = null;
        Coroutine vignetteCoroutine = null;
        
        if (targetCamera != null)
        {
            if (debug) Debug.Log("[PlayerDeathCamera] Starting zoom coroutine");
            zoomCoroutine = StartCoroutine(ZoomCamera());
        }
        else
        {
            if (debug) Debug.LogWarning("[PlayerDeathCamera] No camera found, skipping zoom");
        }
        
        if (vignetteSprite != null)
        {
            if (debug) Debug.Log("[PlayerDeathCamera] Starting vignette fade-in coroutine");
            vignetteCoroutine = StartCoroutine(FadeInVignette());
        }
        else
        {
            if (debug) Debug.LogWarning("[PlayerDeathCamera] No vignette sprite found, skipping vignette effect");
        }
        
        if (debug) Debug.Log("[PlayerDeathCamera] Starting time slowdown coroutine");
        timeCoroutine = StartCoroutine(SlowTime());
        
        // Wait for all coroutines to complete
        if (zoomCoroutine != null)
        {
            yield return zoomCoroutine;
        }
        if (vignetteCoroutine != null)
        {
            yield return vignetteCoroutine;
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

        // Disable player attacks - this prevents PlayerAttack.Update() from running
        if (playerAttack != null)
        {
            originalPlayerAttackEnabled = playerAttack.enabled;
            playerAttack.enabled = false;
            if (debug) Debug.Log($"[PlayerDeathCamera] Disabled player attacks (was {originalPlayerAttackEnabled})");
        }
        
        // Also disable PlayerCombat to prevent entering combat state
        if (playerCombat != null)
        {
            originalPlayerCombatEnabled = playerCombat.enabled;
            playerCombat.enabled = false;
            if (debug) Debug.Log($"[PlayerDeathCamera] Disabled player combat (was {originalPlayerCombatEnabled})");
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
        
        // Re-enable player attacks and combat before disabling the GameObject
        // This ensures they're enabled when the player respawns
        if (playerAttack != null)
        {
            playerAttack.enabled = originalPlayerAttackEnabled;
            if (debug) Debug.Log($"[PlayerDeathCamera] Re-enabled player attacks (restored to {originalPlayerAttackEnabled})");
        }
        
        if (playerCombat != null)
        {
            playerCombat.enabled = originalPlayerCombatEnabled;
            if (debug) Debug.Log($"[PlayerDeathCamera] Re-enabled player combat (restored to {originalPlayerCombatEnabled})");
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
        
        // Store camera position and calculate target position (centered on player)
        Vector3 startPosition = targetCamera.transform.position;
        Vector3 targetPosition = startPosition;
        
        if (centerOnPlayer && playerObject != null)
        {
            // Calculate position to center player on screen
            // For orthographic: position camera so player is at center
            // For perspective: position camera so player is at center of view
            Vector3 playerPosition = playerObject.transform.position;
            
            // Keep the camera's Z position (depth) but center X and Y on player
            if (isOrthographic)
            {
                targetPosition = new Vector3(playerPosition.x, playerPosition.y, startPosition.z);
            }
            else
            {
                // For perspective cameras, we need to account for the camera's forward direction
                // Calculate offset to center player in view
                Vector3 cameraForward = targetCamera.transform.forward;
                float distanceToPlayer = Vector3.Distance(startPosition, playerPosition);
                targetPosition = playerPosition - cameraForward * distanceToPlayer;
                // Preserve original Z if it's a 2D setup
                if (Mathf.Approximately(startPosition.z, playerPosition.z))
                {
                    targetPosition.z = startPosition.z;
                }
            }
            
            if (debug) Debug.Log($"[PlayerDeathCamera] Centering camera on player: {startPosition} -> {targetPosition}");
        }
        
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
            
            // Apply zoom
            if (isOrthographic)
            {
                targetCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            }
            else
            {
                targetCamera.fieldOfView = Mathf.Lerp(startSize, targetSize, t);
            }
            
            // Apply camera position centering
            if (centerOnPlayer)
            {
                targetCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }
            
            yield return null;
        }
        
        // Ensure we reach the target values
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
            
            if (centerOnPlayer)
            {
                targetCamera.transform.position = targetPosition;
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
    
    private IEnumerator FadeInVignette()
    {
        if (vignetteSprite == null)
        {
            if (debug) Debug.LogWarning("[PlayerDeathCamera] Vignette sprite is null, cannot fade in");
            yield break;
        }
        
        // Ensure vignette GameObject is active
        if (!vignetteSprite.gameObject.activeInHierarchy)
        {
            if (debug) Debug.Log("[PlayerDeathCamera] Activating vignette GameObject");
            vignetteSprite.gameObject.SetActive(true);
        }
        
        // Ensure vignette Image component is enabled
        vignetteSprite.enabled = true;
        
        Color vignetteColor = vignetteSprite.color;
        float startAlpha = vignetteColor.a;
        float targetAlpha = vignetteMaxAlpha;
        
        if (debug) Debug.Log($"[PlayerDeathCamera] Starting vignette fade-in from {startAlpha} to {targetAlpha}. GameObject active: {vignetteSprite.gameObject.activeInHierarchy}, Image enabled: {vignetteSprite.enabled}");
        
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            if (vignetteSprite == null || !vignetteSprite.gameObject.activeInHierarchy)
            {
                if (debug) Debug.LogWarning("[PlayerDeathCamera] Vignette sprite became null/inactive during fade");
                yield break;
            }
            
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so fade continues even when time is slowed
            float t = elapsed / zoomDuration;
            
            // Smooth easing curve (ease-in-out)
            t = t * t * (3f - 2f * t);
            
            vignetteColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            vignetteSprite.color = vignetteColor;
            
            yield return null;
        }
        
        // Ensure we reach the target alpha
        if (vignetteSprite != null && vignetteSprite.gameObject.activeInHierarchy)
        {
            vignetteColor.a = targetAlpha;
            vignetteSprite.color = vignetteColor;
        }
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
            
            // Reset camera position
            targetCamera.transform.position = originalCameraPosition;
        }
        
        // Reset vignette - hide it completely
        HideVignette();
        
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

