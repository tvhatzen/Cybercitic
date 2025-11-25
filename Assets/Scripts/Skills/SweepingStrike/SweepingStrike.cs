using UnityEngine;
using System.Collections;
using Game.Skills;

namespace Game.Skills
{
    [CreateAssetMenu(fileName = "Sweeping Strike", menuName = "Scriptable Objects/Skills/Sweeping Strike")]
    public class SweepingStrike : Skill, ISkillEffect
    {
        [Header("Sweeping Strike Specific")]
        [SerializeField] private float sweepAngle = 120f;
        [SerializeField] private int additionalDamage = 25;

        public void ApplyEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            if (Debug)
            {
                UnityEngine.Debug.Log($"Sweeping Strike activated! Sweeping {sweepAngle} degrees with {SkillDamage + additionalDamage} damage!");
            }
            
            // Apply sweeping damage to enemies in arc
            ApplySweepingDamage(dependencies);
        }

        public void CleanupEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            // No cleanup needed for sweeping strike
        }

        public Quaternion GetEffectRotation(Vector3 playerForward, Quaternion defaultRotation)
        {
            // Use default rotation (forward direction)
            return defaultRotation;
        }

        private void ApplySweepingDamage(ISkillDependencies dependencies)
        {
            if (dependencies?.PlayerTransform == null)
            {
                if (Debug) UnityEngine.Debug.LogWarning("[SweepingStrike] Cannot apply damage - player transform is null!");
                return;
            }

            // Get player position and forward direction
            Vector3 playerPos = dependencies.PlayerTransform.position;
            Vector3 playerForward = dependencies.PlayerTransform.forward;
            
            // Find all enemies in range
            Collider[] enemies = Physics.OverlapSphere(playerPos, SkillRange, LayerMask.GetMask("Enemy"));
            
            foreach (var enemy in enemies)
            {
                Vector3 directionToEnemy = (enemy.transform.position - playerPos).normalized;
                
                // Check if enemy is within the sweep angle
                float angle = Vector3.Angle(playerForward, directionToEnemy);
                if (angle <= sweepAngle / 2f)
                {
                    // Use reflection to find HealthSystem component
                    var enemyHealth = enemy.GetComponent<MonoBehaviour>();
                    if (enemyHealth != null)
                    {
                        var takeDamageMethod = enemyHealth.GetType().GetMethod("TakeDamage",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (takeDamageMethod != null)
                        {
                            int totalDamage = SkillDamage + additionalDamage;
                            takeDamageMethod.Invoke(enemyHealth, new object[] { totalDamage });
                            if (Debug)
                            {
                                UnityEngine.Debug.Log($"Sweeping Strike dealt {totalDamage} damage to {enemy.name}");
                            }
                        }
                    }
                }
            }
        }
    }
}
