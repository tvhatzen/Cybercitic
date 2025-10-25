using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
        // initialize all equipped skills
        foreach (var skill in equippedSkills)
        {
            if (skill != null)
            {
                skill.Initialize();
            }
        }
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

        bool success = skill.Activate();
        if (success)
        {
            GameEvents.SkillActivated(skill);
            if(debug) Debug.Log($"[PlayerSkills] Successfully activated skill: {skill.SkillName}");
        }

        return success;
    }

    public bool ActivateSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= equippedSkills.Count)
            return false;
        
        Skill skill = equippedSkills[skillIndex];
        if (skill == null)
            return false;
        
        if(debug) Debug.Log($"[PlayerSkills] Activating skill at index {skillIndex}: {skill.SkillName}");

        PlayParticles();
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
                if(debug) Debug.LogWarning("PlayerSkills: No empty skill slots available!");
                return;
            }
        }

        if (slotIndex < 0 || slotIndex >= 4)
        {
            if(debug) Debug.LogWarning($"PlayerSkills: Invalid skill slot index: {slotIndex}");
            return;
        }
        // add to player equipped skills
        while (equippedSkills.Count <= slotIndex)
        {
            equippedSkills.Add(null);
        }

        // equip skill
        equippedSkills[slotIndex] = skill;
        skill.Initialize();
        skill.UnlockSkill();
        
        GameEvents.SkillUnlocked(skill); // FIXME: causing skills to stay locked when unlocked at start
        if(debug) Debug.Log($"PlayerSkills: Equipped skill {skill.SkillName} to slot {slotIndex}");
    }

    public void UnequipSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Count) return;

        Skill skill = equippedSkills[slotIndex];
        if (skill != null)
        {
            skill.LockSkill();
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
        
        foreach (var skill in equippedSkills)
        {
            if (skill != null)
            {
                skill.ResetCooldown();
            }
        }
    }

    // reset all equipped skills for a fresh start
    public void ResetAllSkills()
    {
        if (debug) Debug.Log("[PlayerSkills] Resetting all equipped skills");
        
        // clear all equipped skills
        for (int i = 0; i < equippedSkills.Count; i++)
        {
            if (equippedSkills[i] != null)
            {
                equippedSkills[i].LockSkill();
                equippedSkills[i] = null;
            }
        }
        
        // clear the list
        equippedSkills.Clear();
        
        if (debug) Debug.Log("[PlayerSkills] All skills cleared for fresh start");
    }

    private void PlayParticles()
    {
        if (skillUseParticle != null)
        {
            skillUseParticle.Play();
        }
    }
}
