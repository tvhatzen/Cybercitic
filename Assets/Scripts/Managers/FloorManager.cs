using UnityEngine;
using System.Collections.Generic;

public class FloorManager : MonoBehaviour
{
    [Header("Player")]
    public Transform playerSpawnPoint;

    [Header("Enemies")]
    public List<GameObject> enemyPrefabsForThisFloor; 
    public List<Transform> enemySpawnPoints;

    private GameObject player;

    void Start()
    {
        SpawnPlayer();
        SpawnEnemies();
    }

    void SpawnPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // player persists across floors, check if it already exists 
        if (player == null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
        }
        else
        {
            Debug.LogError("Player Instance not found.");
        }
    }

    void SpawnEnemies()
    {
        foreach (Transform point in enemySpawnPoints)
        {
            // spawn assigned enemy prefab
            int index = enemySpawnPoints.IndexOf(point);
            if (index < enemyPrefabsForThisFloor.Count)
            {
                GameObject prefab = enemyPrefabsForThisFloor[index];
                Instantiate(prefab, point.position, point.rotation);
            }
        }
    }
}
