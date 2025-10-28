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
        public Sprite baseSprite; // base sprite (level 0)
        public List<Sprite> upgradedSprites = new List<Sprite>(); // upgraded sprites (level 1, 2, 3)
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
        // subscribe to upgrade events to update sprites when upgrades change
        if (UpgradeManager.Instance != null)
        {
            GameEvents.OnUpgradePurchased += OnUpgradePurchased;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
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
        
        // notify Event Bus of animation change
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
        
        // notify Event Bus of animation change
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
    /// </summary>
    public void UpdateBodyPartSprites()
    {
        if (UpgradeManager.Instance == null) return;

        // only update sprites if not currently playing an animation
        if (!isPlaying)
        {
            SetIdleSprites();
        }
        
        if (debug) Debug.Log("[FrameBasedPlayerAnimator] Body part sprites updated");
    }

    /// <summary>
    /// Set all body parts to their default sprites based on current upgrade levels
    /// </summary>
    private void SetIdleSprites()
    {
        // use the first frame of the first available animation as default
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
                
                // update all body part sprites for this animation frame
                foreach (var bodyPartFrame in frame.bodyPartFrames)
                {
                    if (bodyPartFrame.spriteRenderer != null)
                    {
                        // get the appropriate sprite for this animation frame and upgrade level
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
        
        // if this was an attack animation, return to appropriate animation based on combat state
        if (sequence.animationName == "Attack")
        {
            var playerCombat = GetComponent<PlayerCombat>();
            var playerMovement = GetComponent<PlayerMovement>();
            
            if (playerCombat != null && playerCombat.InCombat)
            {
                // still in combat, return to standing
                PlayStandingAnimation();
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation completed, returning to standing");
            }
            else if (playerMovement != null && playerMovement.CanMove)
            {
                // not in combat and can move, return to running
                PlayRunAnimation();
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation completed, returning to running");
            }
            else
            {
                // fallback to standing
                PlayStandingAnimation();
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation completed, returning to standing (fallback)");
            }
        }
        
        GameEvents.AnimationCompleted();
        
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Animation '{sequence.animationName}' completed");
    }

    private IEnumerator PlayAnimationSequenceOnce(AnimationSequence sequence)
    {
        isPlaying = true;
        currentFrameIndex = 0;

        // play the animation once (ignore the loop setting)
        for (int i = 0; i < sequence.frames.Count; i++)
        {
            currentFrameIndex = i;
            var frame = sequence.frames[i];
            
            // update all body part sprites for this animation frame
            foreach (var bodyPartFrame in frame.bodyPartFrames)
            {
                if (bodyPartFrame.spriteRenderer != null)
                {
                    // get the appropriate sprite for this animation frame and upgrade level
                    Sprite targetSprite = GetSpriteForAnimationFrame(bodyPartFrame, i);
                    if (targetSprite != null)
                    {
                        bodyPartFrame.spriteRenderer.sprite = targetSprite;
                    }
                }
            }
            
            // use custom duration for Attack animation only on the last frame, otherwise use frame duration
            float duration;
            if (sequence.animationName == "Attack" && i == sequence.frames.Count - 1)
            {
                duration = attackDuration;
            }
            else
            {
                duration = frame.duration;
            }
            yield return new WaitForSeconds(duration);
        }

        isPlaying = false;
        currentAnimationCoroutine = null;
        
        // if this was an attack animation, return to appropriate animation based on combat state
        if (sequence.animationName == "Attack")
        {
            if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation sequence completed");
            
            var playerCombat = GetComponent<PlayerCombat>();
            var playerMovement = GetComponent<PlayerMovement>();
            
            if (playerCombat != null && playerCombat.InCombat)
            {
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Still in combat, transitioning to standing");
                yield return new WaitForEndOfFrame();
                
                // update currentAnimation to Standing before playing it
                currentAnimation = "Standing";
                PlayStandingAnimation();
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation completed, returning to standing");
            }
            else if (playerMovement != null && playerMovement.CanMove)
            {
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Not in combat and can move, transitioning to running");
                yield return new WaitForEndOfFrame();
                
                // update currentAnimation to Running before playing it
                currentAnimation = "Running";
                PlayRunAnimation();
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Attack animation completed, returning to running");
            }
            else
            {
                if (debug) Debug.Log("[FrameBasedPlayerAnimator] Fallback to standing animation");
                yield return new WaitForEndOfFrame();
                
                currentAnimation = "Standing";
                PlayStandingAnimation();
            }
        }
        
        // notify Event Bus of animation completion
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
        // get the current upgrade level for this body part
        var upgrade = GetUpgradeForBodyPart(bodyPartFrame.bodyPart);
        int upgradeLevel = upgrade != null ? upgrade.CurrentLevel : 0;

        // get the sprite based on upgrade level
        if (upgradeLevel == 0)
        {
            return bodyPartFrame.baseSprite;
        }
        else if (upgradeLevel > 0 && upgradeLevel <= bodyPartFrame.upgradedSprites.Count)
        {
            return bodyPartFrame.upgradedSprites[upgradeLevel - 1];
        }
        else if (bodyPartFrame.upgradedSprites.Count > 0)
        {
            return bodyPartFrame.upgradedSprites[bodyPartFrame.upgradedSprites.Count - 1];
        }

        // fallback to base sprite
        return bodyPartFrame.baseSprite;
    }

    private void OnUpgradePurchased(Upgrade upgrade)
    {
        UpdateBodyPartSprites();
        if (debug) Debug.Log($"[FrameBasedPlayerAnimator] Upgrade purchased: {upgrade.UpgradeName}");
    }

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

    // ensure the player is in the correct animation state for combat
    public void EnsureCombatAnimationState()
    {
        // if player is in combat but not playing any animation, play standing
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

    // debugging
    public string GetCurrentAnimation() => currentAnimation;
    public bool IsPlaying() => isPlaying;
    public int GetCurrentFrameIndex() => currentFrameIndex;
}
// *** When upgrading core/rightarm/left leg but not right leg,
// right leg in attack animation is showing two legs and its the first upgrade color 