using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Handles frame-based animations for modular character parts using pre-positioned sprites.
/// Simply swaps sprites based on upgrade levels while maintaining frame positions.
/// </summary>
public class FrameBasedPlayerAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimationFrame
    {
        public string frameName;
        public float duration = 0.1f;
        public List<BodyPartFrame> bodyPartFrames = new List<BodyPartFrame>();
    }

    [System.Serializable]
    public class BodyPartFrame
    {
        public UpgradeShopUI.BodyPart bodyPart;
        public SpriteRenderer spriteRenderer;
        public Sprite baseSprite; // Base sprite (level 0)
        public List<Sprite> upgradedSprites = new List<Sprite>(); // Upgraded sprites (level 1, 2, 3, etc.)
    }

    [System.Serializable]
    public class AnimationSequence
    {
        public string animationName;
        public List<AnimationFrame> frames = new List<AnimationFrame>();
        public bool loop = true;
    }

    [Header("Animation Sequences")]
    [SerializeField] private List<AnimationSequence> animationSequences = new List<AnimationSequence>();
    
    [Header("Current Animation")]
    [SerializeField] private string currentAnimation = "";
    [SerializeField] private bool isPlaying = false;
    [SerializeField] private int currentFrameIndex = 0;
    
    [Header("Animation Settings")]
    [SerializeField] private float attackDuration = 0.5f;
    
    [Header("Animation State Management")]
    [SerializeField] private bool isInCombat = false;
    [SerializeField] private bool isMoving = false;
    
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private Dictionary<string, AnimationSequence> animationLookup = new Dictionary<string, AnimationSequence>();
    private Dictionary<UpgradeShopUI.BodyPart, BodyPartFrame> bodyPartLookup = new Dictionary<UpgradeShopUI.BodyPart, BodyPartFrame>();
    private Coroutine currentAnimationCoroutine;

    private void Awake()
    {
        InitializeAnimationLookup();
        InitializeBodyPartLookup();
    }

    private void Start()
    {
        // Subscribe to upgrade events to update sprites when upgrades change
        if (UpgradeManager.Instance != null)
        {
            // Subscribe to centralized Event Bus
            GameEvents.OnUpgradePurchased += OnUpgradePurchased;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            // Unsubscribe from centralized Event Bus
            GameEvents.OnUpgradePurchased -= OnUpgradePurchased;
        }
    }

    private void InitializeAnimationLookup()
    {
        animationLookup.Clear();
        foreach (var sequence in animationSequences)
        {
            animationLookup[sequence.animationName] = sequence;
            if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Added animation: {sequence.animationName}");
        }
        
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Initialized {animationLookup.Count} animations");
    }

    private void InitializeBodyPartLookup()
    {
        bodyPartLookup.Clear();
        foreach (var sequence in animationSequences)
        {
            foreach (var frame in sequence.frames)
            {
                foreach (var bodyPartFrame in frame.bodyPartFrames)
                {
                    if (!bodyPartLookup.ContainsKey(bodyPartFrame.bodyPart))
                    {
                        bodyPartLookup[bodyPartFrame.bodyPart] = bodyPartFrame;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Play an animation sequence
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        if (!animationLookup.ContainsKey(animationName))
        {
            if (debug) Debug.LogWarning($"[FrameBasedPlayerAnimator] Animation '{animationName}' not found");
            return;
        }

        if (isPlaying)
        {
            if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Stopping current animation to play: {animationName}");
            StopCurrentAnimation();
        }

        currentAnimation = animationName;
        currentAnimationCoroutine = StartCoroutine(PlayAnimationSequence(animationLookup[animationName]));
        
        // Notify Event Bus of animation change
        GameEvents.AnimationChanged(animationName);
        
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Playing animation: {animationName}");
    }

    /// <summary>
    /// Play an animation sequence with forced non-looping behavior
    /// </summary>
    public void PlayAnimationOnce(string animationName)
    {
        if (!animationLookup.ContainsKey(animationName))
        {
            if (debug) Debug.LogWarning($"[FrameBasedPlayerAnimator] Animation '{animationName}' not found");
            return;
        }

        if (isPlaying)
        {
            StopCurrentAnimation();
        }

        currentAnimation = animationName;
        currentAnimationCoroutine = StartCoroutine(PlayAnimationSequenceOnce(animationLookup[animationName]));
        
        // Notify Event Bus of animation change
        GameEvents.AnimationChanged(animationName);
        
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Playing animation once: {animationName}");
    }

    /// <summary>
    /// Stop the current animation
    /// </summary>
    public void StopCurrentAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        isPlaying = false;
        currentFrameIndex = 0;
        
        if (debug) Debug.Log("[FrameBasedPlayerAnimator] Animation stopped");
    }

    /// <summary>
    /// Update all body part sprites based on current upgrade levels
    /// This method is called when upgrades change, but doesn't interfere with animation frame cycling
    /// </summary>
    public void UpdateBodyPartSprites()
    {
        if (UpgradeManager.Instance == null) return;

        // Only update sprites if we're not currently playing an animation
        // The animation system will handle sprite updates during playback
        if (!isPlaying)
        {
            // Set to the first frame of the current upgrade level
            SetIdleSprites();
        }
        
        if (debug) Debug.Log("[FrameBasedPlayerAnimator] Body part sprites updated");
    }

    /// <summary>
    /// Set all body parts to their default sprites based on current upgrade levels
    /// </summary>
    private void SetIdleSprites()
    {
        // Use the first frame of the first available animation as default
        foreach (var sequence in animationSequences)
        {
            if (sequence.frames.Count > 0)
            {
                var firstFrame = sequence.frames[0];
                foreach (var bodyPartFrame in firstFrame.bodyPartFrames)
                {
                    if (bodyPartFrame.spriteRenderer != null)
                    {
                        Sprite defaultSprite = GetSpriteForAnimationFrame(bodyPartFrame, 0);
                        if (defaultSprite != null)
                        {
                            bodyPartFrame.spriteRenderer.sprite = defaultSprite;
                        }
                    }
                }
                break;
            }
        }
    }

    private UpgradeShopUI.BodyPart GetBodyPartFromUpgrade(Upgrade upgrade)
    {
        string name = upgrade.UpgradeName.ToLower();

        if (name.Contains("core")) return UpgradeShopUI.BodyPart.Core;
        if (name.Contains("larm") || name.Contains("left arm")) return UpgradeShopUI.BodyPart.LeftArm;
        if (name.Contains("rarm") || name.Contains("right arm")) return UpgradeShopUI.BodyPart.RightArm;
        if (name.Contains("lleg") || name.Contains("left leg")) return UpgradeShopUI.BodyPart.LeftLeg;
        if (name.Contains("rleg") || name.Contains("right leg")) return UpgradeShopUI.BodyPart.RightLeg;

        return UpgradeShopUI.BodyPart.Core;
    }

    private IEnumerator PlayAnimationSequence(AnimationSequence sequence)
    {
        isPlaying = true;
        currentFrameIndex = 0;

        do
        {
            for (int i = 0; i < sequence.frames.Count; i++)
            {
                currentFrameIndex = i;
                var frame = sequence.frames[i];
                
                // Update all body part sprites for this animation frame
                foreach (var bodyPartFrame in frame.bodyPartFrames)
                {
                    if (bodyPartFrame.spriteRenderer != null)
                    {
                        // Get the appropriate sprite for this animation frame and upgrade level
                        Sprite targetSprite = GetSpriteForAnimationFrame(bodyPartFrame, i);
                        if (targetSprite != null)
                        {
                            bodyPartFrame.spriteRenderer.sprite = targetSprite;
                        }
                    }
                }
                
                yield return new WaitForSeconds(frame.duration);
            }
        } while (sequence.loop);

        isPlaying = false;
        currentAnimationCoroutine = null;
        
        // If this was an attack animation and we're in combat, return to standing
        if (sequence.animationName == "Attack")
        {
            // Check if we're still in combat and should return to standing
            var playerCombat = GetComponent<PlayerCombat>();
            if (playerCombat != null && playerCombat.InCombat)
            {
                PlayStandingAnimation();
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation completed, returning to standing");
            }
        }
        
        // Notify Event Bus of animation completion
        GameEvents.AnimationCompleted();
        
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Animation '{sequence.animationName}' completed");
    }

    private IEnumerator PlayAnimationSequenceOnce(AnimationSequence sequence)
    {
        isPlaying = true;
        currentFrameIndex = 0;

        // Play the animation once (ignore the loop setting)
        for (int i = 0; i < sequence.frames.Count; i++)
        {
            currentFrameIndex = i;
            var frame = sequence.frames[i];
            
            // Update all body part sprites for this animation frame
            foreach (var bodyPartFrame in frame.bodyPartFrames)
            {
                if (bodyPartFrame.spriteRenderer != null)
                {
                    // Get the appropriate sprite for this animation frame and upgrade level
                    Sprite targetSprite = GetSpriteForAnimationFrame(bodyPartFrame, i);
                    if (targetSprite != null)
                    {
                        bodyPartFrame.spriteRenderer.sprite = targetSprite;
                    }
                }
            }
            
            // Use custom duration for Attack animation only on the last frame, otherwise use frame duration
            float duration;
            if (sequence.animationName == "Attack" && i == sequence.frames.Count - 1)
            {
                // Only hold the last frame (attack frame) for the custom duration
                duration = attackDuration;
            }
            else
            {
                // Use normal frame duration for all other frames
                duration = frame.duration;
            }
            yield return new WaitForSeconds(duration);
        }

        isPlaying = false;
        currentAnimationCoroutine = null;
        
        // If this was an attack animation and we're in combat, return to standing
        if (sequence.animationName == "Attack")
        {
            if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation sequence completed");
            
            // Check if we're still in combat and should return to standing
            var playerCombat = GetComponent<PlayerCombat>();
            if (playerCombat != null && playerCombat.InCombat)
            {
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Still in combat, transitioning to standing");
                // Use a small delay to ensure the animation state is properly updated
                yield return new WaitForEndOfFrame();
                
                // Update currentAnimation to Standing before playing it
                currentAnimation = "Standing";
                PlayStandingAnimation();
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation completed, returning to standing");
            }
            else
            {
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Not in combat, not transitioning to standing");
            }
        }
        
        // Notify Event Bus of animation completion
        GameEvents.AnimationCompleted();
        
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Animation '{sequence.animationName}' completed once");
    }

    private Upgrade GetUpgradeForBodyPart(UpgradeShopUI.BodyPart bodyPart)
    {
        if (UpgradeManager.Instance == null) return null;

        var allUpgrades = UpgradeManager.Instance.GetAllUpgrades();
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null) continue;
            if (GetBodyPartFromUpgrade(upgrade) == bodyPart)
            {
                return upgrade;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the appropriate sprite for a specific animation frame and upgrade level
    /// </summary>
    private Sprite GetSpriteForAnimationFrame(BodyPartFrame bodyPartFrame, int animationFrameIndex)
    {
        // Get the current upgrade level for this body part
        var upgrade = GetUpgradeForBodyPart(bodyPartFrame.bodyPart);
        int upgradeLevel = upgrade != null ? upgrade.CurrentLevel : 0;

        // Get the sprite based on upgrade level
        if (upgradeLevel == 0)
        {
            // Use base sprite (level 0)
            return bodyPartFrame.baseSprite;
        }
        else if (upgradeLevel > 0 && upgradeLevel <= bodyPartFrame.upgradedSprites.Count)
        {
            // Use upgraded sprite for the current upgrade level
            return bodyPartFrame.upgradedSprites[upgradeLevel - 1];
        }
        else if (bodyPartFrame.upgradedSprites.Count > 0)
        {
            // Use the highest available upgrade sprite
            return bodyPartFrame.upgradedSprites[bodyPartFrame.upgradedSprites.Count - 1];
        }

        // Fallback to base sprite
        return bodyPartFrame.baseSprite;
    }

    private void OnUpgradePurchased(Upgrade upgrade)
    {
        UpdateBodyPartSprites();
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Upgrade purchased: {upgrade.UpgradeName}");
    }

    // Public methods for common animations
    public void PlayAttackAnimation() => PlayAnimationOnce("Attack");
    public void PlayWalkAnimation() => PlayAnimation("Walk");
    public void PlayRunAnimation() => PlayAnimation("Running");
    public void PlayStandingAnimation() 
    {
        if (debug) Debug.Log("[FrameBasedPlayerAnimator] PlayStandingAnimation called");
        PlayAnimation("Standing");
    }
    public void PlayDamageAnimation() => PlayAnimation("Damage");
    public void PlayDeathAnimation() => PlayAnimation("Death");

    /// <summary>
    /// Ensure the player is in the correct animation state for combat
    /// </summary>
    public void EnsureCombatAnimationState()
    {
        // If we're in combat but not playing any animation, play standing
        var playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat != null && playerCombat.InCombat && !isPlaying && currentAnimation != "Standing")
        {
            if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Ensuring standing animation in combat. Current: {currentAnimation}");
            PlayStandingAnimation();
        }
        else if (debug && playerCombat != null && playerCombat.InCombat)
        {
            if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Not ensuring standing - isPlaying: {isPlaying}, currentAnimation: {currentAnimation}");
        }
    }

    // Getters for debugging
    public string GetCurrentAnimation() => currentAnimation;
    public bool IsPlaying() => isPlaying;
    public int GetCurrentFrameIndex() => currentFrameIndex;
}
