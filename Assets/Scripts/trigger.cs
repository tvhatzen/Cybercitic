using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

// Static class to recreate triggers when they get destroyed
public static class TriggerRecreator
{
    public static void RecreateTrigger(Vector3 position, Quaternion rotation, bool loadNewScene, string nextFloorSceneName)
    {
        // Find a MonoBehaviour to start the coroutine
        MonoBehaviour coroutineRunner = Object.FindObjectOfType<FloorManager>();
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(RecreateTriggerCoroutine(position, rotation, loadNewScene, nextFloorSceneName));
        }
        else
        {
            Debug.LogError("[TriggerRecreator] No MonoBehaviour found to start coroutine!");
        }
    }
    
    private static System.Collections.IEnumerator RecreateTriggerCoroutine(Vector3 position, Quaternion rotation, bool loadNewScene, string nextFloorSceneName)
    {
        yield return null; // Wait one frame
        
        // Check if trigger still exists
        trigger existingTrigger = Object.FindObjectOfType<trigger>();
        if (existingTrigger == null)
        {
            Debug.Log("[TriggerRecreator] No trigger found, recreating...");
            
            // Create a new trigger GameObject
            GameObject newTrigger = new GameObject("NextLevelTrigger_Recreated");
            newTrigger.AddComponent<BoxCollider>().isTrigger = true;
            
            // Set up the collider with proper size
            BoxCollider collider = newTrigger.GetComponent<BoxCollider>();
            collider.size = new Vector3(2f, 2f, 2f); // Make it larger so player doesn't miss it
            
            // Add the trigger component
            trigger triggerComponent = newTrigger.AddComponent<trigger>();
            
            // Configure the trigger settings
            triggerComponent.loadNewScene = loadNewScene;
            triggerComponent.nextFloorSceneName = nextFloorSceneName;
            triggerComponent.debug = true; // Enable debug for the recreated trigger
            
            // Position it at the same location as the original
            newTrigger.transform.position = position;
            newTrigger.transform.rotation = rotation;
            
            Debug.Log($"[TriggerRecreator] Trigger recreated at position: {newTrigger.transform.position}");
            Debug.Log($"[TriggerRecreator] Settings - loadNewScene: {loadNewScene}, nextFloorSceneName: {nextFloorSceneName}");
        }
        else
        {
            Debug.Log("[TriggerRecreator] Trigger already exists, skipping recreation");
        }
    }
}

[RequireComponent(typeof(Collider))]
public class trigger : MonoBehaviour
{
    [Header("Next Floor Settings")]
    public bool loadNewScene = false; // if true, loads a new scene; if false, progresses floor in same scene
    public string nextFloorSceneName; // scene to load when triggered (only if loadNewScene is true)

    public bool debug = false;
    
    private bool hasBeenTriggered = false; // prevent multiple triggers
    private bool isBeingDestroyed = false; // prevent accidental destruction

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        
        // Ensure this trigger is not destroyed by SceneLoader
        if (transform.parent != null && transform.parent.name.Contains("SceneLoader"))
        {
            if(debug) Debug.LogWarning($"[Trigger] {gameObject.name} is child of SceneLoader! Moving to scene root.");
            transform.SetParent(null);
        }
        
        // Mark this trigger as protected from accidental destruction
        gameObject.tag = "Untagged"; // Remove any tags that might cause issues
        gameObject.name = "NextLevelTrigger_Protected"; // Rename to avoid conflicts
        
        // Add a small delay to ensure the trigger is fully initialized before any cleanup
        StartCoroutine(EnsureTriggerSurvival());
        
        // Add a coroutine that continuously checks if the trigger is still alive
        StartCoroutine(MonitorTriggerHealth());
        
        if(debug) Debug.Log($"[Trigger] {gameObject.name} Awake called");
        if(debug) Debug.Log($"[Trigger] Parent: {(transform.parent != null ? transform.parent.name : "None")}");
        if(debug) Debug.Log($"[Trigger] Scene: {gameObject.scene.name}");
    }
    
    private void OnDestroy()
    {
        if(debug) Debug.Log($"[Trigger] {gameObject.name} is being destroyed!");
        if(debug) Debug.Log($"[Trigger] Parent when destroyed: {(transform.parent != null ? transform.parent.name : "None")}");
        if(debug) Debug.Log($"[Trigger] Scene when destroyed: {gameObject.scene.name}");
        
        // Log the stack trace to see what's calling Destroy
        if(debug) Debug.Log($"[Trigger] Stack trace: {System.Environment.StackTrace}");
        
        // If this is an accidental destruction, try to prevent it
        if (!isBeingDestroyed && gameObject.scene.isLoaded)
        {
            if(debug) Debug.LogWarning($"[Trigger] {gameObject.name} was accidentally destroyed! This should not happen!");
            
            // Try to recreate the trigger immediately using a static method
            TriggerRecreator.RecreateTrigger(transform.position, transform.rotation, loadNewScene, nextFloorSceneName);
        }
        
        isBeingDestroyed = true;
    }
    
    
    private System.Collections.IEnumerator EnsureTriggerSurvival()
    {
        // Wait a frame to ensure everything is initialized
        yield return null;
        
        // Check if the trigger is still alive
        if (gameObject != null && gameObject.activeInHierarchy)
        {
            if(debug) Debug.Log($"[Trigger] {gameObject.name} survived initialization!");
        }
        else
        {
            if(debug) Debug.LogWarning($"[Trigger] {gameObject.name} was destroyed during initialization!");
        }
    }
    
    private System.Collections.IEnumerator MonitorTriggerHealth()
    {
        // Monitor the trigger for 5 seconds after creation
        for (int i = 0; i < 50; i++) // Check every 0.1 seconds for 5 seconds
        {
            yield return new WaitForSeconds(0.1f);
            
            if (gameObject == null || !gameObject.activeInHierarchy)
            {
                if(debug) Debug.LogWarning($"[Trigger] {gameObject?.name ?? "Unknown"} was destroyed after {i * 0.1f} seconds!");
                break;
            }
        }
        
        if(debug) Debug.Log($"[Trigger] {gameObject.name} monitoring completed - trigger is still alive!");
    }
    
    // Method to safely destroy the trigger
    public void SafeDestroy()
    {
        if(debug) Debug.Log($"[Trigger] {gameObject.name} safely destroying...");
        isBeingDestroyed = true;
        Destroy(gameObject);
    }
    
    private void OnDisable()
    {
        if(debug) Debug.Log($"[Trigger] {gameObject.name} is being disabled!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            hasBeenTriggered = true; // prevent re-triggering
            
            if(debug) Debug.Log($"[Trigger] Player reached floor end! Current floor: {FloorManager.Instance?.CurrentFloor ?? -1}");
            if(debug) Debug.Log($"[Trigger] loadNewScene: {loadNewScene}, nextFloorSceneName: {nextFloorSceneName}");
            if(debug) Debug.Log($"[Trigger] GameObject active: {gameObject.activeInHierarchy}, enabled: {enabled}");
            
            LoadNextFloor();
        }
        else if (other.CompareTag("Player") && hasBeenTriggered)
        {
            if(debug) Debug.Log("[Trigger] Player entered trigger but it has already been triggered, ignoring");
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

                // Load the scene with transition animation
                SceneLoader.LoadScene(nextFloorSceneName);
            }
            else
            {
                if(debug) Debug.LogWarning("[Trigger] Next floor scene name not set on trigger!");
            }
        }
        else
        {
            // Handle same-scene floor progression
            if(debug) Debug.Log("[Trigger] Same-scene floor progression");
            FloorManager.Instance.LoadNextFloor();
        }
    }
}