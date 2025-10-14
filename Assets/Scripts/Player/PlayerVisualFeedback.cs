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

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem attackParticles;
    [SerializeField] private ParticleSystem combatEnterParticles;
    [SerializeField] private ParticleSystem combatExitParticles;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip combatEnterSound;
    [SerializeField] private AudioClip combatExitSound;

    private AudioSource audioSource;
    private Transform currentTarget; // Track current target for icon positioning

    [Header("DEBUG")]
    [SerializeField] private bool debug = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
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
        GameEvents.OnPlayerEnterCombat += HandleCombatEnter;
        GameEvents.OnPlayerExitCombat += HandleCombatExit;
        GameEvents.OnPlayerTargetChanged += HandleTargetChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from combat events
        GameEvents.OnPlayerAttack -= HandlePlayerAttack;
        GameEvents.OnPlayerEnterCombat -= HandleCombatEnter;
        GameEvents.OnPlayerExitCombat -= HandleCombatExit;
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
        PlayAttackSound();

        if (debug) Debug.Log($"[PlayerVisualFeedback] Playing attack visuals for target: {target?.name ?? "none"}");
    }

    private void HandleCombatEnter(Transform[] enemies)
    {
        PlayCombatEnterEffect();
        
        if (debug) Debug.Log($"[PlayerVisualFeedback] Entering combat with {enemies.Length} enemies");
    }

    private void HandleCombatExit()
    {
        PlayCombatExitEffect();
        
        if (debug) Debug.Log("[PlayerVisualFeedback] Exiting combat");
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

    private void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    private void PlayCombatEnterEffect()
    {
        if (combatEnterParticles != null)
        {
            combatEnterParticles.Play();
        }

        if (audioSource != null && combatEnterSound != null)
        {
            audioSource.PlayOneShot(combatEnterSound);
        }
    }

    private void PlayCombatExitEffect()
    {
        if (combatExitParticles != null)
        {
            combatExitParticles.Play();
        }

        if (audioSource != null && combatExitSound != null)
        {
            audioSource.PlayOneShot(combatExitSound);
        }
    }
}

