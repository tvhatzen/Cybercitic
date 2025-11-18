using UnityEngine;
using System.Collections;

public class TargetingIconAnim : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] targetingIconSprites;
    public float frameRate = 12f; // frames per second
    public bool debug = false;
    
    private SpriteRenderer spriteRenderer;
    private Coroutine animationCoroutine;
    private bool animating = false;

    void Awake()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configure sprite renderer for world space rendering
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = 100; // High sorting order to appear above other sprites
            spriteRenderer.sortingLayerName = "Default";
        }
    }

    // Start the targeting icon animation loop
    public void StartAnimation()
    {
        // Check if sprites are assigned
        if (targetingIconSprites == null || targetingIconSprites.Length == 0)
        {
            Debug.LogWarning($"[TargetingIconAnim] No sprites assigned to {gameObject.name}! Please assign sprites in the Inspector.");
            // Still enable the sprite renderer in case a single sprite was set directly
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            return;
        }

        // Stop any existing animation first
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        animating = true;
        
        // Ensure sprite renderer is enabled and set the first sprite immediately
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            // Set the first sprite immediately so it's visible right away
            if (targetingIconSprites[0] != null)
            {
                spriteRenderer.sprite = targetingIconSprites[0];
            }
        }
        
        // Only start animation loop if we have more than one sprite
        if (targetingIconSprites.Length > 1)
        {
            animationCoroutine = StartCoroutine(AnimationLoop());
        }
    }

    // Stop the targeting icon animation
    public void StopAnimation()
    {
        animating = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        // Hide the sprite when animation stops
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
        }
    }

    // Coroutine that loops through the sprites
    private IEnumerator AnimationLoop()
    {
        float frameTime = 1f / frameRate;
        int currentFrame = 1; // Start at frame 1 since frame 0 is already displayed

        // If only one sprite, exit early
        if (targetingIconSprites.Length <= 1)
        {
            if (debug) Debug.Log("[TargetingIconAnim] Only one sprite, no animation needed");
            yield break;
        }

        if (debug) Debug.Log($"[TargetingIconAnim] Starting animation loop with {targetingIconSprites.Length} sprites at {frameRate} fps");

        while (animating && targetingIconSprites != null && targetingIconSprites.Length > 1)
        {
            // Wait for the frame duration
            yield return new WaitForSeconds(frameTime);

            // Check if we're still animating (might have been stopped)
            if (!animating)
            {
                if (debug) Debug.Log("[TargetingIconAnim] Animation stopped, exiting loop");
                break;
            }

            // Ensure GameObject is still active (coroutines can't run on inactive objects)
            if (!gameObject.activeInHierarchy)
            {
                if (debug) Debug.LogWarning("[TargetingIconAnim] GameObject is inactive, pausing animation");
                animating = false;
                break;
            }

            // Set the current sprite
            if (spriteRenderer != null && currentFrame < targetingIconSprites.Length && targetingIconSprites[currentFrame] != null)
            {
                spriteRenderer.sprite = targetingIconSprites[currentFrame];
                if (debug) Debug.Log($"[TargetingIconAnim] Set sprite to frame {currentFrame}");
            }
            else
            {
                if (debug) Debug.LogWarning($"[TargetingIconAnim] Failed to set sprite at frame {currentFrame}");
            }

            // Move to the next frame, loop back to 0 if at the end
            currentFrame = (currentFrame + 1) % targetingIconSprites.Length;
        }

        if (debug) Debug.Log("[TargetingIconAnim] Animation loop ended");
    }
}

