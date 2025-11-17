using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Shield", menuName = "Scriptable Objects/Skills/Shield")]
public class Shield : Skill
{
    [Header("Shield Specific")]
    [SerializeField] private float damageReduction = 0.5f; // 50% damage reduction
    [SerializeField] private float shieldDuration = 8f; // How long the shield lasts
    [SerializeField] private ParticleSystem shieldEffect; // Visual effect for the shield
    [SerializeField] private GameObject shieldPrefab;

    private bool isShieldActive = false;
    private float originalDefense = 0f;
    private EntityData playerEntityData;
    private bool hasOriginalDefenseBeenSaved = false;
    private HealthSystem playerHealthSystem;

    protected override void ApplySkillEffects()
    {
        // Log immediately to verify this method is being called
        Debug.LogError("[Shield] ===== SHIELD ApplySkillEffects() OVERRIDE CALLED =====");
        
        try
        {
            base.ApplySkillEffects();
            
            // Always log this critical operation (debug flag may not work on ScriptableObjects)
            Debug.Log($"[Shield] ApplySkillEffects called! Providing {damageReduction * 100}% damage reduction for {shieldDuration} seconds!");
            
            // Apply shield effect
            ApplyShield();
            
            // Verify shield was applied - always check this
            if (playerHealthSystem != null)
            {
                bool isImmune = playerHealthSystem.IsShieldImmune;
                Debug.Log($"[Shield] Verification - playerHealthSystem.IsShieldImmune = {isImmune}");
                if (!isImmune)
                {
                    Debug.LogError("[Shield] WARNING: Shield immunity was NOT set correctly!");
                }
            }
            else
            {
                Debug.LogError("[Shield] ERROR: playerHealthSystem is NULL after ApplyShield()!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Shield] EXCEPTION in ApplySkillEffects: {e.Message}\n{e.StackTrace}");
        }
    }
    
    public void ApplyShield()
    {
        // Get player's components - always get fresh references
        if (PlayerInstance.Instance != null)
        {
            // Always get fresh references to ensure we have the current player instance
            // (in case player was respawned or recreated)
            GameObject player = PlayerInstance.Instance.gameObject;
            playerEntityData = player.GetComponent<EntityData>();
            playerHealthSystem = player.GetComponent<HealthSystem>();
            
            Debug.Log($"[Shield] Looking for HealthSystem on {player.name}...");
            
            // Set shield immunity FIRST - this is critical!
            // Always log these critical operations (debug flag may not work on ScriptableObjects)
            if (playerHealthSystem != null)
            {
                playerHealthSystem.SetShieldImmunity(true);
                isShieldActive = true;
                Debug.Log($"[Shield] Player shield immunity enabled on {PlayerInstance.Instance.name} (HealthSystem found directly)");
                
                // Double-check it was set
                if (playerHealthSystem.IsShieldImmune)
                {
                    Debug.Log($"[Shield] Confirmed: shieldImmunityActive is now TRUE");
                }
                else
                {
                    Debug.LogError($"[Shield] ERROR: SetShieldImmunity(true) was called but IsShieldImmune is still FALSE!");
                }
            }
            else
            {
                // Try to find HealthSystem in children as fallback
                playerHealthSystem = PlayerInstance.Instance.GetComponentInChildren<HealthSystem>();
                if (playerHealthSystem != null)
                {
                    playerHealthSystem.SetShieldImmunity(true);
                    isShieldActive = true;
                    Debug.Log($"[Shield] Found HealthSystem in children, shield immunity enabled on {PlayerInstance.Instance.name}");
                    
                    // Double-check it was set
                    if (playerHealthSystem.IsShieldImmune)
                    {
                        Debug.Log($"[Shield] Confirmed: shieldImmunityActive is now TRUE (found in children)");
                    }
                    else
                    {
                        Debug.LogError($"[Shield] ERROR: SetShieldImmunity(true) was called but IsShieldImmune is still FALSE!");
                    }
                }
                else
                {
                    Debug.LogError($"[Shield] Player HealthSystem not found on {PlayerInstance.Instance.name} or its children! Shield immunity will not work!");
                    Debug.LogError($"[Shield] PlayerInstance has {PlayerInstance.Instance.GetComponents<Component>().Length} components");
                }
            }
            
            // Apply defense boost if EntityData exists
            if (playerEntityData != null)
            {
                // Store original defense value only once
                if (!hasOriginalDefenseBeenSaved)
                {
                    originalDefense = playerEntityData.currentDefense;
                    hasOriginalDefenseBeenSaved = true;
                    if(debug) Debug.Log($"[Shield] Original defense saved: {originalDefense:F2}");
                }
                
                // Apply shield damage reduction by setting defense to our damage reduction value
                float shieldDefense = originalDefense + damageReduction;
                shieldDefense = Mathf.Clamp01(shieldDefense);
                
                playerEntityData.currentDefense = shieldDefense;
                
                if(debug) Debug.Log($"[Shield] Applied shield! Defense: {originalDefense:F2} -> {playerEntityData.currentDefense:F2} (Added {damageReduction * 100}% reduction)");
            }
            else
            {
                if(debug) Debug.LogWarning("[Shield] Player EntityData not found! Defense boost will not apply, but immunity should still work.");
            }
            
            // Start shield duration timer
            if (PlayerSkills.Instance != null)
            {
                PlayerSkills.Instance.StartCoroutine(ShieldDurationTimer());
            }
            
            // Create visual effect
            CreateShieldEffect();
        }
        else
        {
            if(debug) Debug.LogError("[Shield] PlayerInstance not found!");
        }
    }
    
    private IEnumerator ShieldDurationTimer()
    {
        if(debug) Debug.Log($"[Shield] Shield duration timer started for {shieldDuration} seconds");
        
        yield return new WaitForSeconds(shieldDuration);
        
        // Remove shield effect
        RemoveShield();
    }
    
    private void RemoveShield()
    {
        if (!isShieldActive) return;
        
        // Always disable shield immunity first, regardless of other components
        if (playerHealthSystem != null)
        {
            playerHealthSystem.SetShieldImmunity(false);
            if (debug) Debug.Log("[Shield] Player shield immunity disabled");
        }
        else
        {
            if (debug) Debug.LogWarning("[Shield] Player HealthSystem not found when removing shield!");
        }
        
        // Restore original defense value if EntityData exists
        if (playerEntityData != null)
        {
            playerEntityData.currentDefense = originalDefense;
            if(debug) Debug.Log($"[Shield] Shield expired! Defense restored to {originalDefense:F2}");
        }
        
        isShieldActive = false;
        
        // Reset the flag so shield can be used again
        hasOriginalDefenseBeenSaved = false;
        
        // Remove visual effect
        RemoveShieldEffect();
    }
    
    private void CreateShieldEffect()
    {
        if (shieldPrefab != null)
        {
            // Activate the shield prefab if it exists
            shieldPrefab.SetActive(true);
            if(debug) Debug.Log("[Shield] Shield visual effect activated");
        }
        else if (PlayerInstance.Instance != null)
        {
            // Use skillEffect from base class if available, otherwise use shieldEffect
            ParticleSystem effectPrefab = skillEffect != null ? skillEffect : shieldEffect;
            
            if (effectPrefab != null)
            {
                // Instantiate shield visual effect on player
                ParticleSystem shield = Instantiate(effectPrefab, PlayerInstance.Instance.transform);
                shield.name = "ShieldEffect";
                
                if(debug) Debug.Log("[Shield] Shield visual effect created");
            }
            else
            {
                if(debug) Debug.LogWarning("[Shield] No shield effect prefab assigned");
            }
        }
        else
        {
            if(debug) Debug.LogWarning("[Shield] PlayerInstance not found");
        }
    }
    
    private void RemoveShieldEffect()
    {
        // Deactivate the shield prefab if it exists
        if (shieldPrefab != null)
        {
            shieldPrefab.SetActive(false);
            if(debug) Debug.Log("[Shield] Shield prefab deactivated");
        }
        else if (PlayerInstance.Instance != null)
        {
            // Find and destroy shield effect
            Transform shieldEffectTransform = PlayerInstance.Instance.transform.Find("ShieldEffect");
            if (shieldEffectTransform != null)
            {
                // Stop the particle system before destroying
                ParticleSystem shieldParticles = shieldEffectTransform.GetComponent<ParticleSystem>();
                if (shieldParticles != null)
                {
                    shieldParticles.Stop();
                }
                
                Destroy(shieldEffectTransform.gameObject);
                if(debug) Debug.Log("[Shield] Shield visual effect removed");
            }
        }
    }
    
    // Override FinishSkill to ensure shield is removed when skill ends
    protected override void FinishSkill()
    {
        // Remove shield if it's still active
        if (isShieldActive)
        {
            RemoveShield();
        }
        
        base.FinishSkill();
    }
    
    // Override ResetCooldown to ensure shield is removed when skill is reset
    public override void ResetCooldown()
    {
        // Remove shield if it's still active
        if (isShieldActive)
        {
            RemoveShield();
        }
        
        // Reset the flag
        hasOriginalDefenseBeenSaved = false;
        
        base.ResetCooldown();
    }
}
