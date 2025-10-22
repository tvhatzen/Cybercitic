using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    /// <summary> Whether movement is allowed. PlayerCombat can toggle this. </summary>
    public bool CanMove { get; set; } = true;

    private CharacterController controller;
    private EntityData entityData;
    private FrameBasedPlayerAnimator frameAnimator;
    
    [Header("Fallback Settings")]
    [SerializeField] private float fallbackMoveSpeed = 5f; // Fallback speed if EntityData is missing
    
    [Header("Animation Settings")]
    [SerializeField] private bool enableMovementAnimations = true;
    [SerializeField] private float movementThreshold = 0.1f; // Minimum movement to trigger running animation
    
    [Header("Debug")]
    public bool debug = false;
    
    // Animation state tracking
    private bool isMoving = false;
    private bool wasMoving = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        entityData = GetComponent<EntityData>();
        frameAnimator = GetComponent<FrameBasedPlayerAnimator>();
        
        if (entityData == null)
        {
            if(debug) Debug.LogError("PlayerMovement: Missing EntityData");
        }
        else
        {
            if(debug) Debug.Log($"PlayerMovement: EntityData found with speed {entityData.currentSpeed}");
        }
        
        if (frameAnimator == null && enableMovementAnimations)
        {
            if(debug) Debug.LogWarning("PlayerMovement: FrameBasedPlayerAnimator not found - movement animations disabled");
        }
    }

    void Start()
    {
        // Try to get EntityData again in Start in case it wasn't ready in Awake
        if (entityData == null)
        {
            entityData = GetComponent<EntityData>();
            if (entityData != null && debug)
            {
                Debug.Log($"PlayerMovement: EntityData found in Start with speed {entityData.currentSpeed}");
            }
        }
        
        // Start running animation immediately when player spawns
        if (enableMovementAnimations && frameAnimator != null)
        {
            frameAnimator.PlayRunAnimation();
            isMoving = true;
            wasMoving = true;
            if (debug) Debug.Log("[PlayerMovement] Started running animation on spawn");
        }
    }

    void Update()
    {
        if (!CanMove) 
        {
            // Player is not allowed to move, stop running animation
            if (isMoving && enableMovementAnimations)
            {
                StopRunningAnimation();
            }
            return;
        }

        // Get movement speed with fallback
        float moveSpeed = GetMovementSpeed();
        
        if (moveSpeed <= 0)
        {
            if (debug) Debug.LogWarning("PlayerMovement: Move speed is 0 or negative");
            return;
        }

        // auto-move to the right
        Vector3 move = Vector3.right * moveSpeed * Time.deltaTime;
        controller.Move(move);

        // Update movement state for animation
        UpdateMovementAnimation(moveSpeed);

        if (debug)
        {
            if (!CanMove) Debug.LogWarning("PlayerMovement: Can't move (CanMove = false)");
            else if (entityData == null) Debug.LogWarning("PlayerMovement: EntityData missing");
            else if (entityData.currentSpeed <= 0) Debug.LogWarning($"PlayerMovement: Speed invalid ({entityData.currentSpeed})");
        }
    }

    private float GetMovementSpeed()
    {
        if (entityData != null && entityData.currentSpeed > 0)
        {
            return entityData.currentSpeed;
        }
        
        // Use fallback speed if EntityData is missing or speed is invalid
        if (debug) Debug.LogWarning("PlayerMovement: Using fallback speed due to missing/invalid EntityData");
        return fallbackMoveSpeed;
    }

    public void ResetToSpawn(Transform spawnPoint)
    {
        if (spawnPoint == null) 
        {
            if (debug) Debug.LogError("PlayerMovement: ResetToSpawn called with null spawnPoint");
            return;
        }

        if (debug) Debug.Log($"PlayerMovement: Resetting to spawn at {spawnPoint.position}");

        // disable movement 
        CanMove = false;

        // teleport player to spawn position and rotation
        controller.enabled = false;  
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        controller.enabled = true;

        // Wait a frame before re-enabling movement to ensure everything is properly set
        StartCoroutine(ReEnableMovementAfterFrame(0.1f));
    }

    private System.Collections.IEnumerator ReEnableMovementAfterFrame(float frameDelay)
    {
        yield return new WaitForSecondsRealtime(frameDelay); 
        
        // Re-enable movement
        CanMove = true;
        
        // Start running animation when movement is re-enabled
        if (enableMovementAnimations && frameAnimator != null)
        {
            frameAnimator.PlayRunAnimation();
            isMoving = true;
            wasMoving = true;
            if (debug) Debug.Log("[PlayerMovement] Started running animation after spawn reset");
        }
        
        if (debug) Debug.Log("PlayerMovement: Movement re-enabled after spawn reset");
    }

    // Public method to check if movement is working properly
    public bool IsMovementWorking()
    {
        return CanMove && (entityData != null || fallbackMoveSpeed > 0);
    }

    // Public method to get current movement speed for debugging
    public float GetCurrentSpeed()
    {
        return GetMovementSpeed();
    }

    /// <summary>
    /// Update movement animation based on current movement state
    /// </summary>
    private void UpdateMovementAnimation(float moveSpeed)
    {
        if (!enableMovementAnimations || frameAnimator == null) return;

        // Determine if player is moving based on speed
        isMoving = moveSpeed > movementThreshold;

        // Handle animation state changes
        if (isMoving && !wasMoving)
        {
            // Started moving - play running animation
            StartRunningAnimation();
        }
        else if (!isMoving && wasMoving)
        {
            // Stopped moving - stop animation
            StopRunningAnimation();
        }

        // Update previous state
        wasMoving = isMoving;
    }

    /// <summary>
    /// Start the running animation
    /// </summary>
    private void StartRunningAnimation()
    {
        if (frameAnimator != null)
        {
            frameAnimator.PlayRunAnimation();
            if (debug) Debug.Log("[PlayerMovement] Started running animation");
        }
    }

    /// <summary>
    /// Stop the running animation
    /// </summary>
    private void StopRunningAnimation()
    {
        if (frameAnimator != null)
        {
            frameAnimator.StopCurrentAnimation();
            if (debug) Debug.Log("[PlayerMovement] Stopped running animation");
        }
    }

    /// <summary>
    /// Force stop all movement animations (useful for combat, death, etc.)
    /// </summary>
    public void ForceStopMovementAnimation()
    {
        if (frameAnimator != null)
        {
            frameAnimator.StopCurrentAnimation();
            if (debug) Debug.Log("[PlayerMovement] Force stopped movement animation");
        }
    }

    /// <summary>
    /// Check if the player is currently moving
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }
}