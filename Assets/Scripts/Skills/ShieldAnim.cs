using UnityEngine;
using System.Collections;

public class ShieldAnim : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] shieldAnim;
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
    }

    // Start the shield animation loop
    public void AnimateShield()
    {
        if (animating || shieldAnim == null || shieldAnim.Length == 0)
        {
            return;
        }

        animating = true;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationLoop());
    }

    // Stop the shield animation
    public void StopShieldAnimation()
    {
        animating = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        // Optionally hide the sprite when animation stops
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
        }
    }

    // Coroutine that loops through the sprites
    private IEnumerator AnimationLoop()
    {
        float frameTime = 1f / frameRate;
        int currentFrame = 0;

        while (animating && shieldAnim != null && shieldAnim.Length > 0)
        {
            // Set the current sprite
            if (spriteRenderer != null && shieldAnim[currentFrame] != null)
            {
                spriteRenderer.sprite = shieldAnim[currentFrame];
            }

            // Wait for the next frame
            yield return new WaitForSeconds(frameTime);

            // Move to the next frame, loop back to 0 if at the end
            currentFrame = (currentFrame + 1) % shieldAnim.Length;
        }
    }
}
