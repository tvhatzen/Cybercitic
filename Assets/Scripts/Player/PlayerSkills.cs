using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerSkills : SingletonBase<PlayerSkills>
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode skill1Key = KeyCode.Q;
    [SerializeField] private KeyCode skill2Key = KeyCode.E;
    [SerializeField] private KeyCode skill3Key = KeyCode.R;
    [SerializeField] private KeyCode skill4Key = KeyCode.T;

    [Header("Skill Slots")]
    [SerializeField] private List<Skill> equippedSkills = new List<Skill>();

    // Events
    public event Action<Skill> OnSkillActivated;
    public event Action<Skill> OnSkillUnlocked;

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
        // Initialize all equipped skills
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
        // Handle skill activation based on input
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
        
        if (Input.GetKeyDown(skill4Key) && equippedSkills.Count > 3 && equippedSkills[3] != null)
        {
            ActivateSkill(equippedSkills[3]);
        }
    }

    public bool ActivateSkill(Skill skill)
    {
        if (skill == null) return false;

        bool success = skill.Activate();
        if (success)
        {
            OnSkillActivated?.Invoke(skill);
            Debug.Log($"Player activated skill: {skill.SkillName}");
        }
        else
        {
            Debug.Log($"Failed to activate skill: {skill.SkillName}");
        }

        return success;
    }

    public bool ActivateSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= equippedSkills.Count) return false;
        Debug.Log("Activated skill");
        return ActivateSkill(equippedSkills[skillIndex]);
    }

    public void EquipSkill(Skill skill, int slotIndex = -1)
    {
        if (skill == null) return;

        // If no slot specified, find the first available slot
        if (slotIndex == -1)
        {
            slotIndex = FindEmptySlot();
            if (slotIndex == -1)
            {
                Debug.LogWarning("No empty skill slots available!");
                return;
            }
        }

        // Ensure the slot index is valid
        if (slotIndex < 0 || slotIndex >= 4)
        {
            Debug.LogWarning($"Invalid skill slot index: {slotIndex}");
            return;
        }

        // Resize the list if necessary
        while (equippedSkills.Count <= slotIndex)
        {
            equippedSkills.Add(null);
        }

        // Equip the skill
        equippedSkills[slotIndex] = skill;
        skill.Initialize();
        skill.UnlockSkill();
        
        OnSkillUnlocked?.Invoke(skill);
        Debug.Log($"Equipped skill {skill.SkillName} to slot {slotIndex}");
    }

    public void UnequipSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Count) return;

        Skill skill = equippedSkills[slotIndex];
        if (skill != null)
        {
            skill.LockSkill();
            equippedSkills[slotIndex] = null;
            Debug.Log($"Unequipped skill from slot {slotIndex}");
        }
    }

    public void UnequipSkill(Skill skill)
    {
        int slotIndex = equippedSkills.IndexOf(skill);
        if (slotIndex != -1)
        {
            UnequipSkill(slotIndex);
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

    public Skill GetSkillAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Count) return null;
        return equippedSkills[slotIndex];
    }

    public List<Skill> GetAllEquippedSkills()
    {
        return new List<Skill>(equippedSkills);
    }

    public bool HasSkill(Skill skill)
    {
        return equippedSkills.Contains(skill);
    }

    public int GetSkillCount()
    {
        int count = 0;
        foreach (var skill in equippedSkills)
        {
            if (skill != null) count++;
        }
        return count;
    }

    // Method to unlock a skill from boss drops
    public void UnlockSkillFromBossDrop(Skill skill)
    {
        if (skill == null) return;

        // Add to player stats
        if (EntityStats.Instance != null)
        {
            EntityStats.Instance.AddSkill(skill);
        }

        // Equip the skill if there's space
        EquipSkill(skill);
        
        Debug.Log($"Unlocked new skill from boss drop: {skill.SkillName}");
    }

    // Method to spawn skill effects at player position
    public void SpawnSkillEffect(GameObject effectPrefab, Vector3 offset = default)
    {
        if (effectPrefab == null) return;

        Vector3 spawnPosition = transform.position + offset;
        GameObject effect = Instantiate(effectPrefab, spawnPosition, transform.rotation);
        
        // Auto-destroy the effect after a reasonable time if it doesn't destroy itself
        if (effect.GetComponent<ParticleSystem>() != null)
        {
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps.main.duration > 0)
            {
                Destroy(effect, ps.main.duration + 1f);
            }
        }
    }
}
