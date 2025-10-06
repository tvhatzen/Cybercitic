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

[System.Serializable]
public class SkillButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button button;
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private TextMeshProUGUI keyLabel;
    
    [Header("Visual Settings")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color castingColor = Color.yellow;

    private Skill assignedSkill;
    private int slotIndex;

    public void Initialize(int index)
    {
        slotIndex = index;
        
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }

        // Set up key label
        if (keyLabel != null)
        {
            switch (index)
            {
                case 0:
                    keyLabel.text = "Q";
                    break;
                case 1:
                    keyLabel.text = "E";
                    break;
                case 2:
                    keyLabel.text = "R";
                    break;
                case 3:
                    keyLabel.text = "T";
                    break;
                default:
                    keyLabel.text = "";
                    break;
            }
        }

        // Initially hide the button
        SetButtonActive(false);
    }

    public void SetSkill(Skill skill)
    {
        assignedSkill = skill;
        
        if (skill != null)
        {
            SetButtonActive(true);
            
            if (skillIcon != null)
                skillIcon.sprite = skill.Icon;
                
            Debug.Log($"Set skill {skill.SkillName} to button slot {slotIndex}");
        }
        else
        {
            SetButtonActive(false);
        }
    }

    public Skill GetSkill()
    {
        return assignedSkill;
    }

    private void OnButtonClicked()
    {
        if (assignedSkill != null && PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.ActivateSkill(slotIndex);
        }
    }

    public void UpdateCooldown()
    {
        if (assignedSkill == null) return;

        switch (assignedSkill.CurrentState)
        {
            case Skill.SkillStates.ReadyToUse:
                SetButtonState(readyColor, 0f, "");
                break;
                
            case Skill.SkillStates.Casting:
                SetButtonState(castingColor, 0f, "CASTING");
                break;
                
            case Skill.SkillStates.Cooldown:
                float progress = assignedSkill.CooldownProgress;
                float remainingTime = assignedSkill.CurrentCooldown;
                SetButtonState(cooldownColor, progress, $"{remainingTime:F1}s");
                break;
                
            case Skill.SkillStates.Locked:
                SetButtonState(cooldownColor, 1f, "LOCKED");
                break;
        }
    }

    private void SetButtonState(Color color, float cooldownFill, string cooldownTextValue)
    {
        if (skillIcon != null)
            skillIcon.color = color;
            
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = cooldownFill;
            cooldownOverlay.gameObject.SetActive(cooldownFill > 0);
        }
        
        if (cooldownText != null)
        {
            cooldownText.text = cooldownTextValue;
            cooldownText.gameObject.SetActive(!string.IsNullOrEmpty(cooldownTextValue));
        }

        if (button != null)
            button.interactable = assignedSkill != null && assignedSkill.IsReady;
    }

    private void SetButtonActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void OnSkillActivated()
    {
        // This method can be called when a skill is activated to provide visual feedback
        Debug.Log($"Skill button {slotIndex} activated!");
    }
}
