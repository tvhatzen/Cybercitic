using UnityEngine;

// Handles all visual feedback for the player (animations, attack target indicator, particles, effects)
public class PlayerVisualFeedback : MonoBehaviour
{
    [Header("Targeting Icon")]
    // the icon GameObject to display above the current target
    [SerializeField] private GameObject targetingIcon;
    private TargetingIconAnim targetingIconAnim; // animation component for the targeting icon

    private Transform currentTarget; // track current target for icon positioning
    private Transform previousTarget; // track previous target to detect changes
    private bool animationStartedForCurrentTarget = false; // track if animation has started for current target

    // offset position of the icon above the target
    [SerializeField] private Vector3 iconOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Animation")]
    [Tooltip("Frame-based animator for handling body part animations")]
    [SerializeField] private FrameBasedPlayerAnimator frameAnimator;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem attackParticles;
    [SerializeField] private ParticleSystem takeDamageParticles;

    //[SerializeField] private bool debug = false;

    private void Awake()
    {
        if (frameAnimator == null)
        {
            frameAnimator = GetComponent<FrameBasedPlayerAnimator>();
        }

        // Get or add TargetingIconAnim component to the targeting icon
        if (targetingIcon != null)
        {
            targetingIconAnim = targetingIcon.GetComponent<TargetingIconAnim>();
            if (targetingIconAnim == null)
            {
                targetingIconAnim = targetingIcon.AddComponent<TargetingIconAnim>();
            }
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
    }

    private void HandlePlayerTakeDamage()
    {
        PlayDamagedAnimation();
        PlayDamagedParticles();
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("damaged");
    }

    private void HandleTargetChanged(Transform oldTarget, Transform newTarget) 
    { 
        previousTarget = currentTarget;
        currentTarget = newTarget;
        animationStartedForCurrentTarget = false; // Reset flag when target changes
    }

    private void UpdateTargetingIcon()
    {
        if (targetingIcon == null) return;

        // if there's a valid current target, show and position the icon
        if (currentTarget != null && !currentTarget.CompareTag("Boss"))
        {
            bool wasInactive = !targetingIcon.activeSelf;
            
            // Ensure the icon is active
            if (wasInactive)
            {
                targetingIcon.SetActive(true);
            }
            
            // Always update position every frame to follow the target (even if it moves)
            targetingIcon.transform.position = currentTarget.position + iconOffset;
            
            // Start animation only once per target - when target changes or icon was just activated
            if (!animationStartedForCurrentTarget && (currentTarget != previousTarget || wasInactive))
            {
                if (targetingIconAnim != null)
                {
                    targetingIconAnim.StartAnimation();
                    animationStartedForCurrentTarget = true; // Mark that we've started animation for this target
                    previousTarget = currentTarget; // Update previousTarget so condition becomes false on next frame
                }
            }
        }
        else
        {
            // no target, hide the icon and stop animation
            if (targetingIcon.activeSelf)
            {
                targetingIcon.SetActive(false);
                
                if (targetingIconAnim != null)
                {
                    targetingIconAnim.StopAnimation();
                }
                
                animationStartedForCurrentTarget = false; // Reset flag when no target
            }
        }
    }

    // ========== ATTACK ========== //
    private void PlayAttackAnimation()
    {
        if (frameAnimator != null) { frameAnimator.PlayAttackAnimation(); }
    }

    private void PlayAttackParticles()
    {
        if (attackParticles != null) { attackParticles.Play(); }
    }

    // ========== DAMAGE ========== //
    private void PlayDamagedAnimation()
    {
        if (frameAnimator != null) { frameAnimator.PlayDamageAnimation(); }
    }

    private void PlayDamagedParticles()
    {
        if (takeDamageParticles != null) { takeDamageParticles.Play(); }
    }
}

