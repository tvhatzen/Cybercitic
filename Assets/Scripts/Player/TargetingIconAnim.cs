using UnityEngine;
using System.Collections;

public class TargetingIconAnim : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] targetingIconSprites;
    public float frameRate = 12f; // frames per second
    
    private SpriteRenderer spriteRenderer;
    private Coroutine animationCoroutine;
    private bool animating = false;

    void Awake()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
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

        // Don't restart if already animating
        if (animating)
        {
            return;
        }

        animating = true;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // Ensure sprite renderer is enabled
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        animationCoroutine = StartCoroutine(AnimationLoop());
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
        int currentFrame = 0;

        while (animating && targetingIconSprites != null && targetingIconSprites.Length > 0)
        {
            // Set the current sprite
            if (spriteRenderer != null && targetingIconSprites[currentFrame] != null)
            {
                spriteRenderer.sprite = targetingIconSprites[currentFrame];
            }

            // Wait for the next frame
            yield return new WaitForSeconds(frameTime);

            // Move to the next frame, loop back to 0 if at the end
            currentFrame = (currentFrame + 1) % targetingIconSprites.Length;
        }
    }
}

