using System.Collections;
using UnityEngine;
using Game.Skills;

namespace Game.Skills
{
/// <summary>
    /// MonoBehaviour responsible for executing skill coroutines and managing skill execution flow.
    /// Handles casting, duration, and cooldown timing.
/// </summary>
public class SkillController : MonoBehaviour
{
        private SkillInstance skillInstance;
        private ISkillDependencies dependencies;

        public void Initialize(SkillInstance instance, ISkillDependencies deps)
        {
            skillInstance = instance;
            dependencies = deps;
        }

        public void SetDependencies(ISkillDependencies deps)
        {
            dependencies = deps;
        }

        public bool ActivateSkill()
        {
            if (skillInstance == null || dependencies == null)
            {
                if (skillInstance?.SkillData.Debug ?? false)
                {
                    Debug.LogError("[SkillController] Cannot activate - instance or dependencies are null!");
                }
                return false;
            }

            if (!skillInstance.CanActivate())
            {
                if (skillInstance.SkillData.Debug)
                {
                    Debug.LogWarning($"[SkillController] Cannot activate {skillInstance.SkillData.SkillName} - State: {skillInstance.CurrentState}, Unlocked: {skillInstance.IsUnlocked}, Charges: {skillInstance.CurrentCharges}");
                }
                return false;
            }

            if (skillInstance.SkillData.Debug)
            {
                Debug.Log($"[SkillController] Activating skill: {skillInstance.SkillData.SkillName}");
            }

            skillInstance.SetCasting();
            StartCoroutine(ExecuteSkillCoroutine());

            return true;
        }

        private IEnumerator ExecuteSkillCoroutine()
        {
            var skillData = skillInstance.SkillData;
            
            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} - Starting execution (casting time: {skillData.CastingTime}s)");
            }

            // Casting phase
            yield return new WaitForSeconds(skillData.CastingTime);
            
            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} - Applying effects");
            }

            // Apply skill effects (handled by skill-specific logic)
            ApplySkillEffects();

            // Duration phase
            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} - Waiting for skill duration: {skillData.SkillDuration}s");
            }

            skillInstance.SetSkillDuration(skillData.SkillDuration);
            float elapsedTime = 0f;
            while (elapsedTime < skillData.SkillDuration)
            {
                elapsedTime += Time.deltaTime;
                skillInstance.UpdateSkillDuration(Time.deltaTime);
                yield return null;
            }

            skillInstance.SetSkillDuration(0f);
            
            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} - Finishing skill");
            }

            // Finish skill - error handling inside FinishSkill if needed
            FinishSkill();
        }

        private void ApplySkillEffects()
        {
            var skillData = skillInstance.SkillData;
            
            // Refresh dependencies if player transform is null
            if (dependencies == null || dependencies.PlayerTransform == null)
            {
                UnityEngine.Debug.LogWarning($"[SkillController] Dependencies are null or player transform missing, refreshing...");
                dependencies = SkillDependencies.FromSingletons();
            }
            
            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} effects applied!");
            }

            // Play sound effect
            if (skillData.SkillSound != null)
            {
                if (dependencies?.AudioManager != null)
                {
                    dependencies.AudioManager.PlaySound(skillData.SkillSound);
                    if (skillData.Debug)
                    {
                        Debug.Log($"[SkillController] Playing sound: {skillData.SkillSound.name}");
                    }
                }
                else
                {
                    if (skillData.Debug)
                    {
                        Debug.LogWarning($"[SkillController] AudioManager is null, cannot play sound");
                    }
                }
            }

            // Play particle effect
            try
            {
                PlaySkillParticleEffect();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[SkillController] Error playing particle effect: {e.Message}");
            }

            // Apply skill-specific effects
            try
            {
                if (skillData is ISkillEffect skillEffect)
                {
                    skillEffect.ApplyEffects(skillInstance, dependencies);
                }
                else
                {
                    // Default damage application
                    ApplyDamageToEnemies();
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[SkillController] Error applying skill-specific effects: {e.Message}\n{e.StackTrace}");
            }
        }

        private void PlaySkillParticleEffect()
        {
            var skillData = skillInstance.SkillData;
            
            if (skillData.SkillEffect == null || dependencies.PlayerTransform == null)
            {
                if (skillData.Debug)
                {
                    Debug.LogWarning($"[SkillController] {skillData.SkillName} - No particle effect assigned or player transform not found");
                }
                return;
            }

            Vector3 playerPos = dependencies.PlayerTransform.position;
            Vector3 playerForward = dependencies.PlayerTransform.forward;
            
            Quaternion effectRotation = Quaternion.LookRotation(playerForward);
            
            // Allow skill-specific rotation override
            if (skillData is ISkillEffect skillEffect)
            {
                effectRotation = skillEffect.GetEffectRotation(playerForward, effectRotation);
            }
            
            ParticleSystem effect = Instantiate(skillData.SkillEffect, playerPos, effectRotation);
            effect.Play();
            
            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} particle effect started");
            }
            
            Destroy(effect.gameObject, 5f);
        }

        private void ApplyDamageToEnemies()
        {
            var skillData = skillInstance.SkillData;
            
            if (dependencies.PlayerTransform == null)
            {
                if (skillData.Debug)
                {
                    Debug.LogWarning("[SkillController] Cannot apply damage - player transform is null!");
                }
                return;
            }

            Vector3 playerPosition = dependencies.PlayerTransform.position;
            Collider[] enemies = Physics.OverlapSphere(playerPosition, skillData.SkillRange, LayerMask.GetMask("Enemy"));
            
            foreach (var enemy in enemies)
            {
                // Try to find HealthSystem component 
                MonoBehaviour enemyHealth = null;
                
                // Try direct type lookup
                var healthSystemType = System.Type.GetType("HealthSystem") ?? 
                                      System.Type.GetType("Game.Characters.HealthSystem");
                if (healthSystemType != null)
                {
                    enemyHealth = enemy.GetComponent(healthSystemType) as MonoBehaviour;
                }
                
                // search all components for one with TakeDamage method
                if (enemyHealth == null)
                {
                    var components = enemy.GetComponents<MonoBehaviour>();
                    foreach (var comp in components)
                    {
                        if (comp.GetType().GetMethod("TakeDamage", 
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) != null)
                        {
                            enemyHealth = comp;
                            break;
                        }
                    }
                }
                
                if (enemyHealth != null)
                {
                    // Call TakeDamage via reflection 
                    var takeDamageMethod = enemyHealth.GetType().GetMethod("TakeDamage", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (takeDamageMethod != null)
                    {
                        takeDamageMethod.Invoke(enemyHealth, new object[] { skillData.SkillDamage });
                        if (skillData.Debug)
                        {
                            Debug.Log($"Dealt {skillData.SkillDamage} damage to {enemy.name}");
                        }
                    }
                }
            }
        }

        private void FinishSkill()
        {
            var skillData = skillInstance.SkillData;
            
            // Cleanup skill-specific effects
            if (skillData is ISkillEffect skillEffect)
            {
                skillEffect.CleanupEffects(skillInstance, dependencies);
            }

            skillInstance.SetSkillDuration(0f);
            skillInstance.ConsumeCharge();
            StartCooldown();
        }

        private void StartCooldown()
        {
            var skillData = skillInstance.SkillData;
            
            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} - Starting cooldown ({skillData.CooldownDuration}s)");
            }

            skillInstance.SetCooldown(skillData.CooldownDuration);
            StartCoroutine(HandleCooldownCoroutine());
        }

        private IEnumerator HandleCooldownCoroutine()
        {
            var skillData = skillInstance.SkillData;
            
            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} - Cooldown coroutine started");
            }

            while (skillInstance.CurrentCooldown > 0)
            {
                skillInstance.UpdateCooldown(Time.deltaTime);
                yield return null;
            }

            if (skillData.Debug)
            {
                Debug.Log($"[SkillController] {skillData.SkillName} - Cooldown finished");
            }
        }
    }
}
