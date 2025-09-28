using UnityEngine;

[CreateAssetMenu(fileName = "LootDrop", menuName = "Scriptable Objects/LootDrop")]
public class LootDrop : ScriptableObject
{
    public GameObject prefab;   // what to spawn in the world
    public int dropChance = 100; // % chance, 100 for now
    public int minAmount = 1;
    public int maxAmount = 1;

    public int GetAmount()
    {
        return Random.Range(minAmount, maxAmount + 1);
    }
}
