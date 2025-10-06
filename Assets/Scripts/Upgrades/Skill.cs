using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
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
    [SerializeField] private GameObject skillEffect;
    
    // Runtime data
    [NonSerialized] public SkillStates currentState = SkillStates.ReadyToUse;
    [NonSerialized] public float currentCooldown = 0f;
    [NonSerialized] public int currentCharges = 0;
    [NonSerialized] public bool isUnlocked = false;

    // Events
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

    public virtual void Initialize()
    {
        currentState = isUnlocked ? SkillStates.ReadyToUse : SkillStates.Locked;
        currentCooldown = 0f;
        currentCharges = maxCharges;
    }

    public virtual bool Activate()
    {
        if (!CanActivate()) return false;

        Debug.Log($"Activating skill: {skillName}");
        currentState = SkillStates.Casting;
        OnSkillActivated?.Invoke(this);

        // Start the skill effect
        StartCoroutine(ExecuteSkill());

        return true;
    }

    protected virtual bool CanActivate()
    {
        return isUnlocked && currentState == SkillStates.ReadyToUse && currentCharges > 0;
    }

    protected virtual IEnumerator ExecuteSkill()
    {
        yield return new WaitForSeconds(castingTime);
        
        ApplySkillEffects();
        
        yield return new WaitForSeconds(skillDuration);
        
        FinishSkill();
    }

    protected virtual void ApplySkillEffects()
    {
        Debug.Log($"{skillName} effects applied!");
        
        // Play sound effect
        if (skillSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(skillSound);
        }
        
        // Spawn visual effect
        if (skillEffect != null)
        {
            // handled by PlayerSkills component
            // spawn at the player's position
        }
        
        // Apply damage to enemies in range
        ApplyDamageToEnemies();
    }

    protected virtual void ApplyDamageToEnemies()
    {
        // Find enemies in range and apply damage
        Collider[] enemies = Physics.OverlapSphere(Vector3.zero, skillRange, LayerMask.GetMask("Enemy"));
        
        foreach (var enemy in enemies)
        {
            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(skillDamage);
                Debug.Log($"Dealt {skillDamage} damage to {enemy.name}");
            }
        }
    }

    protected virtual void FinishSkill()
    {
        currentCharges--;
        StartCooldown();
    }

    protected virtual void StartCooldown()
    {
        currentState = SkillStates.Cooldown;
        currentCooldown = cooldownDuration;
        OnSkillCooldownStarted?.Invoke(this);
        
        // Start coroutine to handle cooldown
        StartCoroutine(HandleCooldown());
    }

    protected virtual IEnumerator HandleCooldown()
    {
        while (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            yield return null;
        }
        
        FinishCooldown();
    }

    protected virtual void FinishCooldown()
    {
        currentCooldown = 0f;
        currentState = SkillStates.ReadyToUse;
        currentCharges = maxCharges;
        OnSkillCooldownFinished?.Invoke(this);
        Debug.Log($"{skillName} is ready to use again!");
    }

    public virtual void UnlockSkill()
    {
        isUnlocked = true;
        currentState = SkillStates.ReadyToUse;
        Debug.Log($"Unlocked skill: {skillName}");
    }

    public virtual void LockSkill()
    {
        isUnlocked = false;
        currentState = SkillStates.Locked;
        currentCooldown = 0f;
        currentCharges = 0;
    }

    // start coroutines from ScriptableObject
    private void StartCoroutine(IEnumerator coroutine)
    {
        if (PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.StartCoroutine(coroutine);
        }
    }
}
