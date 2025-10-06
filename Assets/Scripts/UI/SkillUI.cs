using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour
{
    [Header("Skill Button References")]
    [SerializeField] private List<SkillButton> skillButtons = new List<SkillButton>();
    
    [Header("Skill Key Labels")]
    [SerializeField] private List<TextMeshProUGUI> keyLabels = new List<TextMeshProUGUI>();

    private void Start()
    {
        InitializeSkillButtons();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeSkillButtons()
    {
        // Initialize all skill buttons
        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (skillButtons[i] != null)
            {
                skillButtons[i].Initialize(i);
            }
        }
    }

    private void SubscribeToEvents()
    {
        if (PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.OnSkillActivated += OnSkillActivated;
            PlayerSkills.Instance.OnSkillUnlocked += OnSkillUnlocked;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.OnSkillActivated -= OnSkillActivated;
            PlayerSkills.Instance.OnSkillUnlocked -= OnSkillUnlocked;
        }
    }

    private void OnSkillActivated(Skill skill)
    {
        // Find the button for this skill and update it
        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (skillButtons[i] != null && skillButtons[i].GetSkill() == skill)
            {
                skillButtons[i].OnSkillActivated();
                break;
            }
        }
    }

    private void OnSkillUnlocked(Skill skill)
    {
        // Find an empty slot and assign the skill
        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (skillButtons[i] != null && skillButtons[i].GetSkill() == null)
            {
                skillButtons[i].SetSkill(skill);
                break;
            }
        }
    }

    private void Update()
    {
        // Update all skill buttons (for cooldown display)
        foreach (var button in skillButtons)
        {
            if (button != null)
            {
                button.UpdateCooldown();
            }
        }
    }
}


