using UnityEngine;

public class BossEnemyCombat : EnemyCombat
{
    public override void Awake()
    {
        base.Awake();

        attackCooldown = 1f;
    }
    
    protected override void AttackTarget(HealthSystem target)
    {
        base.AttackTarget(target);
    }
}
