using UnityEngine;

// Handles all visual feedback for the player (animations, attack target indicator, particles, effects)
public class PlayerVisualFeedback : MonoBehaviour
{
    [Header("Targeting Icon")]
    // the icon GameObject to display above the current target
    [SerializeField] private GameObject targetingIcon;
    private Transform currentTarget; // track current target for icon positioning

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

    private void HandleTargetChanged(Transform oldTarget, Transform newTarget) { currentTarget = newTarget;  }

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
            targetingIcon.SetActive(false); // no target, hide the icon
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

