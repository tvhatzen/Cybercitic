using UnityEngine;
using System.Collections.Generic;

public class BossLoot : EnemyLoot
{
    [Header("Skill Drops")]
    [SerializeField] private SkillDrop[] skillDrops;
    
    [Header("Boss Specific Settings")]
    [SerializeField] private bool isBoss = true;
    [SerializeField] private int bossCreditsMultiplier = 5;

    protected override void HandleDeath(HealthSystem hs)
    {
        // Give more credits for boss kills
        if (CurrencyManager.Instance != null)
        {
            int bossCredits = creditsOnDeath * bossCreditsMultiplier;
            Debug.Log($"{gameObject.name} (BOSS) dropped {bossCredits} credits!");
            CurrencyManager.Instance.AddCredits(bossCredits);
        }

        // Call base loot handling
        base.HandleDeath(hs);
        
        // Handle skill drops
        HandleSkillDrops();
    }

    private void HandleSkillDrops()
    {
        if (skillDrops == null || skillDrops.Length == 0) return;

        foreach (var skillDrop in skillDrops)
        {
            if (skillDrop.skill == null) continue;

            // Check if skill should drop
            if (Random.Range(0f, 100f) < skillDrop.dropChance)
            {
                // Check if player already has this skill
                if (PlayerSkills.Instance != null && !PlayerSkills.Instance.HasSkill(skillDrop.skill))
                {
                    // Unlock the skill
                    PlayerSkills.Instance.UnlockSkillFromBossDrop(skillDrop.skill);
                    
                    // Show notification
                    ShowSkillUnlockedNotification(skillDrop.skill);
                    
                    Debug.Log($"BOSS DROP: Unlocked skill {skillDrop.skill.SkillName}!");
                }
                else
                {
                    Debug.Log($"Player already has skill {skillDrop.skill.SkillName}, giving extra credits instead");
                    // Give extra credits if player already has the skill
                    if (CurrencyManager.Instance != null)
                    {
                        int bonusCredits = 50;
                        CurrencyManager.Instance.AddCredits(bonusCredits);
                        Debug.Log($"Received {bonusCredits} bonus credits for already having {skillDrop.skill.SkillName}");
                    }
                }
            }
        }
    }

    private void ShowSkillUnlockedNotification(Skill skill)
    {
        // show UI notification
        Debug.Log($"NEW SKILL UNLOCKED: {skill.SkillName} - {skill.Description}");
    }
}

[System.Serializable]
public class SkillDrop
{
    [Header("Skill Drop Settings")]
    public Skill skill;
    [Range(0f, 100f)]
    public float dropChance = 100f;
    
    [Header("Additional Info")]
    public string dropMessage = "New skill unlocked!";
}
