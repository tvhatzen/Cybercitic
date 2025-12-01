using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Skills;

/// <summary>
/// EntityStats is a PLAYER-ONLY singleton for managing upgrades and skills.
/// </summary>
public class EntityStats : SingletonBase<EntityStats>
{
    [Header("Skills and Upgrades")]
    public List<Skill> skills = new List<Skill>();
    public List<Upgrade> upgrades = new List<Upgrade>();

    private EntityData playerData;

    [Header("DEBUG")]
    public bool debug = false;

    protected override void Awake()
    {
        base.Awake();
        
        playerData = GetComponent<EntityData>();
        if (playerData == null)
        {
            if(debug) Debug.LogWarning("EntityStats: No EntityData found on player. Add EntityData component to player.");
        }
    }

    public EntityData GetPlayerData()
    {
        return playerData;
    }

    // stat modification methods - these modify the player's EntityData
    public void ModifyHealth(int amount)
    {
        if (playerData == null)
        {
            // Try to get playerData if it's null
            playerData = GetComponent<EntityData>();
            if (playerData == null && debug)
            {
                Debug.LogWarning("[EntityStats] playerData is null, attempting to find EntityData component");
            }
        }
        
        if (playerData != null)
        {
            int oldHealth = playerData.baseHealth;
            playerData.ModifyBaseStats(healthMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"[EntityStats] Health modified by {amount}. Old: {oldHealth}, New: {playerData.baseHealth}");
        }
        else
        {
            if(debug) Debug.LogError("[EntityStats] Cannot modify health - playerData is null!");
        }
    }

    public void ModifySpeed(float amount)
    {
        if (playerData == null) playerData = GetComponent<EntityData>();
        
        if (playerData != null)
        {
            float oldSpeed = playerData.baseSpeed;
            playerData.ModifyBaseStats(speedMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"[EntityStats] Speed modified by {amount}. Old: {oldSpeed}, New: {playerData.baseSpeed}");
        }
        else if(debug) Debug.LogError("[EntityStats] Cannot modify speed - playerData is null!");
    }

    public void ModifyAttack(int amount)
    {
        if (playerData == null) playerData = GetComponent<EntityData>();
        
        if (playerData != null)
        {
            int oldAttack = playerData.baseAttack;
            playerData.ModifyBaseStats(attackMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"[EntityStats] Attack modified by {amount}. Old: {oldAttack}, New: {playerData.baseAttack}");
        }
        else if(debug) Debug.LogError("[EntityStats] Cannot modify attack - playerData is null!");
    }

    public void ModifyDodgeChance(float amount)
    {
        if (playerData == null) playerData = GetComponent<EntityData>();
        
        if (playerData != null)
        {
            float oldDodge = playerData.baseDodgeChance;
            playerData.ModifyBaseStats(dodgeMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"[EntityStats] Dodge chance modified by {amount}. Old: {oldDodge}, New: {playerData.baseDodgeChance}");
        }
        else if(debug) Debug.LogError("[EntityStats] Cannot modify dodge chance - playerData is null!");
    }

    public void ModifyDefense(float amount)
    {
        if (playerData == null) playerData = GetComponent<EntityData>();
        
        if (playerData != null)
        {
            float oldDefense = playerData.baseDefense;
            playerData.ModifyBaseStats(defenseMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"[EntityStats] Defense modified by {amount}. Old: {oldDefense}, New: {playerData.baseDefense}");
        }
        else if(debug) Debug.LogError("[EntityStats] Cannot modify defense - playerData is null!");
    }

    // reset all stats to base values
    public void ResetStats()
    {
        if (playerData != null)
        {
            playerData.ResetToOriginalStats();
            
            GameEvents.StatsChanged(this);
        }
        
        // clear all skills and upgrades
        skills.Clear();
        upgrades.Clear();
        
        // reset the health system to update UI
        var healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.ResetToOriginalStats();
        }
        
        if (debug) Debug.Log("[EntityStats] All stats, skills, and upgrades reset to original values");
    }

    // add skill to the player's skill list
    public void AddSkill(Skill skill)
    {
        if (skill != null && !skills.Contains(skill))
        {
            skills.Add(skill);
            if(debug) Debug.Log($"Added skill: {skill.name}");
        }
    }

    // remove skill from the player's skill list
    public void RemoveSkill(Skill skill)
    {
        if (skills.Contains(skill))
        {
            skills.Remove(skill);
            if(debug) Debug.Log($"Removed skill: {skill.name}");
        }
    }
}
