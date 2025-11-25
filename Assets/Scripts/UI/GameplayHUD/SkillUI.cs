using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Game.Skills;

public class SkillUI : MonoBehaviour
{
    // skill buttons should be a more abstract 'Button' class that handles visuals and input finctionality
    // can make menu buttons, upgrade buttons, skill buttons, etc. inherit from base button class
    // components like images, text, button, additional visual elements can be handled in the button class itself
    // can also set custom colors and change sprites, text, etc. from the button class
    // example of base class: MenuButton, SkillButton, UpgradeButton

    // SkillButton class should handle its own visuals and input functionality
    // SkillUI just manages the collection of SkillButtons and their interactions with the PlayerSkills system

    [Header("Skill Button References")]
    [SerializeField] private List<SkillButton> skillButtons = new List<SkillButton>();
    
    [Header("Skill Key Labels")]
    [SerializeField] private List<TextMeshProUGUI> keyLabels = new List<TextMeshProUGUI>();

    [Header("Testing")]
    [SerializeField] private bool unlockAllSkillsOnStart = false;
    [SerializeField] private List<Skill> testSkills = new List<Skill>(); // for testing

    public bool debug = false;

    private void Start()
    {
        InitializeSkillButtons();
        SubscribeToEvents();
        
        // Test mode: unlock all skills if checkbox is enabled
        if (unlockAllSkillsOnStart)
        {
            UnlockAllTestSkills();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeSkillButtons()
    {
        // initialize all skill buttons
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
        GameEvents.OnSkillActivated += OnSkillActivated;
        GameEvents.OnSkillUnlocked += OnSkillUnlocked;
    }

    private void UnsubscribeFromEvents()
    {
        GameEvents.OnSkillActivated -= OnSkillActivated;
        GameEvents.OnSkillUnlocked -= OnSkillUnlocked;
    }

    private void OnSkillActivated(Skill skill)
    {
        // find button for this skill and trigger visual feedback
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
        // find an empty slot and assign the skill
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
        // update all skill buttons (for cooldown display)
        foreach (var button in skillButtons)
        {
            if (button != null)
            {
                button.UpdateCooldown();
            }
        }
        
        // Debug: Log every few seconds to see if Update is being called
        if (debug && Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
        {
            Debug.Log($"[SkillUI] Update called - {skillButtons.Count} skill buttons");
        }
    }

    // Test method to unlock all skills and assign them to buttons
    private void UnlockAllTestSkills()
    {
        if (PlayerSkills.Instance == null)
        {
            if(debug) Debug.LogWarning("PlayerSkills instance not found! Cannot unlock test skills.");
            return;
        }

        if(debug) Debug.Log($"[SkillUI TEST MODE] Unlocking {testSkills.Count} test skills");

        // unlock and equip each test skill to available slots
        for (int i = 0; i < testSkills.Count && i < skillButtons.Count; i++)
        {
            if (testSkills[i] != null)
            {
                PlayerSkills.Instance.EquipSkill(testSkills[i], i);
                if(debug) Debug.Log($"[SkillUI TEST MODE] Equipped {testSkills[i].SkillName} to slot {i}");
            }
        }

        if(debug) Debug.Log("[SkillUI TEST MODE] All test skills unlocked and assigned!");
    }

    // manually unlock test skills
    public void UnlockTestSkillsManually()
    {
        UnlockAllTestSkills();
    }

    // test method to clear all skills 
    public void ClearAllSkills()
    {
        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (skillButtons[i] != null)
            {
                skillButtons[i].SetSkill(null);
            }
            
            if (PlayerSkills.Instance != null)
            {
                PlayerSkills.Instance.UnequipSkill(i);
            }
        }
        
        if(debug) Debug.Log("[SkillUI] All skills cleared");
    }
}