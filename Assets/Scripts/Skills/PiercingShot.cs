using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Piercing Shot", menuName = "Scriptable Objects/Skills/Piercing Shot")]
public class PiercingShot : Skill
{
    [Header("Piercing Shot Specific")]
    [SerializeField] private int additionalDamage = 50; // high damage bonus
    [SerializeField] private float pierceRange = 10f; // how far the shot travels
    [SerializeField] private float pierceWidth = 0.5f; // width of the piercing line
    [SerializeField] private LayerMask enemyLayerMask = -1; // what layers can be hit

    protected override void ApplySkillEffects()
    {
        base.ApplySkillEffects();
        
        if(debug) Debug.Log($"Piercing Shot activated! Firing high-damage shot with {skillDamage + additionalDamage} damage!");

        // fire piercing shot
        FirePiercingShot();
    }
    
    private void FirePiercingShot()
    {
        // get player position and forward direction
        Vector3 playerPos = Vector3.zero;
        Vector3 playerForward = Vector3.forward;
        
        // try to get player position from PlayerInstance
        if (PlayerInstance.Instance != null)
        {
            playerPos = PlayerInstance.Instance.transform.position;
            playerForward = PlayerInstance.Instance.transform.forward;
        }
        
        // find the closest enemy to aim at
        Transform targetEnemy = FindClosestEnemy();
        if (targetEnemy != null)
        {
            playerForward = (targetEnemy.position - playerPos).normalized;
            if(debug) Debug.Log($"[PiercingShot] Aiming at enemy: {targetEnemy.name}");
        }
        else
        {
            if(debug) Debug.Log("[PiercingShot] No enemies found, firing in default direction");
        }
        
        // cast a line to find all enemies in the piercing path
        RaycastHit[] hits = Physics.BoxCastAll(
            playerPos, 
            Vector3.one * pierceWidth, 
            playerForward, 
            Quaternion.identity, 
            pierceRange, 
            enemyLayerMask
        );
        
        if(debug) Debug.Log($"[PiercingShot] Found {hits.Length} enemies in piercing path");
        
        // apply damage to all enemies hit
        foreach (var hit in hits)
        {
            HealthSystem enemyHealth = hit.collider.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                int totalDamage = skillDamage + additionalDamage;
                enemyHealth.TakeDamage(totalDamage);
                if(debug) Debug.Log($"[PiercingShot] Dealt {totalDamage} damage to {hit.collider.name}");
            }
        }
    }
    
    private Transform FindClosestEnemy()
    {
        // get player position
        Vector3 playerPos = Vector3.zero;
        if (PlayerInstance.Instance != null)
        {
            playerPos = PlayerInstance.Instance.transform.position;
        }
        
        // find all enemies in range
        Collider[] enemies = Physics.OverlapSphere(playerPos, skillRange, LayerMask.GetMask("Enemy"));
        
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(playerPos, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }
        return closestEnemy;
    }
}
