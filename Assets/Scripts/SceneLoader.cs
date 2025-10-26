using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
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
    
    private void Awake()
    {
        // Make this a singleton to persist across scene loads
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize transition image if not assigned
            if (transitionImage == null)
            {
                CreateTransitionImage();
            }
            
            transitionRectTransform = transitionImage.GetComponent<RectTransform>();
            
            // Start with transition image off-screen
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
        
        // Only load PersistentScene if it's not already loaded
        if (!SceneManager.GetSceneByName("PersistentScene").isLoaded)
        {
            SceneManager.LoadScene("PersistentScene", LoadSceneMode.Additive);
        }
    }
    
    private void CreateTransitionImage()
    {
        // Look for existing transition canvas first
        Canvas transitionCanvas = GameObject.Find("TransitionCanvas")?.GetComponent<Canvas>();
        
        if (transitionCanvas == null)
        {
            // Create a canvas for the transition
            GameObject canvasGO = new GameObject("TransitionCanvas");
            transitionCanvas = canvasGO.AddComponent<Canvas>();
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 1000; // Ensure it's on top
            
            // Make canvas persistent across scene loads
            DontDestroyOnLoad(canvasGO);
            
            // Ensure canvas is active
            canvasGO.SetActive(true);
            
            if (debug) Debug.Log("[SceneLoader] Created new transition canvas");
        }
        else
        {
            if (debug) Debug.Log("[SceneLoader] Found existing transition canvas");
        }
        
        // Create the transition image
        GameObject imageGO = new GameObject("TransitionImage");
        imageGO.transform.SetParent(transitionCanvas.transform, false);
        
        transitionImage = imageGO.AddComponent<Image>();
        transitionImage.color = Color.black; // Default to black, can be changed in inspector
        
        // Set up RectTransform to cover full screen
        transitionRectTransform = imageGO.GetComponent<RectTransform>();
        transitionRectTransform.anchorMin = Vector2.zero;
        transitionRectTransform.anchorMax = Vector2.one;
        transitionRectTransform.offsetMin = Vector2.zero;
        transitionRectTransform.offsetMax = Vector2.zero;
        
        // Ensure the image is active and visible
        imageGO.SetActive(true);
        transitionImage.enabled = true;
        
        // Set initial position off-screen
        transitionRectTransform.anchoredPosition = new Vector2(-Screen.width, 0);
        
        if (debug) Debug.Log("[SceneLoader] Created transition image UI element");
    }
    
    // Static method to access the SceneLoader from anywhere
    public static SceneLoader Instance
    {
        get
        {
            if (instance == null)
            {
                // Try to find existing SceneLoader in the scene
                instance = FindObjectOfType<SceneLoader>();
                if (instance == null)
                {
                    // Create a new SceneLoader if none exists
                    GameObject loaderGO = new GameObject("SceneLoader");
                    instance = loaderGO.AddComponent<SceneLoader>();
                    DontDestroyOnLoad(loaderGO);
                }
            }
            return instance;
        }
    }
    
    public void LoadSceneWithTransition(string sceneName)
    {
        if (isTransitioning)
        {
            if (debug) Debug.LogWarning("[SceneLoader] Already transitioning, ignoring request");
            return;
        }
        
        // Ensure transition image exists
        if (transitionImage == null || transitionRectTransform == null)
        {
            if (debug) Debug.LogWarning("[SceneLoader] Transition image missing, recreating...");
            CreateTransitionImage();
        }
        
        StartCoroutine(TransitionAndLoadScene(sceneName));
    }
    
    // Static method to load scene with transition from anywhere
    public static void LoadScene(string sceneName)
    {
        //if (debug) Debug.Log($"[SceneLoader] Static LoadScene called for: {sceneName}");
        
        // Ensure the SceneLoader instance is properly initialized
        var loader = Instance;
        if (loader.transitionImage == null)
        {
            //if (debug) Debug.Log("[SceneLoader] Transition image missing, recreating...");
            loader.CreateTransitionImage();
        }
        
        loader.LoadSceneWithTransition(sceneName);
    }
    
    // Test method to check if transition image is working
    public void TestTransition()
    {
        if (debug) Debug.Log("[SceneLoader] Testing transition animation");
        StartCoroutine(TestTransitionAnimation());
    }
    
    private IEnumerator TestTransitionAnimation()
    {
        if (transitionImage == null)
        {
            if (debug) Debug.LogError("[SceneLoader] No transition image found, creating one");
            CreateTransitionImage();
        }
        
        // Test slide in
        yield return StartCoroutine(SlideTransition(true));
        
        // Wait a moment
        yield return new WaitForSeconds(0.5f);
        
        // Test slide out
        yield return StartCoroutine(SlideTransition(false));
        
        if (debug) Debug.Log("[SceneLoader] Test transition completed");
    }
    
    private IEnumerator TransitionAndLoadScene(string sceneName)
    {
        isTransitioning = true;
        
        if (debug) Debug.Log($"[SceneLoader] Starting transition to scene: {sceneName}");
        
        // Phase 1: Slide in from left
        yield return StartCoroutine(SlideTransition(true));
        
        // Phase 2: Load the scene
        if (debug) Debug.Log($"[SceneLoader] Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
        
        // Wait for the scene to fully load and initialize
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
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
