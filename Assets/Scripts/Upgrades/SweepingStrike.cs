using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Sweeping Strike", menuName = "Scriptable Objects/Skills/Sweeping Strike")]
public class SweepingStrike : Skill
{
    [Header("Sweeping Strike Specific")]
    [SerializeField] private float sweepAngle = 120f;
    [SerializeField] private int additionalDamage = 25;

    protected override void ApplySkillEffects()
    {
        base.ApplySkillEffects();
        
        if(debug) Debug.Log($"Sweeping Strike activated! Sweeping {sweepAngle} degrees with {skillDamage + additionalDamage} damage!");
        
        // apply sweeping damage to enemies in arc
        ApplySweepingDamage();
    }
    
    protected override void PlaySkillParticleEffect()
    {
        if (skillEffect != null && PlayerInstance.Instance != null)
        {
            // Get player position and forward direction
            Vector3 playerPos = PlayerInstance.Instance.transform.position;
            Vector3 playerForward = PlayerInstance.Instance.transform.forward;
            
            // Instantiate particle effect at player position, oriented forward
            ParticleSystem effect = Instantiate(skillEffect, playerPos, Quaternion.LookRotation(playerForward));
            effect.Play();
            
            if(debug) Debug.Log("[SweepingStrike] Particle effect started");
            
            // Destroy the effect
            Destroy(effect.gameObject, 5f);
        }
        else
        {
            if(debug) Debug.LogWarning("[SweepingStrike] No particle effect assigned or PlayerInstance not found");
        }
    }

    private void ApplySweepingDamage()
    {
        // get player position and forward direction
        Vector3 playerPos = Vector3.zero; 
        Vector3 playerForward = Vector3.forward; 
        
        // find all enemies in range
        Collider[] enemies = Physics.OverlapSphere(playerPos, skillRange, LayerMask.GetMask("Enemy"));
        
        foreach (var enemy in enemies)
        {
            Vector3 directionToEnemy = (enemy.transform.position - playerPos).normalized;
            
            // check if enemy is within the sweep angle
            float angle = Vector3.Angle(playerForward, directionToEnemy);
            if (angle <= sweepAngle / 2f)
            {
                HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                {
                    int totalDamage = skillDamage + additionalDamage;
                    enemyHealth.TakeDamage(totalDamage);
                    if(debug) Debug.Log($"Sweeping Strike dealt {totalDamage} damage to {enemy.name}");
                }
            }
        }
    }
}
