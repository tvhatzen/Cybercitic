using UnityEngine;

// Handles all visual feedback for an enemy (outline, particles, effects).
[RequireComponent(typeof(HealthSystem))]
public class EnemyVisualFeedback : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material outlineMaterial;
    private Material originalMaterial;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem damageParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem spawnParticles;

    private HealthSystem healthSystem;
    private bool isTargeted = false;
    private int lastKnownHealth; // track health to detect damage

    [SerializeField] private bool debug = false;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();

        // store original material
        if (targetRenderer != null)
        {
            originalMaterial = targetRenderer.sharedMaterial;
        }

        // ensure all particle systems don't play on awake
        DisableParticleAutoPlay();
    }

    private void Start()
    {
        // initialize last known health
        if (healthSystem != null)
        {
            lastKnownHealth = healthSystem.CurrentHealth;
        }
    }

    private void DisableParticleAutoPlay()
    {
        // disable Play on Awake for all particle systems
        if (damageParticles != null)
        {
            var main = damageParticles.main;
            main.playOnAwake = false;
            damageParticles.Stop();
        }

        if (deathParticles != null)
        {
            var main = deathParticles.main;
            main.playOnAwake = false;
            deathParticles.Stop();
        }

        if (spawnParticles != null)
        {
            var main = spawnParticles.main;
            main.playOnAwake = false;
            spawnParticles.Stop();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerTargetChanged += HandleTargetChanged;

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += HandleHealthChanged;
            healthSystem.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerTargetChanged -= HandleTargetChanged;

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= HandleHealthChanged;
            healthSystem.OnDeath -= HandleDeath;
        }

        // remove outline when disabled
        RemoveOutline();
    }

    private void HandleTargetChanged(Transform oldTarget, Transform newTarget)
    {
        // check if this enemy is the new target
        if (newTarget != null && newTarget.root == transform.root)
        {
            ShowOutline();
            if (debug) Debug.Log($"[EnemyVisualFeedback] {name} is now targeted");
        }
        // check if this enemy was the old target
        else if (oldTarget != null && oldTarget.root == transform.root)
        {
            RemoveOutline();
            if (debug) Debug.Log($"[EnemyVisualFeedback] {name} is no longer targeted");
        }
    }

    private void HandleHealthChanged(int newHealth)
    {
        // only play damage effect if health decreased (took damage)
        if (newHealth < lastKnownHealth && newHealth > 0)
        {
            PlayDamageEffect();
        }
        
        // update last known health for next comparison
        lastKnownHealth = newHealth;
    }

    private void HandleDeath(HealthSystem hs)
    {
        PlayDeathEffect();
        RemoveOutline();
    }

    private void ShowOutline()
    {
        if (targetRenderer != null && outlineMaterial != null && !isTargeted)
        {
            // apply outline material (create instance to avoid modifying shared material)
            targetRenderer.material = outlineMaterial;
            isTargeted = true;
            
            if (debug) Debug.Log($"[EnemyVisualFeedback] Outline applied to {name}");
        }
    }

    private void RemoveOutline()
    {
        if (targetRenderer != null && originalMaterial != null && isTargeted)
        {
            // restore original material (use sharedMaterial to restore the original reference)
            targetRenderer.sharedMaterial = originalMaterial;
            isTargeted = false;
            
            if (debug) Debug.Log($"[EnemyVisualFeedback] Outline removed from {name}");
        }
    }

    public void PlaySpawnEffect()
    {
        if (spawnParticles != null)
        {
            spawnParticles.Play();
            if (debug) Debug.Log($"[EnemyVisualFeedback] Playing spawn effect for {name}");
        }
    }

    private void PlayDamageEffect()
    {
        // play damage particles
        if (damageParticles != null)
        {
            damageParticles.Play();
        }

        // Play damage sound
        AudioManager.Instance.PlaySound("enemyDamaged");

        if (debug) Debug.Log($"[EnemyVisualFeedback] Playing damage effect for {name}");
    }

    private void PlayDeathEffect()
    {
        // play death particles
        if (deathParticles != null)
        {
            // detach particles so they continue after object is destroyed
            deathParticles.transform.SetParent(null);
            deathParticles.Play();
            Destroy(deathParticles.gameObject, deathParticles.main.duration);
        }

        // play death sound
        AudioManager.Instance.PlaySound("enemyDie");

        if (debug) Debug.Log($"[EnemyVisualFeedback] Playing death effect for {name}");
    }
}

