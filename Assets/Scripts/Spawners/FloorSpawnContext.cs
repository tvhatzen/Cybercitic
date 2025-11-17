using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct FloorSpawnContext
{
    public FloorManager Manager { get; }
    public int Floor { get; }
    public bool IsBossFloor { get; }
    public GameObject PlayerPrefab { get; }
    public Transform PlayerSpawnPoint { get; }
    public IReadOnlyList<GameObject> EnemyPrefabs { get; }
    public IReadOnlyList<Transform> EnemySpawnPoints { get; }
    public GameObject BossPrefab { get; }
    public Transform BossSpawnPoint { get; }

    public FloorSpawnContext(
        FloorManager manager,
        GameObject playerPrefab,
        Transform playerSpawnPoint,
        IReadOnlyList<GameObject> enemyPrefabs,
        IReadOnlyList<Transform> enemySpawnPoints,
        GameObject bossPrefab,
        Transform bossSpawnPoint,
        int floor,
        bool isBossFloor)
    {
        Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        PlayerPrefab = playerPrefab;
        PlayerSpawnPoint = playerSpawnPoint;
        EnemyPrefabs = enemyPrefabs ?? Array.Empty<GameObject>();
        EnemySpawnPoints = enemySpawnPoints ?? Array.Empty<Transform>();
        BossPrefab = bossPrefab;
        BossSpawnPoint = bossSpawnPoint;
        Floor = floor;
        IsBossFloor = isBossFloor;
    }
}