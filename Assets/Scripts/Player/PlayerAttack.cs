using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackCooldown = 1f;
    private float nextAttack;

    private PlayerCombat combat;

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

        // apply damage 
        var health = target.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(health.DamagePerHit); // make enemy dmg
        }
    }
}
