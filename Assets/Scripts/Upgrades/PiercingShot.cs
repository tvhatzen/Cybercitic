using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Piercing Shot", menuName = "Scriptable Objects/Skills/Piercing Shot")]
public class PiercingShot : Skill
{
    [Header("Piercing Shot Specific")]
    [SerializeField] private int additionalDamage = 75; // High damage bonus
    [SerializeField] private float pierceRange = 10f; // How far the shot travels
    [SerializeField] private float pierceWidth = 0.5f; // Width of the piercing line
    [SerializeField] private LayerMask enemyLayerMask = -1; // What layers can be hit

    protected override void ApplySkillEffects()
    {
        base.ApplySkillEffects();
        
        if(debug) Debug.Log($"Piercing Shot activated! Firing high-damage shot with {skillDamage + additionalDamage} damage!");
        
        // Fire piercing shot
        FirePiercingShot();
    }
    
    protected override void PlaySkillParticleEffect()
    {
        if (skillEffect != null && PlayerInstance.Instance != null)
        {
            // Get player position and forward direction
            Vector3 playerPos = PlayerInstance.Instance.transform.position;
            Vector3 playerForward = PlayerInstance.Instance.transform.forward;
            
            // Find the closest enemy to aim at
            Transform targetEnemy = FindClosestEnemy();
            if (targetEnemy != null)
            {
                playerForward = (targetEnemy.position - playerPos).normalized;
                if(debug) Debug.Log($"[PiercingShot] Aiming particle effect at enemy: {targetEnemy.name}");
            }
            
            // Instantiate particle effect oriented toward target
            ParticleSystem effect = Instantiate(skillEffect, playerPos, Quaternion.LookRotation(playerForward));
            effect.Play();
            
            if(debug) Debug.Log("[PiercingShot] Particle effect started");
            
            // Destroy the effect after a reasonable time
            Destroy(effect.gameObject, 5f);
        }
        else
        {
            if(debug) Debug.LogWarning("[PiercingShot] No particle effect assigned or PlayerInstance not found");
        }
    }

    private void FirePiercingShot()
    {
        // Get player position and forward direction
        Vector3 playerPos = Vector3.zero;
        Vector3 playerForward = Vector3.forward;
        
        // Try to get player position from PlayerInstance
        if (PlayerInstance.Instance != null)
        {
            playerPos = PlayerInstance.Instance.transform.position;
            playerForward = PlayerInstance.Instance.transform.forward;
        }
        
        // Find the closest enemy to aim at
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
        
        // Cast a line to find all enemies in the piercing path
        RaycastHit[] hits = Physics.BoxCastAll(
            playerPos, 
            Vector3.one * pierceWidth, 
            playerForward, 
            Quaternion.identity, 
            pierceRange, 
            enemyLayerMask
        );
        
        if(debug) Debug.Log($"[PiercingShot] Found {hits.Length} enemies in piercing path");
        
        // Apply damage to all enemies hit
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
        
        // Note: Particle effect is handled in PlaySkillParticleEffect()
    }
    
    private Transform FindClosestEnemy()
    {
        // Get player position
        Vector3 playerPos = Vector3.zero;
        if (PlayerInstance.Instance != null)
        {
            playerPos = PlayerInstance.Instance.transform.position;
        }
        
        // Find all enemies in range
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
