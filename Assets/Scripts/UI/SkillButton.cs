using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SkillButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button button;
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private TextMeshProUGUI skillNameText; 
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private GameObject toolTip;

    [Header("Visual Settings")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color castingColor = Color.yellow;

    private Skill assignedSkill;
    private int slotIndex;

    [Header("DEBUG")]
    public bool debug = false;

    public void Initialize(int index)
    {
        slotIndex = index;

        // set up button click listener
        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // clear any existing listeners
            button.onClick.AddListener(OnButtonClicked);
            if (debug) Debug.Log($"[SkillButton] Button click listener added for slot {index}");
        }
        else
        {
            Debug.LogError($"[SkillButton] Button component is null for slot {index}!");
        }

        // set up key label
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

        // show the button but with no skill assigned
        if (skillIcon != null)
        {
            skillIcon.enabled = false;
            skillIcon.sprite = null;
        }

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.gameObject.SetActive(false);
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
            cooldownText.gameObject.SetActive(false);
        }

        if (skillNameText != null)
        {
            skillNameText.text = "";
            skillNameText.gameObject.SetActive(false);
        }

        SetButtonActive(true);
    }

    public void SetSkill(Skill skill)
    {
        assignedSkill = skill;

        if (skill != null)
        {
            SetButtonActive(true);

            // set skill icon
            if (skillIcon != null && skill.Icon != null)
            {
                skillIcon.sprite = skill.Icon;
                skillIcon.enabled = true;
                skillIcon.color = readyColor; 
            }

            // set skill name
            if (skillNameText != null)
            {
                skillNameText.text = skill.SkillName;
                skillNameText.gameObject.SetActive(true);
            }

            // initialize cooldown overlay
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.gameObject.SetActive(false);
            }

            // clear cooldown text
            if (cooldownText != null)
            {
                cooldownText.text = "";
                cooldownText.gameObject.SetActive(false);
            }
        }
        else
        {
            // clear button
            if (skillIcon != null)
            {
                skillIcon.sprite = null;
                skillIcon.enabled = false;
            }
            
            if (skillNameText != null)
            {
                skillNameText.text = "";
                skillNameText.gameObject.SetActive(false);
            }
            
            SetButtonActive(false);
        }
    }

    public Skill GetSkill()
    {
        return assignedSkill;
    }

    public void OnButtonClicked()
    {
        if (debug) Debug.Log($"[SkillButton] Button clicked for slot {slotIndex}");
        
        // Check if button is interactable
        if (button != null && !button.interactable)
        {
            if (debug) Debug.LogWarning($"[SkillButton] Button is not interactable for slot {slotIndex}");
        }
        
        // Check if assignedSkill is ready
        if (assignedSkill != null && !assignedSkill.IsReady)
        {
            if (debug) Debug.LogWarning($"[SkillButton] Skill {assignedSkill.SkillName} is not ready (State: {assignedSkill.CurrentState})");
        }
        
        if (assignedSkill != null && PlayerSkills.Instance != null)
        {
            if (debug) Debug.Log($"[SkillButton] Attempting to activate skill: {assignedSkill.SkillName}");
            bool activated = PlayerSkills.Instance.ActivateSkill(slotIndex);
            if (debug) Debug.Log($"[SkillButton] Skill activation result: {activated}");
        }
        else
        {
            if (debug) Debug.LogWarning($"[SkillButton] Cannot activate skill - assignedSkill: {assignedSkill != null}, PlayerSkills: {PlayerSkills.Instance != null}");
        }
    }

    public void UpdateCooldown()
    {
        if (assignedSkill == null) 
        {
            if(debug) Debug.Log($"[SkillButton] Slot {slotIndex} - No skill assigned");
            return;
        }

        if(debug) Debug.Log($"[SkillButton] Slot {slotIndex} - {assignedSkill.SkillName} State: {assignedSkill.CurrentState}, Ready: {assignedSkill.IsReady}, Cooldown: {assignedSkill.CurrentCooldown:F1}s");

        switch (assignedSkill.CurrentState)
        {
            case Skill.SkillStates.ReadyToUse:
                SetButtonState(readyColor, 0f, "");
                if(debug) Debug.Log($"[SkillButton] {assignedSkill.SkillName} - READY TO USE");
                break;

            case Skill.SkillStates.Casting:
                SetButtonState(castingColor, 0f, "CASTING");
                cooldownText.fontSize = 16; // change text size
                if (debug) Debug.Log($"[SkillButton] {assignedSkill.SkillName} is CASTING");
                break;

            case Skill.SkillStates.Cooldown:
                float progress = assignedSkill.CooldownProgress;
                float remainingTime = assignedSkill.CurrentCooldown;
                SetButtonState(cooldownColor, progress, $"{remainingTime:F1}s");
                cooldownText.fontSize = 30; // change text size
                if (debug) Debug.Log($"[SkillButton] {assignedSkill.SkillName} cooldown: {remainingTime:F1}s (progress: {progress:F2})");
                break;

            case Skill.SkillStates.Locked:
                SetButtonState(cooldownColor, 1f, "LOCKED");
                if(debug) Debug.Log($"[SkillButton] {assignedSkill.SkillName} - LOCKED");
                break;
        }
    }

    private void SetButtonState(Color color, float cooldownFill, string cooldownTextValue)
    {
        if(debug) Debug.Log($"[SkillButton] SetButtonState - Color: {color}, Fill: {cooldownFill}, Text: '{cooldownTextValue}'");
        
        if (skillIcon != null)
        {
            skillIcon.color = color;
            if(debug) Debug.Log($"[SkillButton] Skill icon color set to: {color}");
        }
        else
        {
            if(debug) Debug.LogWarning($"[SkillButton] Slot {slotIndex} - skillIcon is null!");
        }

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = cooldownFill;
            cooldownOverlay.gameObject.SetActive(cooldownFill > 0);
            if(debug) Debug.Log($"[SkillButton] Cooldown overlay - Fill: {cooldownFill}, Active: {cooldownOverlay.gameObject.activeSelf}");
        }
        else
        {
            if(debug) Debug.LogWarning($"[SkillButton] Slot {slotIndex} - cooldownOverlay is null!");
        }

        // display cooldown timer text
        if (cooldownText != null)
        {
            cooldownText.text = cooldownTextValue;
            cooldownText.gameObject.SetActive(!string.IsNullOrEmpty(cooldownTextValue));
            
            if(debug) Debug.Log($"[SkillButton] Cooldown text - Text: '{cooldownTextValue}', Active: {cooldownText.gameObject.activeSelf}");
        }
        else
        {
            if(debug) Debug.LogWarning($"[SkillButton] Slot {slotIndex} - cooldownText is null!");
        }

        if (button != null)
        {
            button.interactable = assignedSkill != null && assignedSkill.IsReady;
            if(debug) Debug.Log($"[SkillButton] Button interactable: {button.interactable}");
        }
        else
        {
            if(debug) Debug.LogWarning($"[SkillButton] Slot {slotIndex} - button is null!");
        }
    }

    private void SetButtonActive(bool active) => gameObject.SetActive(active);
    public void OnSkillActivated() => OnButtonClicked();
    public void ShowTooltip() => toolTip.SetActive(true);
    public void HideTooltip() => toolTip.SetActive(false);
}
