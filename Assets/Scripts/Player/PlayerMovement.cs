using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    /// <summary> Whether movement is allowed. PlayerCombat can toggle this. </summary>
    public bool CanMove { get; set; } = true;

    private CharacterController controller;
    private EntityStats stats;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<EntityStats>();
    }

    void Update()
    {
        if (!CanMove) return;

        // auto-move to the right
        var moveSpeed = stats.speed;

        Vector3 move = Vector3.right * moveSpeed * Time.deltaTime;
        controller.Move(move);
    }

    public void ResetToSpawn(Transform spawnPoint)
    {
        if (spawnPoint == null) return;

        // disable movement 
        CanMove = false;

        // teleport player to spawn position and rotation
        controller.enabled = false;  
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        controller.enabled = true;

        // re-enable movement 
        CanMove = true;
    }
}
