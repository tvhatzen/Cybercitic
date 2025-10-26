using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Shield", menuName = "Scriptable Objects/Skills/Shield")]
public class Shield : Skill
{
    [Header("Shield Specific")]
    [SerializeField] private float damageReduction = 0.5f; // 50% damage reduction
    [SerializeField] private float shieldDuration = 8f; // How long the shield lasts
    [SerializeField] private ParticleSystem shieldEffect; // Visual effect for the shield
    
    private bool isShieldActive = false;
    private float originalDefense = 0f;
    private EntityData playerEntityData;
    
    protected override void ApplySkillEffects()
    {
        base.ApplySkillEffects();
        
        if(debug) Debug.Log($"Shield activated! Providing {damageReduction * 100}% damage reduction for {shieldDuration} seconds!");
        
        // Apply shield effect
        ApplyShield();
    }
    
    protected override void PlaySkillParticleEffect()
    {
        // Shield uses a different particle effect system (attached to player)
        // The base particle effect is not used for shield
        if(debug) Debug.Log("[Shield] Using custom shield particle effect system");
    }
    
    private void ApplyShield()
    {
        // Get player's EntityData component
        if (PlayerInstance.Instance != null)
        {
            playerEntityData = PlayerInstance.Instance.GetComponent<EntityData>();
            if (playerEntityData != null)
            {
                // Store original defense value
                originalDefense = playerEntityData.currentDefense;
                
                // Apply shield damage reduction
                playerEntityData.currentDefense = Mathf.Min(1f, originalDefense + damageReduction);
                
                isShieldActive = true;
                
                if(debug) Debug.Log($"[Shield] Applied shield! Defense: {originalDefense:F2} -> {playerEntityData.currentDefense:F2}");
                
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
            
            // Remove visual effect
            RemoveShieldEffect();
        }
    }
    
    private void CreateShieldEffect()
    {
        if (PlayerInstance.Instance != null)
        {
            // Use skillEffect from base class if available, otherwise use shieldEffect
            ParticleSystem effectPrefab = skillEffect != null ? skillEffect : shieldEffect;
            
            if (effectPrefab != null)
            {
                // Instantiate shield visual effect on player
                ParticleSystem shield = Instantiate(effectPrefab, PlayerInstance.Instance.transform);
                shield.name = "ShieldEffect";
                
                // Play the particle system
                shield.Play();
                if(debug) Debug.Log("[Shield] Shield particle effect started");
                
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
        if (PlayerInstance.Instance != null)
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
        
        base.ResetCooldown();
    }
}
