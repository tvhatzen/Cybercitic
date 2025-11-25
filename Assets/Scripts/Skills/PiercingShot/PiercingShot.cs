using UnityEngine;
using System.Collections;
using Game.Skills;

namespace Game.Skills
{
    [CreateAssetMenu(fileName = "Piercing Shot", menuName = "Scriptable Objects/Skills/Piercing Shot")]
    public class PiercingShot : Skill, ISkillEffect
    {
        [Header("Piercing Shot Specific")]
        [SerializeField] private int additionalDamage = 50; // high damage bonus
        [SerializeField] private float pierceRange = 10f; // how far the shot travels
        [SerializeField] private float pierceWidth = 0.5f; // width of the piercing line
        [SerializeField] private LayerMask enemyLayerMask = -1; // what layers can be hit
        [SerializeField] private GameObject effectPrefab; // visual effect prefab (optional - if set, uses its rotation)
        [SerializeField] private float rotationY = 90f; // Y-axis rotation for particle effect

        public void ApplyEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            if (Debug)
            {
                UnityEngine.Debug.Log($"Piercing Shot activated! Firing high-damage shot with {SkillDamage + additionalDamage} damage!");
            }
            
            // Fire piercing shot
            FirePiercingShot(dependencies);
        }

        public void CleanupEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            // No cleanup needed for piercing shot
        }

        public Quaternion GetEffectRotation(Vector3 playerForward, Quaternion defaultRotation)
        {
            // Apply 90 degrees rotation on Y axis for piercing shot
            if (Debug)
            {
                UnityEngine.Debug.Log($"[PiercingShot] Using {rotationY}° Y rotation for particle effect");
            }
            return Quaternion.Euler(0, rotationY, 0);
        }
    
        private void FirePiercingShot(ISkillDependencies dependencies)
        {
            if (dependencies?.PlayerTransform == null)
            {
                if (Debug) UnityEngine.Debug.LogWarning("[PiercingShot] Cannot fire - player transform is null!");
                return;
            }

            // Get player position and rotation
            Vector3 playerPos = dependencies.PlayerTransform.position;
            Quaternion playerRotation = dependencies.PlayerTransform.rotation;
            
            // Determine the firing direction and rotation based on prefab's rotation
            // Add 90 degrees on Y axis to correct the direction (shooting forward instead of behind)
            Vector3 firingDirection = Vector3.forward;
            Quaternion effectRotation = Quaternion.identity;
            Quaternion yAxis90Rotation = Quaternion.Euler(0, rotationY, 0); // 90 degree rotation on Y axis
            
            if (effectPrefab != null)
            {
                // Use the prefab's exact rotation with 90 degree Y-axis offset
                effectRotation = effectPrefab.transform.rotation * yAxis90Rotation;
                // Get the prefab's forward direction rotated 90 degrees on Y axis
                firingDirection = effectRotation * Vector3.forward;
                if (Debug) UnityEngine.Debug.Log($"[PiercingShot] Using effect prefab rotation (with {rotationY}° Y offset): {effectRotation.eulerAngles}, forward: {firingDirection}");
            }
            else
            {
                // Fallback: use player's rotation with 90 degree Y-axis offset
                effectRotation = playerRotation * yAxis90Rotation;
                firingDirection = effectRotation * Vector3.forward;
                if (Debug) UnityEngine.Debug.Log($"[PiercingShot] Using player rotation (with {rotationY}° Y offset): {effectRotation.eulerAngles}, forward: {firingDirection}");
            }
            
            // Cast a line to find all enemies in the piercing path
            RaycastHit[] hits = Physics.BoxCastAll(
                playerPos, 
                Vector3.one * pierceWidth, 
                firingDirection, 
                effectRotation, 
                pierceRange, 
                enemyLayerMask
            );
            
            if (Debug) UnityEngine.Debug.Log($"[PiercingShot] Found {hits.Length} enemies in piercing path");
            
            // Apply damage to all enemies hit
            foreach (var hit in hits)
            {
                // Use reflection to find HealthSystem component
                var enemyHealth = hit.collider.GetComponent<MonoBehaviour>();
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
                            UnityEngine.Debug.Log($"[PiercingShot] Dealt {totalDamage} damage to {hit.collider.name}");
                        }
                    }
                }
            }
        }
    }
}
