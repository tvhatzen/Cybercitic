using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float baseAttackCooldown = 1f;
    [Tooltip("How much speed affects attack cooldown. Higher values = more speed impact.")]
    public float speedMultiplier = 0.1f;

    private float nextAttack;

    private PlayerCombat combat;
    private FrameBasedPlayerAnimator frameAnimator;
    private EntityData playerData;

    // speed mutiplier testing variable
    public float SpeedMultiplier => playerData != null ? playerData.currentSpeedMultiplier : 0;

    public bool debug = false;

    void Awake() 
    {
        combat = GetComponent<PlayerCombat>();
        frameAnimator = GetComponent<FrameBasedPlayerAnimator>();
        playerData = GetComponent<EntityData>();
        
        if (playerData == null && debug)
        {
            Debug.LogWarning($"[PlayerAttack] No EntityData found on {name}. Speed upgrades will not work.");
        }
    }

    void Update()
    {
        if (combat.InCombat && combat.CurrentTarget != null)
        {
            if (Time.time >= nextAttack)
            {
                float dynamicCooldown = CalculateAttackCooldown();
                nextAttack = Time.time + dynamicCooldown;
                AttackTarget(combat.CurrentTarget);
            }
        }
    }
    
    // attack cooldown based on player's speed upgrade
    private float CalculateAttackCooldown()
    {
        if (playerData == null)
        {
            return baseAttackCooldown;
        }

        // Speed reduces cooldown: higher speed = faster attacks
        // Formula: cooldown = baseCooldown / (1 + speed * speedMultiplier)
        float speedBonus = playerData.currentSpeed * SpeedMultiplier;
        Debug.Log($"current speed multiplier: {SpeedMultiplier}");

        float dynamicCooldown = baseAttackCooldown / (1f + speedBonus);
        
        // Ensure minimum cooldown to prevent instant attacks
        float minCooldown = 0.1f;
        dynamicCooldown = Mathf.Max(minCooldown, dynamicCooldown);
        
        if (debug)
        {
            Debug.Log($"[PlayerAttack] Speed: {playerData.currentSpeed:F2}, Cooldown: {dynamicCooldown:F2}s (Base: {baseAttackCooldown}s)");
        }
        
        return dynamicCooldown;
    }

    private void AttackTarget(Transform target)
    {
        // play attack animation only if not already playing an attack
        if (frameAnimator != null && !frameAnimator.IsPlaying())
        {
            frameAnimator.PlayAttackAnimation();
            if (debug) Debug.Log("[PlayerAttack] Playing attack animation");
        }

        // play attack sound via Event Bus
        GameEvents.RequestSound("attack");
        if (debug) Debug.Log("[PlayerAttack] Requesting attack sound via Event Bus");

        GameEvents.PlayerAttack(target);

        // apply damage using PLAYER damage
        var health = target.GetComponent<HealthSystem>();
        if (health != null)
        {
            var playerHealth = GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                int damage = playerHealth.DamagePerHit;
                health.TakeDamage(damage); 
                
                var playerData = GetComponent<EntityData>();
                if(debug) Debug.Log($"[PlayerAttack] Player attacks {target.name} for {damage} damage (EntityData.currentAttack: {playerData?.currentAttack})");
            }
        }
    }
}
