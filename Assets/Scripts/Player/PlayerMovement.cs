using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    /// <summary> Whether movement is allowed. PlayerCombat can toggle this. </summary>
    public bool CanMove { get; set; } = true;

    private CharacterController controller;
    private EntityData entityData;
    
    [Header("Fallback Settings")]
    [SerializeField] private float fallbackMoveSpeed = 5f; // Fallback speed if EntityData is missing

    public bool debug = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        entityData = GetComponent<EntityData>();
        
        if (entityData == null)
        {
            if(debug) Debug.LogError("PlayerMovement: Missing EntityData");
        }
        else
        {
            if(debug) Debug.Log($"PlayerMovement: EntityData found with speed {entityData.currentSpeed}");
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
    }

    void Update()
    {
        if (!CanMove) return;

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
}