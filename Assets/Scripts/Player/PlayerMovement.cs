using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    /// <summary> Whether movement is allowed. PlayerCombat can toggle this. </summary>
    public bool CanMove { get; set; } = true;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!CanMove) return;

        // simple auto-move to the right
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
