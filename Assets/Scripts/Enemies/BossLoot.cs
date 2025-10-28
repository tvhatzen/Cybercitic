using UnityEngine;
using System.Collections.Generic;

public class BossLoot : EnemyLoot
{
    [Header("Skill Drops")]
    [SerializeField] private SkillDrop[] skillDrops;
    
    [Header("Boss Specific Settings")]
    [SerializeField] private int bossCreditsMultiplier = 5;

    protected override void HandleDeath(HealthSystem hs)
    {
        // give credits for boss kills
        if (CurrencyManager.Instance != null)
        {
            int bossCredits = creditsOnDeath * bossCreditsMultiplier;
            if(debug) Debug.Log($"{gameObject.name} (BOSS) dropped {bossCredits} credits!");
            CurrencyManager.Instance.AddCredits(bossCredits);
        }

        base.HandleDeath(hs);
        
        // skill drops
        //HandleSkillDrops();
    }

    private void HandleSkillDrops()
    {
        if (skillDrops == null || skillDrops.Length == 0) return;

        foreach (var skillDrop in skillDrops)
        {
            if (skillDrop.skill == null) continue;

            // check if skill should drop
            if (Random.Range(0f, 100f) < skillDrop.dropChance)
            {
                // check if player already has this skill
                if (PlayerSkills.Instance != null && !PlayerSkills.Instance.HasSkill(skillDrop.skill))
                {
                    // unlock skill
                    PlayerSkills.Instance.UnlockSkillFromBossDrop(skillDrop.skill);
                    
                    if(debug) Debug.Log($"BOSS DROP: Unlocked skill {skillDrop.skill.SkillName}!");
                }
                else
                {
                    if (debug) Debug.Log($"Player already has skill {skillDrop.skill.SkillName}, giving extra credits instead");
                    
                    // give credits if player already has the skill
                    if (CurrencyManager.Instance != null)
                    {
                        int bonusCredits = 50;
                        CurrencyManager.Instance.AddCredits(bonusCredits);
                        if(debug) Debug.Log($"Received {bonusCredits} bonus credits for already having {skillDrop.skill.SkillName}");
                    }
                }
            }
        }
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
