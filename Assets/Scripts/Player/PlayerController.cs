using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public LayerMask enemyLayer;
    public float combatCheckRadius = 2f;

    private CharacterController controller;
    private Transform currentEnemy;

    private bool inCombat;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!inCombat)
        {
            // left to right auto move
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }

        CheckForCombat();
    }

    void CheckForCombat()
    {
        // look for enemies inside combat radius
        Collider[] hits = Physics.OverlapSphere(transform.position, combatCheckRadius, enemyLayer);

        if (hits.Length > 0)
        {
            if (!inCombat)
            {
                // enter combat
                inCombat = true;
                currentEnemy = hits[0].transform;
                GameEvents.PlayerEnteredCombat(currentEnemy);
            }

            // attack automatically
            GameEvents.PlayerAttack(currentEnemy);
        }
        else
        {
            if (inCombat)
            {
                // leave combat when player cant attack anyymore
                inCombat = false;
                currentEnemy = null;
                GameEvents.PlayerExitedCombat();
            }
        }
    }

    // debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatCheckRadius);
    }
}
