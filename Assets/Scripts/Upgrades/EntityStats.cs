using System.Collections.Generic;
using UnityEngine;
using System;

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
        if (playerData != null)
        {
            playerData.ModifyBaseStats(healthMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"Health modified by {amount}. New base: {playerData.baseHealth}");
        }
    }

    public void ModifySpeed(float amount)
    {
        if (playerData != null)
        {
            playerData.ModifyBaseStats(speedMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"Speed modified by {amount}. New base: {playerData.baseSpeed}");
        }
    }

    public void ModifyAttack(int amount)
    {
        if (playerData != null)
        {
            playerData.ModifyBaseStats(attackMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"Attack modified by {amount}. New base: {playerData.baseAttack}");
        }
    }

    public void ModifyDodgeChance(float amount)
    {
        if (playerData != null)
        {
            playerData.ModifyBaseStats(dodgeMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"Dodge chance modified by {amount}. New base: {playerData.baseDodgeChance}");
        }
    }

    public void ModifyDefense(float amount)
    {
        if (playerData != null)
        {
            playerData.ModifyBaseStats(defenseMod: amount);
            
            GameEvents.StatsChanged(this);
            if(debug) Debug.Log($"Defense modified by {amount}. New base: {playerData.baseDefense}");
        }
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
