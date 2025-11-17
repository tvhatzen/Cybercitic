using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
    #region Variables

    [Header("Skill Info")]
    [SerializeField] private string skillName;
    [SerializeField] private string skillDescription;
    [SerializeField] private Sprite skillIcon;
    [SerializeField] private Sprite castIcon;
    
    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private float castingTime = 1f;
    [SerializeField] private float skillDuration = 2f;
    
    [Header("Skill Effects")]
    public int skillDamage = 50;
    public float skillRange = 5f;
    [SerializeField] private int maxCharges = 1;
    
    [Header("Audio/Visual")]
    [SerializeField] private AudioClip skillSound;
    [SerializeField] protected ParticleSystem skillEffect;
    
    // runtime data
    [NonSerialized] public SkillStates currentState = SkillStates.ReadyToUse;
    [NonSerialized] public float currentCooldown = 0f;
    [NonSerialized] public int currentCharges = 0;
    [NonSerialized] public bool isUnlocked = false;
    [NonSerialized] public bool hasBeenUsed = false; // track if skill has been used this run

    #endregion

    #region Events, Enum & Properties

    // events
    public event Action<Skill> OnSkillActivated;
    public event Action<Skill> OnSkillCooldownStarted;
    public event Action<Skill> OnSkillCooldownFinished;

    public enum SkillStates 
    {
        ReadyToUse,
        Casting,
        Cooldown,
        Locked
    }

    public string SkillName => skillName;
    public string Description => skillDescription;
    public Sprite Icon => currentState == SkillStates.Casting ? castIcon : skillIcon;
    public SkillStates CurrentState => currentState;
    public float CooldownDuration => cooldownDuration;
    public float CurrentCooldown => currentCooldown;
    public float CooldownProgress => currentCooldown / cooldownDuration;
    public bool IsOnCooldown => currentState == SkillStates.Cooldown;
    public bool IsReady => currentState == SkillStates.ReadyToUse && isUnlocked;
    public bool IsUnlocked => isUnlocked;

    [Header("DEBUG")]
    public bool debug = false;

    #endregion

    public virtual void Initialize()
    {
        currentState = isUnlocked ? SkillStates.ReadyToUse : SkillStates.Locked;
        currentCooldown = 0f;
        currentCharges = maxCharges;
        hasBeenUsed = false; // initialize as not used
    }

    public virtual bool Activate()
    {
        if (!CanActivate())
        {
            if(debug) Debug.LogWarning($"[Skill] Cannot activate {skillName} - State: {currentState}, Unlocked: {isUnlocked}, Charges: {currentCharges}");
            return false;
        }

        if(debug) Debug.Log($"[Skill] Activating skill: {skillName}");
        currentState = SkillStates.Casting;
        OnSkillActivated?.Invoke(this);

        // start the skill effect
        StartCoroutine(ExecuteSkill());

        return true;
    }

    protected virtual bool CanActivate()
    {
        return isUnlocked && currentState == SkillStates.ReadyToUse && currentCharges > 0 && !hasBeenUsed; 
    }

    protected virtual IEnumerator ExecuteSkill()
    {
        if(debug) Debug.Log($"[Skill] {skillName} - Starting execution (casting time: {castingTime}s)");
        yield return new WaitForSeconds(castingTime);
        
        if(debug) Debug.Log($"[Skill] {skillName} - Applying effects");
        
        // Debug: Check what type this actually is
        Debug.Log($"[Skill] DEBUG: Calling ApplySkillEffects on type: {this.GetType().Name}");
        
        ApplySkillEffects();
        
        // WORKAROUND: If this is a Shield, manually apply shield immunity directly
        // (This is a workaround in case the override isn't being called due to Unity ScriptableObject type issues)
        // Check by skill name since ScriptableObjects can lose their derived type
        if (skillName.ToLower().Contains("shield"))
        {
            Debug.LogError($"[Skill] WORKAROUND: Detected Shield skill (name: {skillName}), manually applying shield immunity!");
            try
            {
                // Directly apply shield immunity to player's HealthSystem
                if (PlayerInstance.Instance != null)
                {
                    var playerHealthSystem = PlayerInstance.Instance.GetComponent<HealthSystem>();
                    if (playerHealthSystem != null)
                    {
                        playerHealthSystem.SetShieldImmunity(true);
                        Debug.LogError($"[Skill] WORKAROUND: Shield immunity enabled on {PlayerInstance.Instance.name}!");
                        
                        // Verify it was set
                        if (playerHealthSystem.IsShieldImmune)
                        {
                            Debug.LogError("[Skill] WORKAROUND: Confirmed - shieldImmunityActive is now TRUE");
                        }
                        else
                        {
                            Debug.LogError("[Skill] WORKAROUND: ERROR - shieldImmunityActive is still FALSE after SetShieldImmunity(true)!");
                        }
                    }
                    else
                    {
                        Debug.LogError("[Skill] WORKAROUND: Player HealthSystem not found!");
                    }
                }
                else
                {
                    Debug.LogError("[Skill] WORKAROUND: PlayerInstance.Instance is null!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Skill] WORKAROUND: Exception applying shield immunity: {e.Message}\n{e.StackTrace}");
            }
        }
        
        if(debug) Debug.Log($"[Skill] {skillName} - Waiting for skill duration: {skillDuration}s");
        yield return new WaitForSeconds(skillDuration);
        
        if(debug) Debug.Log($"[Skill] {skillName} - Finishing skill");
        FinishSkill();
    }

    protected virtual void ApplySkillEffects()
    {
        if(debug) Debug.Log($"{skillName} effects applied!");
        
        // play sound effect
        if (skillSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(skillSound);
        }
        
        // play particle effect
        PlaySkillParticleEffect();
        
        // apply damage to enemies in range
        ApplyDamageToEnemies();
    }
    
    protected virtual void PlaySkillParticleEffect()
    {
        if (skillEffect != null && PlayerInstance.Instance != null)
        {
            // Get player position and forward direction
            Vector3 playerPos = PlayerInstance.Instance.transform.position;
            Vector3 playerForward = PlayerInstance.Instance.transform.forward;
            
            // Instantiate particle effect
            ParticleSystem effect = Instantiate(skillEffect, playerPos, Quaternion.LookRotation(playerForward));
            effect.Play();
            
            if(debug) Debug.Log($"[Skill] {skillName} particle effect started");
            
            // Destroy the effect after a reasonable time
            Destroy(effect.gameObject, 5f);
        }
        else
        {
            if(debug) Debug.LogWarning($"[Skill] {skillName} - No particle effect assigned or PlayerInstance not found");
        }
    }

    protected virtual void ApplyDamageToEnemies()
    {
        // find enemies in range and apply damage
        Collider[] enemies = Physics.OverlapSphere(Vector3.zero, skillRange, LayerMask.GetMask("Enemy"));
        
        foreach (var enemy in enemies)
        {
            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(skillDamage);
                if(debug) Debug.Log($"Dealt {skillDamage} damage to {enemy.name}");
            }
        }
    }

    protected virtual void FinishSkill()
    {
        // Remove shield immunity if this was a Shield skill
        if (skillName.ToLower().Contains("shield"))
        {
            if (PlayerInstance.Instance != null)
            {
                var playerHealthSystem = PlayerInstance.Instance.GetComponent<HealthSystem>();
                if (playerHealthSystem != null)
                {
                    playerHealthSystem.SetShieldImmunity(false);
                    if (debug) Debug.Log($"[Skill] Shield immunity removed from {PlayerInstance.Instance.name}");
                }
            }
        }
        
        currentCharges--;
        //hasBeenUsed = true; // mark skill as used
        StartCooldown(); 
    }

    protected virtual void StartCooldown()
    {
        currentState = SkillStates.Cooldown;
        currentCooldown = cooldownDuration;
        if(debug) Debug.Log($"[Skill] {skillName} - Starting cooldown ({cooldownDuration}s)");
        OnSkillCooldownStarted?.Invoke(this);
        
        // start coroutine to handle cooldown
        StartCoroutine(HandleCooldown());
    }

    protected virtual IEnumerator HandleCooldown()
    {
        if(debug) Debug.Log($"[Skill] {skillName} - Cooldown coroutine started");
        while (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            yield return null;
        }
        
        if(debug) Debug.Log($"[Skill] {skillName} - Cooldown finished");
        FinishCooldown();
    }

    protected virtual void FinishCooldown()
    {
        currentCooldown = 0f;
        // Don't reset state to ReadyToUse if skill has been used
        if (!hasBeenUsed)
        {
            currentState = SkillStates.ReadyToUse;
            currentCharges = maxCharges;
        }
        OnSkillCooldownFinished?.Invoke(this);
        if(debug && !hasBeenUsed) Debug.Log($"{skillName} is ready to use again!");
    }

    public virtual void UnlockSkill()
    {
        isUnlocked = true;
        currentState = SkillStates.ReadyToUse;
        if(debug) Debug.Log($"Unlocked skill: {skillName}");
    }

    public virtual void LockSkill()
    {
        isUnlocked = false;
        currentState = SkillStates.Locked;
        currentCooldown = 0f;
        currentCharges = 0;
    }

    public virtual void ResetCooldown()
    {
        if (isUnlocked)
        {
            currentState = SkillStates.ReadyToUse;
            currentCooldown = 0f;
            currentCharges = maxCharges;
            hasBeenUsed = false; // reset the used flag on respawn
            if (debug) Debug.Log($"[Skill] {skillName} cooldown reset - ready to use!");
        }
    }

    // start coroutines from ScriptableObject
    private void StartCoroutine(IEnumerator coroutine)
    {
        if (PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.StartCoroutine(coroutine);
            if(debug) Debug.Log($"[Skill] {skillName} - Coroutine started via PlayerSkills");
        }
        else
        {
            if(debug) Debug.LogError($"[Skill] {skillName} - Cannot start coroutine! PlayerSkills.Instance is null!");
        }
    }
}