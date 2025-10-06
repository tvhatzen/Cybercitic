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
                    keyLabel.text = "1";
                    break;
                case 3:
                    keyLabel.text = "T";
                    break;
                default:
                    keyLabel.text = "";
                    break;
            }
        }

        // Initially hide the button (set to true for testing)
        SetButtonActive(true);
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

    public void OnButtonClicked()
    {
        if (assignedSkill != null && PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.ActivateSkill(slotIndex);
            Debug.Log("Skill button clicked");
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
        // called when a skill is activated for visual feedback
        Debug.Log($"Skill button {slotIndex} activated!");
    }
}
