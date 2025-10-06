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
        
        Debug.Log($"Sweeping Strike activated! Sweeping {sweepAngle} degrees with {skillDamage + additionalDamage} damage!");
        
        // Apply sweeping damage to enemies in arc
        ApplySweepingDamage();
    }

    private void ApplySweepingDamage()
    {
        // Get player position and forward direction
        Vector3 playerPos = Vector3.zero; // PlayerSkills.Instance.transform.position 
        Vector3 playerForward = Vector3.forward; // PlayerSkills.Instance.transform.forward
        
        // Find all enemies in range
        Collider[] enemies = Physics.OverlapSphere(playerPos, skillRange, LayerMask.GetMask("Enemy"));
        
        foreach (var enemy in enemies)
        {
            Vector3 directionToEnemy = (enemy.transform.position - playerPos).normalized;
            
            // Check if enemy is within the sweep angle
            float angle = Vector3.Angle(playerForward, directionToEnemy);
            if (angle <= sweepAngle / 2f)
            {
                HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                {
                    int totalDamage = skillDamage + additionalDamage;
                    enemyHealth.TakeDamage(totalDamage);
                    Debug.Log($"Sweeping Strike dealt {totalDamage} damage to {enemy.name}");
                }
            }
        }
    }
}
