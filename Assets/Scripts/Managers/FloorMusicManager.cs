using System;
using UnityEngine;

public class FloorMusicManager
{
    private readonly FloorManager floorManager;

    public FloorMusicManager(FloorManager floorManager)
    {
        this.floorManager = floorManager ?? throw new ArgumentNullException(nameof(floorManager));
    }

    public void PlayForFloor(int floor)
    {
        if (AudioManager.Instance == null)
            return;

        string musicTrack = GetMusicTrackForFloor(floor);
        AudioManager.Instance.PlayMusicTrack(musicTrack);

        if (floorManager.debug)
            Debug.Log($"[FloorMusicManager] Playing music track '{musicTrack}' for floor {floor}");
    }

    private static string GetMusicTrackForFloor(int floor)
    {
        if (floor >= 1 && floor <= 5)
        {
            return "floor1_5";
        }

        if (floor >= 6 && floor <= 10)
        {
            return "floor6_10";
        }

        if (floor >= 11 && floor <= 15)
        {
            return "floor11_15";
        }

        Debug.LogWarning($"[FloorMusicManager] Floor {floor} is outside expected range (1-15), defaulting to floor1_5 music");
        return "floor1_5";
    }
}

