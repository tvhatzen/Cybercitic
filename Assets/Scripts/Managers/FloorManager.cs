using UnityEngine;
using System.Collections.Generic;
using System;

public class FloorManager : SingletonBase<FloorManager>
{
    [Header("Player")]
    public Transform playerSpawnPoint;

    [Header("Enemies")]
    public List<GameObject> enemyPrefabsForThisFloor; 
    public List<Transform> enemySpawnPoints;

    // event to tell others when an enemy appears
    public static event Action<GameObject> OnEnemySpawned;

    void Start()
    {
        // start in gameplay (for now)
        UIManager.Instance.ShowScreen(UIManager.MenuScreen.Gameplay);

        SpawnPlayer();
        SpawnEnemies();
    }

    void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // player persists across floors, check if it already exists 
        if (player != null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
        }
        else
            Debug.LogError("Player Instance not found.");
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemySpawnPoints.Count; i++)
        {
            if (i < enemyPrefabsForThisFloor.Count)
            {
                GameObject prefab = enemyPrefabsForThisFloor[i];
                Transform point = enemySpawnPoints[i];

                var enemy = Instantiate(prefab, point.position, point.rotation);
                
                // fire event for each enemy
                OnEnemySpawned?.Invoke(enemy);
            }
        }
    }
}
