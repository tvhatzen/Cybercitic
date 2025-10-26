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

    protected override void ApplySkillEffects()
    {
        base.ApplySkillEffects();
        
        if(debug) Debug.Log($"Shield activated! Providing {damageReduction * 100}% damage reduction for {shieldDuration} seconds!");
        
        // Apply shield effect
        ApplyShield();
    }
    
    private void ApplyShield()
    {
        // Get player's EntityData component
        if (PlayerInstance.Instance != null)
        {
            playerEntityData = PlayerInstance.Instance.GetComponent<EntityData>();
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
                
                isShieldActive = true;
                
                if(debug) Debug.Log($"[Shield] Applied shield! Defense: {originalDefense:F2} -> {playerEntityData.currentDefense:F2} (Added {damageReduction * 100}% reduction)");
                
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
                if(debug) Debug.LogError("[Shield] Player EntityData not found!");
            }
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
        if (isShieldActive && playerEntityData != null)
        {
            // Restore original defense value
            playerEntityData.currentDefense = originalDefense;
            isShieldActive = false;
            
            if(debug) Debug.Log($"[Shield] Shield expired! Defense restored to {originalDefense:F2}");
            
            // Reset the flag so shield can be used again
            hasOriginalDefenseBeenSaved = false;
            
            // Remove visual effect
            RemoveShieldEffect();
        }
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
