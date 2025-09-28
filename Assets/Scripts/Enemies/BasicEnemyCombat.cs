using UnityEngine;

public class BasicEnemyCombat : EnemyCombat
{
    public override void Awake()
    {
        base.Awake();

        attackCooldown = 2f;
        damage = 5;
    }
    
    protected override void AttackTarget(HealthSystem target)
    {
        base.AttackTarget(target);
    }
}
