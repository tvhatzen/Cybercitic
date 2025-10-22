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
            UpgradeManager.Instance.OnUpgradePurchased += OnUpgradePurchased;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradePurchased -= OnUpgradePurchased;
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
            StopCurrentAnimation();
        }

        currentAnimation = animationName;
        currentAnimationCoroutine = StartCoroutine(PlayAnimationSequence(animationLookup[animationName]));
        
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Playing animation: {animationName}");
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
        
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Animation '{sequence.animationName}' completed");
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
    public void PlayAttackAnimation() => PlayAnimation("Attack");
    public void PlayWalkAnimation() => PlayAnimation("Walk");
    public void PlayRunAnimation() => PlayAnimation("Running");
    public void PlayDamageAnimation() => PlayAnimation("Damage");
    public void PlayDeathAnimation() => PlayAnimation("Death");

    // Getters for debugging
    public string GetCurrentAnimation() => currentAnimation;
    public bool IsPlaying() => isPlaying;
    public int GetCurrentFrameIndex() => currentFrameIndex;
}
