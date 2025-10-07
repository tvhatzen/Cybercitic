using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityStats : SingletonBase<EntityStats>
{
    // make it so enemies can also use this, will need to alter stats of same enemy type depending on floor level
    // elits and boss enemies can add skills to use other types of attacks
    // implement enemy tiers - increases health and damage (make scalable?)

    [Header("Base Stats")] 
    public int baseHealth = 100;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private int baseAttack = 10;
    [SerializeField] private float baseDodgeChance = 0.1f;

    [Header("Current Stats")] // reference these in movement & combat !!!
    public int Health;
    public float speed;
    public int attack;
    public float dodgeChance;

    [Header("Skills and Upgrades")]
    public List<Skill> skills = new List<Skill>();
    public List<Upgrade> upgrades = new List<Upgrade>();

    public event Action<EntityStats> OnStatsChanged;

    protected override void Awake()
    {
        base.Awake();
        InitializeStats();
    }

    private void InitializeStats()
    {
        Health = baseHealth;
        speed = baseSpeed;
        attack = baseAttack;
        dodgeChance = baseDodgeChance;
    }

    // Stat modification methods
    public void ModifyHealth(int amount)
    {
        Health += amount;
        OnStatsChanged?.Invoke(this);
        Debug.Log($"Health modified by {amount}. Current: {Health}");
    }

    public void ModifySpeed(float amount)
    {
        speed += amount;
        OnStatsChanged?.Invoke(this);
        Debug.Log($"Speed modified by {amount}. Current: {speed}");
    }

    public void ModifyAttack(int amount)
    {
        attack += amount;
        OnStatsChanged?.Invoke(this);
        Debug.Log($"Attack modified by {amount}. Current: {attack}");
    }

    public void ModifyDodgeChance(float amount)
    {
        dodgeChance = Mathf.Clamp01(dodgeChance + amount);
        OnStatsChanged?.Invoke(this);
        Debug.Log($"Dodge chance modified by {amount}. Current: {dodgeChance}");
    }

    // Reset all stats to base values
    public void ResetStats()
    {
        InitializeStats();
        OnStatsChanged?.Invoke(this);
    }

    // Add skill to the player's skill list
    public void AddSkill(Skill skill)
    {
        if (skill != null && !skills.Contains(skill))
        {
            skills.Add(skill);
            Debug.Log($"Added skill: {skill.name}");
        }
    }

    // Remove skill from the player's skill list
    public void RemoveSkill(Skill skill)
    {
        if (skills.Contains(skill))
        {
            skills.Remove(skill);
            Debug.Log($"Removed skill: {skill.name}");
        }
    }
}
