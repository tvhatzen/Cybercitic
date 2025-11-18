using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Shield", menuName = "Scriptable Objects/Skills/Shield")]
public class Shield : Skill
{
    [Header("Shield Specific")]
    [SerializeField] private float damageReduction = 0.5f; // 50% damage reduction
    [SerializeField] private float shieldDuration = 8f; // How long the shield lasts
    public GameObject shieldPrefab; // Shield GameObject with ShieldAnim component
    [SerializeField] private Vector3 shieldScale = Vector3.one; // Scale of the shield sprite (adjust to resize)

    private bool isShieldActive = false;
    private float originalDefense = 0f;
    private EntityData playerEntityData;
    private bool hasOriginalDefenseBeenSaved = false;
    private HealthSystem playerHealthSystem;
    private ShieldAnim currentShieldAnim; // Reference to the active shield animation

    protected override void ApplySkillEffects()
    {
        // Log immediately to verify this method is being called
        Debug.LogError("[Shield] ===== SHIELD ApplySkillEffects() OVERRIDE CALLED =====");
        
        try
        {
            
            // Play sound effect
            if (skillSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(skillSound);
            }
            
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
    
    // Override to prevent particle effects from showing (we use sprite animation instead)
    protected override void PlaySkillParticleEffect()
    {
        // Do nothing - we use sprite frame animation instead of particles
        if(debug) Debug.Log("[Shield] Skipping particle effect - using sprite animation instead");
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
        if (PlayerInstance.Instance == null)
        {
            Debug.LogWarning("[Shield] PlayerInstance not found - cannot create shield effect");
            return;
        }

        // Remove any existing shield animation first
        if (currentShieldAnim != null)
        {
            currentShieldAnim.StopShieldAnimation();
            // Destroy the old shield object
            if (currentShieldAnim.gameObject != null)
            {
                Destroy(currentShieldAnim.gameObject);
            }
            currentShieldAnim = null;
        }

        if (shieldPrefab != null)
        {
            // Instantiate the shield prefab as a child of the player
            GameObject shieldObj = Instantiate(shieldPrefab, PlayerInstance.Instance.transform);
            shieldObj.name = "ShieldEffect"; // Use consistent name for easy finding
            
            // Handle RectTransform conversion if needed (UI to world space)
            // Note: When parenting a RectTransform to a regular Transform, Unity handles it,
            // but we ensure the SpriteRenderer works correctly for world space rendering
            RectTransform rectTransform = shieldObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Convert RectTransform size to scale if needed
                // For world space sprites, we'll use the regular transform
                // The RectTransform won't interfere with world space rendering
            }
            
            // Reset transform to ensure proper positioning
            shieldObj.transform.localPosition = Vector3.zero;
            shieldObj.transform.localRotation = Quaternion.identity;
            shieldObj.transform.localScale = shieldScale; // Apply custom scale
            
            // Ensure the GameObject is active
            shieldObj.SetActive(true);
            
            // Get ShieldAnim component (should already be on the prefab)
            currentShieldAnim = shieldObj.GetComponent<ShieldAnim>();
            if (currentShieldAnim == null)
            {
                Debug.LogWarning("[Shield] ShieldAnim component not found on prefab, adding it");
                currentShieldAnim = shieldObj.AddComponent<ShieldAnim>();
            }
            
            // Ensure SpriteRenderer is properly configured for world space
            SpriteRenderer spriteRenderer = shieldObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = shieldObj.AddComponent<SpriteRenderer>();
            }
            // Set sorting order so shield appears in front of player
            spriteRenderer.sortingOrder = 10;
            spriteRenderer.sortingLayerName = "Default";
            
            // Verify the sprite array is populated
            if (currentShieldAnim.shieldAnim == null || currentShieldAnim.shieldAnim.Length == 0)
            {
                Debug.LogWarning("[Shield] ShieldAnim sprite array is empty! Make sure sprites are assigned in the prefab.");
            }
            else
            {
                // Start the animation
                currentShieldAnim.AnimateShield();
                Debug.Log($"[Shield] Shield frame animation started with {currentShieldAnim.shieldAnim.Length} sprites");
            }
        }
        else
        {
            Debug.LogWarning("[Shield] Shield prefab is not assigned in the ScriptableObject! Please assign the Shield prefab in the Inspector.");
            
            // Create a new GameObject with ShieldAnim component as fallback
            GameObject shieldObj = new GameObject("ShieldEffect");
            shieldObj.transform.SetParent(PlayerInstance.Instance.transform);
            shieldObj.transform.localPosition = Vector3.zero;
            shieldObj.transform.localRotation = Quaternion.identity;
            shieldObj.transform.localScale = shieldScale; // Apply custom scale
            
            currentShieldAnim = shieldObj.AddComponent<ShieldAnim>();
            Debug.LogWarning("[Shield] Created fallback Shield GameObject - sprites need to be assigned!");
        }
    }
    
    private void RemoveShieldEffect()
    {
        // Stop the shield animation and destroy the GameObject
        if (currentShieldAnim != null)
        {
            currentShieldAnim.StopShieldAnimation();
            Debug.Log("[Shield] Shield animation stopped");
            
            // Always destroy the instantiated shield object
            if (currentShieldAnim.gameObject != null)
            {
                Destroy(currentShieldAnim.gameObject);
                Debug.Log("[Shield] Shield GameObject destroyed");
            }
            
            currentShieldAnim = null;
        }
        
        // Fallback: Try to find and remove any shield effect by name
        if (PlayerInstance.Instance != null)
        {
            Transform shieldEffectTransform = PlayerInstance.Instance.transform.Find("ShieldEffect");
            if (shieldEffectTransform != null)
            {
                ShieldAnim anim = shieldEffectTransform.GetComponent<ShieldAnim>();
                if (anim != null)
                {
                    anim.StopShieldAnimation();
                }
                
                Destroy(shieldEffectTransform.gameObject);
                Debug.Log("[Shield] Shield visual effect removed (fallback cleanup)");
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
