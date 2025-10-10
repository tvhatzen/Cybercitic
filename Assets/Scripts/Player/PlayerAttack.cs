using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackCooldown = 1f;
    private float nextAttack;

    private PlayerCombat combat;

    public bool debug = false;

    void Awake() => combat = GetComponent<PlayerCombat>();

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
