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
    
    private void Awake()
    {
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
        
        SceneManager.LoadScene("PersistentScene", LoadSceneMode.Additive);
    }
    
    private void CreateTransitionImage()
    {
        // Create a canvas for the transition if it doesn't exist
        Canvas transitionCanvas = FindObjectOfType<Canvas>();
        if (transitionCanvas == null)
        {
            GameObject canvasGO = new GameObject("TransitionCanvas");
            transitionCanvas = canvasGO.AddComponent<Canvas>();
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 1000; // Ensure it's on top
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
        
        if (debug) Debug.Log("[SceneLoader] Created transition image UI element");
    }
    
    public void LoadSceneWithTransition(string sceneName)
    {
        if (isTransitioning)
        {
            if (debug) Debug.LogWarning("[SceneLoader] Already transitioning, ignoring request");
            return;
        }
        
        StartCoroutine(TransitionAndLoadScene(sceneName));
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
        
        // Wait a frame for the scene to load
        yield return null;
        
        // Phase 3: Slide out to right
        yield return StartCoroutine(SlideTransition(false));
        
        isTransitioning = false;
        
        if (debug) Debug.Log("[SceneLoader] Transition completed");
    }
    
    private IEnumerator SlideTransition(bool slideIn)
    {
        float elapsedTime = 0f;
        Vector2 startPosition;
        Vector2 endPosition;
        
        if (slideIn)
        {
            // Start off-screen left, slide to center
            startPosition = new Vector2(-Screen.width, 0);
            endPosition = Vector2.zero;
        }
        else
        {
            // Start at center, slide off-screen right
            startPosition = Vector2.zero;
            endPosition = new Vector2(Screen.width, 0);
        }
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            float curveValue = transitionCurve.Evaluate(progress);
            
            Vector2 currentPosition = Vector2.Lerp(startPosition, endPosition, curveValue);
            transitionRectTransform.anchoredPosition = currentPosition;
            
            yield return null;
        }
        
        // Ensure final position
        transitionRectTransform.anchoredPosition = endPosition;
    }
    
    // Public method to set transition image color
    public void SetTransitionColor(Color color)
    {
        if (transitionImage != null)
        {
            transitionImage.color = color;
        }
    }
    
    // Public method to set transition duration
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Max(0.1f, duration);
    }
    
    // Public method to check if currently transitioning
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}
