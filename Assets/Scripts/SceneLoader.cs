using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : SingletonBase<SceneLoader>
{
    [Header("Transition Animation")]
    [SerializeField] private Image transitionImage;
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Debug")]
    public bool debug = false;
    
    private bool isTransitioning = false;
    private RectTransform transitionRectTransform;
    private static SceneLoader instance;
    
    protected override void Awake()
    {
        base.Awake();

        // make this a singleton to persist across scene loads
        if (instance == null)
        {
            transitionRectTransform = transitionImage.GetComponent<RectTransform>();

            // start with transition image off-screen
            if (transitionRectTransform != null)
            {
                transitionRectTransform.anchoredPosition = new Vector2(-Screen.width, 0);
            }
            
            if (debug) Debug.Log("[SceneLoader] Singleton instance created and initialized");
        }
        else
        {
            if (debug) Debug.Log("[SceneLoader] Duplicate instance found, destroying");
            Destroy(gameObject);
            return;
        }
    }
    
    public void LoadSceneWithTransition(string sceneName)
    {
        if (isTransitioning)
        {
            if (debug) Debug.LogWarning("[SceneLoader] Already transitioning, ignoring request");
            return;
        }
        
        // ensure transition image exists
        if (transitionImage == null || transitionRectTransform == null)
        {
            if (debug) Debug.LogWarning("[SceneLoader] Transition image missing, recreating...");
        }
        
        StartCoroutine(TransitionAndLoadScene(sceneName));
    }
    
    public static void LoadScene(string sceneName)
    {
        // ensure the SceneLoader instance is properly initialized
        var loader = Instance;
        loader.LoadSceneWithTransition(sceneName);
    }
    
    private IEnumerator TransitionAndLoadScene(string sceneName)
    {
        isTransitioning = true;
        
        if (debug) Debug.Log($"[SceneLoader] Starting transition to scene: {sceneName}");
        
        // Phase 1: Slide in from left
        yield return StartCoroutine(SlideTransition(true));

        // play scene transition sound
        AudioManager.Instance.PlaySound("levelTransition");
        
        // Phase 2: Load the scene
        if (debug) Debug.Log($"[SceneLoader] Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
        
        // Wait for the scene to fully load and initialize
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Verify the scene actually loaded
        if (debug) Debug.Log($"[SceneLoader] Scene loaded. Current scene: {SceneManager.GetActiveScene().name}");
        
        // Phase 3: Slide out to right
        if (debug) Debug.Log("[SceneLoader] Starting slide-out animation");
        yield return StartCoroutine(SlideTransition(false));
        
        isTransitioning = false;
        
        if (debug) Debug.Log("[SceneLoader] Transition completed");
    }
    
    private IEnumerator SlideTransition(bool slideIn)
    {
        if (transitionImage == null || transitionRectTransform == null)
        {
            if (debug) Debug.LogError("[SceneLoader] Transition image or RectTransform is null!");
            yield break;
        }
        
        // Ensure the image is visible and active
        transitionImage.gameObject.SetActive(true);
        transitionImage.enabled = true;
        
        float elapsedTime = 0f;
        Vector2 startPosition;
        Vector2 endPosition;
        
        if (slideIn)
        {
            // Start off-screen left, slide to center
            startPosition = new Vector2(-Screen.width, 0);
            endPosition = Vector2.zero;
            if (debug) Debug.Log("[SceneLoader] Starting slide-in animation");
        }
        else
        {
            // Start at center, slide off-screen right
            startPosition = Vector2.zero;
            endPosition = new Vector2(Screen.width, 0);
            if (debug) Debug.Log("[SceneLoader] Starting slide-out animation");
        }
        
        // Set initial position
        transitionRectTransform.anchoredPosition = startPosition;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            float curveValue = transitionCurve.Evaluate(progress);
            
            Vector2 currentPosition = Vector2.Lerp(startPosition, endPosition, curveValue);
            transitionRectTransform.anchoredPosition = currentPosition;
            
            if (debug && elapsedTime % 0.1f < Time.deltaTime) // Log every 0.1 seconds
            {
                Debug.Log($"[SceneLoader] Transition progress: {progress:F2}, Position: {currentPosition}");
            }
            
            yield return null;
        }
        
        // Ensure final position
        transitionRectTransform.anchoredPosition = endPosition;
        
        if (debug) Debug.Log($"[SceneLoader] Transition completed. Final position: {endPosition}");
    }
}
