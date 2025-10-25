using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider))]
public class trigger : MonoBehaviour
{
    [Header("Next Floor Settings")]
    public bool loadNewScene = false; // if true, loads a new scene; if false, progresses floor in same scene
    public string nextFloorSceneName; // scene to load when triggered (only if loadNewScene is true)

    public bool debug = false;
    
    private bool hasBeenTriggered = false; // prevent multiple triggers

    private SceneLoader sceneLoader;

    private void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            hasBeenTriggered = true; // prevent re-triggering
            
            if(debug) Debug.Log($"[Trigger] Player reached floor end!");
            LoadNextFloor();
        }
    }

    private void LoadNextFloor()
    {
        if (loadNewScene)
        {
            // load a new scene
            if (!string.IsNullOrEmpty(nextFloorSceneName))
            {
                if(debug) Debug.Log($"[Trigger] Loading new scene: {nextFloorSceneName}");
                
                // increment floor BEFORE loading scene so OnSceneLoaded has correct floor number
                int currentFloorBefore = FloorManager.Instance.CurrentFloor;
                FloorManager.Instance.IncrementFloor();
                if(debug) Debug.Log($"[Trigger] Floor incremented from {currentFloorBefore} to {FloorManager.Instance.CurrentFloor}");

                // Load the scene - OnSceneLoaded will handle setting state to Playing
                //SceneManager.LoadScene(nextFloorSceneName);
                sceneLoader.LoadSceneWithTransition(nextFloorSceneName);
            }
            else
            {
                if(debug) Debug.LogWarning("[Trigger] Next floor scene name not set on trigger!");
            }
        }
    }
}