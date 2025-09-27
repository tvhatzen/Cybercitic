using System.Collections;
using UnityEngine;

public class CombatManager : SingletonBase<CombatManager>
{
    private HealthSystem playerHealth;
    private HealthSystem enemyHealth;

    [SerializeField] private float playerAttackRate = 1.0f; // attacks per second
    [SerializeField] private float enemyAttackRate = 1.5f;

    private Coroutine combatRoutine;

    void OnEnable()
    {
        GameEvents.OnPlayerEnterCombat += HandleEnterCombat;
        GameEvents.OnPlayerExitCombat += HandleExitCombat;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerEnterCombat -= HandleEnterCombat;
        GameEvents.OnPlayerExitCombat -= HandleExitCombat;
    }

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerController>().GetComponent<HealthSystem>();
    }

    public void HandleEnterCombat(Transform enemy)
    {        
        Debug.Log("Player entered combat with " + enemy.name);

        enemyHealth = enemy.GetComponent<HealthSystem>();
        
        // subscribe to deaths to exit combat automatically
        playerHealth.OnDeath += OnAnyDeath;
        enemyHealth.OnDeath += OnAnyDeath;

        combatRoutine = StartCoroutine(CombatLoop(enemyHealth));
    }

    void HandleExitCombat()
    {
        Debug.Log("Player exited combat");

        if (combatRoutine != null)
        {
            StopCoroutine(combatRoutine);
            combatRoutine = null;
        }

        // unsubscribe
        if (enemyHealth != null)
        {
            playerHealth.OnDeath -= OnAnyDeath;
            enemyHealth.OnDeath -= OnAnyDeath;
        }

        enemyHealth = null;
    }

    void OnAnyDeath(HealthSystem dead)
    {
        // automatically exit combat
        HandleExitCombat();
    }

    private IEnumerator CombatLoop(HealthSystem enemy)
    {
        Debug.Log("entered combat loop coroutine");

        float playerTimer = 0f;
        float enemyTimer = 0f;

        while (playerHealth.CurrentHealth > 0 && enemy.CurrentHealth > 0)
        {
            playerTimer += Time.deltaTime;
            enemyTimer += Time.deltaTime;

            if (playerTimer >= 1f / playerAttackRate)
            {
                playerTimer = 0f;
                Attack(playerHealth, enemy);
            }

            if (enemyTimer >= 1f / enemyAttackRate)
            {
                enemyTimer = 0f;
                Attack(enemy, playerHealth);
            }

            yield return null;
        }
    }

    private void Attack(HealthSystem attacker, HealthSystem target)
    {
        target.TakeDamage(attacker.DamagePerHit);
        Debug.Log(attacker.name + " attacks " + target.name);
    }
}
