using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackCooldown = 1f;
    private float nextAttack;

    private PlayerCombat combat;
    private FrameBasedPlayerAnimator frameAnimator;

    public bool debug = false;

    void Awake() 
    {
        combat = GetComponent<PlayerCombat>();
        frameAnimator = GetComponent<FrameBasedPlayerAnimator>();
    }

    void Update()
    {
        if (combat.InCombat && combat.CurrentTarget != null)
        {
            if (Time.time >= nextAttack)
            {
                nextAttack = Time.time + attackCooldown;
                AttackTarget(combat.CurrentTarget);
                // later have damage / animation 
            }
        }
    }

    private void AttackTarget(Transform target)
    {
        // Play attack animation only if not already playing an attack
        if (frameAnimator != null && !frameAnimator.IsPlaying())
        {
            frameAnimator.PlayAttackAnimation();
            if (debug) Debug.Log("[PlayerAttack] Playing attack animation");
        }

        // Play attack sound via Event Bus
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
                health.TakeDamage(damage); // use PLAYER's damage
                
                var playerData = GetComponent<EntityData>();
                if(debug) Debug.Log($"[PlayerAttack] Player attacks {target.name} for {damage} damage (EntityData.currentAttack: {playerData?.currentAttack})");
            }
        }
    }
}
