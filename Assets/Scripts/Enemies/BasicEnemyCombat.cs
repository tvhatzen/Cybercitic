using UnityEngine;

public class BasicEnemyCombat : EnemyCombat
{
    public override void Awake()
    {
        base.Awake();

        attackCooldown = 2f;
    }
    
    protected override void AttackTarget(HealthSystem target)
    {
        base.AttackTarget(target);
    }
}