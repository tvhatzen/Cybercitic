using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Game.Skills;

public class PlayerSkills : SingletonBase<PlayerSkills>
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode skill1Key = KeyCode.Alpha1;
    [SerializeField] private KeyCode skill2Key = KeyCode.Alpha2;
    [SerializeField] private KeyCode skill3Key = KeyCode.Alpha3;

    [Header("Skill Slots")]
    [SerializeField] private List<Skill> equippedSkills = new List<Skill>();

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem skillUseParticle;

    public bool debug = false;

    private SkillManager skillManager;

    protected override void Awake()
    {
        base.Awake(); 
        
        // Get or create SkillManager
        skillManager = GetComponent<SkillManager>();
        if (skillManager == null)
        {
            skillManager = gameObject.AddComponent<SkillManager>();
            if (debug) Debug.Log("[PlayerSkills] Created SkillManager component");
        }
    }

    private void Start()
    {
        InitializeSkills();
    }

    private void Update()
    {
        HandleSkillInput();
    }

    private void InitializeSkills()
    {
        // Skills are now initialized through SkillManager
    }

    private void HandleSkillInput()
    {
        // handle skill activation based on input
        if (Input.GetKeyDown(skill1Key) && equippedSkills.Count > 0 && equippedSkills[0] != null)
        {
            ActivateSkill(equippedSkills[0]);
        }
        
        if (Input.GetKeyDown(skill2Key) && equippedSkills.Count > 1 && equippedSkills[1] != null)
        {
            ActivateSkill(equippedSkills[1]);
        }
        
        if (Input.GetKeyDown(skill3Key) && equippedSkills.Count > 2 && equippedSkills[2] != null)
        {
            ActivateSkill(equippedSkills[2]);
        }
    }

    public bool ActivateSkill(Skill skill)
    {
        if (skill == null)
        {
            if(debug) Debug.LogWarning("[PlayerSkills] Cannot activate - skill is null");
            return false;
        }

        if (skillManager == null)
        {
            if(debug) Debug.LogError("[PlayerSkills] SkillManager is null!");
            return false;
        }

        bool success = skillManager.ActivateSkill(skill);
        if (success)
        {
            GameEvents.SkillActivated(skill);
            if(debug) Debug.Log($"[PlayerSkills] Successfully activated skill: {skill.SkillName}");
        }

        return success;
    }

    public bool ActivateSkill(int skillIndex)
    {
        if(debug) Debug.Log($"[PlayerSkills] ActivateSkill called with index {skillIndex}");
        
        if (skillIndex < 0 || skillIndex >= equippedSkills.Count)
        {
            if(debug) Debug.LogWarning($"[PlayerSkills] Invalid skill index {skillIndex} (count: {equippedSkills.Count})");
            return false;
        }
        
        Skill skill = equippedSkills[skillIndex];
        if (skill == null)
        {
            if(debug) Debug.LogWarning($"[PlayerSkills] No skill at index {skillIndex}");
            return false;
        }
        
        // Get skill instance to check state
        SkillInstance instance = skillManager?.GetSkillInstance(skill);
        string stateInfo = instance != null ? $"State: {instance.CurrentState}, Ready: {instance.IsReady}" : "State: Unknown";
        if(debug) Debug.Log($"[PlayerSkills] Activating skill at index {skillIndex}: {skill.SkillName} ({stateInfo})");

        AudioManager.Instance.PlaySound("useSkill");

        return ActivateSkill(skill);
    }

    public void EquipSkill(Skill skill, int slotIndex = -1)
    {
        if (skill == null) return;

        // if no slot specified, find first available slot
        if (slotIndex == -1)
        {
            slotIndex = FindEmptySlot();
            if (slotIndex == -1)
            {
                if(debug) Debug.LogWarning("No empty skill slots available!");
                return;
            }
        }

        if (slotIndex < 0 || slotIndex >= 4)
        {
            if(debug) Debug.LogWarning($"Invalid skill slot index: {slotIndex}");
            return;
        }
        // add to player equipped skills
        while (equippedSkills.Count <= slotIndex)
        {
            equippedSkills.Add(null);
        }

        // equip skill
        equippedSkills[slotIndex] = skill;
        
        // Use SkillManager to unlock and manage the skill
        if (skillManager != null)
        {
            skillManager.UnlockSkill(skill);
        }
        
        GameEvents.SkillUnlocked(skill);
        if(debug) Debug.Log($"Equipped skill {skill.SkillName} to slot {slotIndex}");
    }

    public void UnequipSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Count) return;

        Skill skill = equippedSkills[slotIndex];
        if (skill != null)
        {
            if (skillManager != null)
            {
                skillManager.LockSkill(skill);
            }
            equippedSkills[slotIndex] = null;
            if(debug) Debug.Log($"Unequipped skill from slot {slotIndex}");
        }
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i >= equippedSkills.Count || equippedSkills[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    // unlock a skill from boss drops
    public void UnlockSkillFromBossDrop(Skill skill)
    {
        if (skill == null) return;

        // add to player stats
        if (EntityStats.Instance != null)
        {
            EntityStats.Instance.AddSkill(skill);
        }

        // equip the skill if there's space
        EquipSkill(skill);
        
        if(debug) Debug.Log($"Unlocked new skill from boss drop: {skill.SkillName}");
    }

    public bool HasSkill(Skill skill)
    {
        return equippedSkills.Contains(skill);
    }

    public void ResetAllSkillCooldowns()
    {
        if (debug) Debug.Log("[PlayerSkills] Resetting all skill cooldowns");
        
        if (skillManager != null)
        {
            skillManager.ResetAllCooldowns();
        }
    }

    // reset all equipped skills for a fresh start
    public void ResetAllSkills()
    {
        if (debug) Debug.Log("[PlayerSkills] Resetting all equipped skills");
        
        // Lock all skills but keep the equipped skills list intact
        // This way we can re-unlock them when starting a new game
        for (int i = 0; i < equippedSkills.Count; i++)
        {
            if (equippedSkills[i] != null)
            {
                if (skillManager != null)
                {
                    skillManager.LockSkill(equippedSkills[i]);
                }
                // Don't clear the equipped skills - keep them for re-unlocking
            }
        }
        
        if (debug) Debug.Log($"[PlayerSkills] All skills locked for fresh start (kept {equippedSkills.Count} skills in equipped list)");
    }
    
    // Re-unlock all equipped skills (called when starting a new game)
    public void ReunlockEquippedSkills()
    {
        if (equippedSkills == null || equippedSkills.Count == 0)
        {
            if (debug) Debug.Log("[PlayerSkills] No equipped skills to re-unlock");
            return;
        }
        
        if (debug) Debug.Log($"[PlayerSkills] Re-unlocking {equippedSkills.Count} equipped skills");
        
        int unlockedCount = 0;
        for (int i = 0; i < equippedSkills.Count; i++)
        {
            if (equippedSkills[i] != null)
            {
                if (skillManager != null)
                {
                    skillManager.UnlockSkill(equippedSkills[i]);
                    unlockedCount++;
                    if (debug) Debug.Log($"[PlayerSkills] Re-unlocked skill: {equippedSkills[i].SkillName}");
                }
                else
                {
                    if (debug) Debug.LogWarning($"[PlayerSkills] SkillManager is null, cannot unlock {equippedSkills[i].SkillName}");
                }
            }
        }
        
        if (debug) Debug.Log($"[PlayerSkills] Re-unlocked {unlockedCount} out of {equippedSkills.Count} equipped skills");
    }
}
