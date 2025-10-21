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
    [Tooltip("Animator component for playing attack animations")]
    [SerializeField] private Animator animator;
    
    [Tooltip("Name of the attack animation trigger in the Animator")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string damagedTriggerName = "Damage";

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem attackParticles;
    [SerializeField] private ParticleSystem takeDamageParticles;

    private Transform currentTarget; // Track current target for icon positioning

    [SerializeField] private bool debug = false;

    private void Awake()
    {
        // If no animator is assigned, try to find one
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        // Subscribe to combat events
        GameEvents.OnPlayerAttack += HandlePlayerAttack;
        GameEvents.OnPlayerTakeDamage += HandlePlayerTakeDamage;
        GameEvents.OnPlayerTargetChanged += HandleTargetChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from combat events
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
            AudioManager.Instance.PlaySound(AudioManager.attack);

        if (debug) Debug.Log($"[PlayerVisualFeedback] Playing attack visuals for target: {target?.name ?? "none"}");
    }

    private void HandlePlayerTakeDamage()
    {
        PlayDamagedAnimation();
        PlayDamagedParticles();
    }

    private void HandleTargetChanged(Transform oldTarget, Transform newTarget)
    {
        currentTarget = newTarget;
        
        if (debug) Debug.Log($"[PlayerVisualFeedback] Target changed from {oldTarget?.name ?? "none"} to {newTarget?.name ?? "none"}");
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
        else
        {
            // no target, hide the icon
            targetingIcon.SetActive(false);
        }
    }

    // ========== ATTACK ========== //
    private void PlayAttackAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }
    }

    private void PlayAttackParticles()
    {
        if (attackParticles != null)
        {
            attackParticles.Play();
        }
    }

    // ========== DAMAGE ========== //
    private void PlayDamagedAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(damagedTriggerName))
        {
            animator.SetTrigger(damagedTriggerName);
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

