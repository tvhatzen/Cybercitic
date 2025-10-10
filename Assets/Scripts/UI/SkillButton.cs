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

    [Header("Visual Settings")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color castingColor = Color.yellow;

    private Skill assignedSkill;
    private int slotIndex;
    private int instanceID; 

    [Header("DEBUG")]
    public bool debug = false;

    public void Initialize(int index)
    {
        slotIndex = index;
        instanceID = GetInstanceID(); // store instance ID for debugging

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
        if (assignedSkill != null && PlayerSkills.Instance != null)
        {
            bool activated = PlayerSkills.Instance.ActivateSkill(slotIndex);
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
                if(debug) Debug.Log($"[SkillButton] {assignedSkill.SkillName} is CASTING");
                break;

            case Skill.SkillStates.Cooldown:
                float progress = assignedSkill.CooldownProgress;
                float remainingTime = assignedSkill.CurrentCooldown;
                SetButtonState(cooldownColor, progress, $"{remainingTime:F1}s");
                if(debug) Debug.Log($"[SkillButton] {assignedSkill.SkillName} cooldown: {remainingTime:F1}s (progress: {progress:F2})");
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

        // display cooldown timer text
        if (cooldownText != null)
        {
            cooldownText.text = cooldownTextValue;
            cooldownText.gameObject.SetActive(!string.IsNullOrEmpty(cooldownTextValue));
            
            if (debug && !string.IsNullOrEmpty(cooldownTextValue))
                Debug.Log($"[SkillButton] Cooldown text set to: '{cooldownTextValue}' (active: {cooldownText.gameObject.activeSelf})");
        }

        if (button != null)
            button.interactable = assignedSkill != null && assignedSkill.IsReady;
    }

    private void SetButtonActive(bool active) => gameObject.SetActive(active);
    public void OnSkillActivated() => OnButtonClicked();
}
