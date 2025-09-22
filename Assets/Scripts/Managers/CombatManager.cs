using UnityEngine;

public class CombatManager : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.OnPlayerEnterCombat += HandleEnterCombat;
        GameEvents.OnPlayerAttack += HandleAttack;
        GameEvents.OnPlayerExitCombat += HandleExitCombat;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerEnterCombat -= HandleEnterCombat;
        GameEvents.OnPlayerAttack -= HandleAttack;
        GameEvents.OnPlayerExitCombat -= HandleExitCombat;
    }

    void HandleEnterCombat(Transform enemy)
    {
        Debug.Log("Player entered combat with " + enemy.name);
    }

    void HandleAttack(Transform target)
    {
        Debug.Log("Player attacks " + target.name);
    }

    void HandleExitCombat()
    {
        Debug.Log("Player exited combat");
    }

    void Start(){
        // instantiate player
        //Instantiate(Resources.Load("Player"), new Vector3(0, 0, 0), Quaternion.identity);
    }
}
