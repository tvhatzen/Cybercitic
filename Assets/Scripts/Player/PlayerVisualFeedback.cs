using UnityEngine;

// Handles all visual feedback for the player (attack animations, particles, effects).
public class PlayerVisualFeedback : MonoBehaviour
{
    [Header("Targeting Icon")]
    // The icon GameObject to display above the current target
    [SerializeField] private GameObject targetingIcon;
    
    // Offset position of the icon above the target
    [SerializeField] private Vector3 iconOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Animation")]
    [Tooltip("Frame-based animator for handling body part animations")]
    [SerializeField] private FrameBasedPlayerAnimator frameAnimator;
    
    [Tooltip("Name of the attack animation trigger in the Animator")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string damagedTriggerName = "Damage";

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem attackParticles;
    [SerializeField] private ParticleSystem takeDamageParticles;

    private Transform currentTarget; // track current target for icon positioning

    [SerializeField] private bool debug = false;

    private void Awake()
    {
        if (frameAnimator == null)
        {
            frameAnimator = GetComponent<FrameBasedPlayerAnimator>();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerAttack += HandlePlayerAttack;
        GameEvents.OnPlayerTakeDamage += HandlePlayerTakeDamage;
        GameEvents.OnPlayerTargetChanged += HandleTargetChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerAttack -= HandlePlayerAttack;
        GameEvents.OnPlayerTakeDamage -= HandlePlayerTakeDamage;
        GameEvents.OnPlayerTargetChanged -= HandleTargetChanged;
    }

    private void LateUpdate()
    {        
        UpdateTargetingIcon();        
    }

    private void HandlePlayerAttack(Transform target)
    {
        PlayAttackAnimation();
        PlayAttackParticles();
        if( AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("attack");

        if (debug) Debug.Log($"[PlayerVisualFeedback] Playing attack visuals for target: {target?.name ?? "none"}");
    }

    private void HandlePlayerTakeDamage()
    {
        PlayDamagedParticles();
        PlayDamagedAnimation();
    }

    private void HandleTargetChanged(Transform oldTarget, Transform newTarget)
    {
        currentTarget = newTarget;
        
        if (debug) 
        {
            string oldTargetName = (oldTarget != null) ? oldTarget.name : "none";
            string newTargetName = (newTarget != null) ? newTarget.name : "none";
            Debug.Log($"[PlayerVisualFeedback] Target changed from {oldTargetName} to {newTargetName}");
        }
    }

    private void UpdateTargetingIcon()
    {
        if (targetingIcon == null) return;

        // if there's a valid current target, show and position the icon
        if (currentTarget != null)
        {
            targetingIcon.SetActive(true);
            targetingIcon.transform.position = currentTarget.position + iconOffset;
        }
        else if (currentTarget == null || currentTarget.CompareTag("Boss"))
        {
            // no target, hide the icon
            targetingIcon.SetActive(false);
        }
    }

    // ========== ATTACK ========== //
    private void PlayAttackAnimation()
    {
        if (frameAnimator != null)
        {
            frameAnimator.PlayAttackAnimation();
        }
    }

    private void PlayAttackParticles()
    {
        if (attackParticles != null)
        {
            attackParticles.Play();
            Debug.Log("Playing player attack particles");
        }
    }

    // ========== DAMAGE ========== //
    private void PlayDamagedAnimation()
    {
        if (frameAnimator != null)
        {
            frameAnimator.PlayDamageAnimation();
        }
    }

    private void PlayDamagedParticles()
    {
        if (takeDamageParticles != null)
        {
            takeDamageParticles.Play();
        }
    }
}

