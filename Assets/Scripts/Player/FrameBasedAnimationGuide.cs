using UnityEngine;

/// <summary>
/// Guide for setting up frame-based animations with upgrade support.
/// This explains how to configure the new system properly.
/// </summary>
public class FrameBasedAnimationGuide : MonoBehaviour
{
    [Header("Setup Guide")]
    [TextArea(15, 25)]
    [SerializeField] private string setupGuide = @"
FRAME-BASED ANIMATION SETUP GUIDE
==================================

PROBLEM SOLVED:
===============
The system was cycling through upgrade levels instead of animation frames.
Now it properly cycles through animation frames while respecting upgrade levels.

HOW TO SET UP:
==============

1. ANIMATION SEQUENCES:
   - Create animation sequences (Run, Attack, Idle, etc.)
   - Each sequence has multiple frames
   - Each frame has a duration (e.g., 0.1f for smooth animation)

2. BODY PART FRAMES:
   For each body part in each animation frame, configure:

   a) SPRITE RENDERER:
      - Assign the SpriteRenderer for this body part
      - This is where the sprite will be displayed

   b) BASE SPRITE (level 0):
      - Single sprite for base upgrade level (Core_1_0, Larm_1_0, etc.)
      - Used when no upgrades are purchased

   c) UPGRADED SPRITES (level 1+):
      - List of sprites for each upgrade level
      - Element 0: Core_1_1, Larm_1_1, etc. (level 1)
      - Element 1: Core_1_2, Larm_1_2, etc. (level 2)
      - Element 2: Core_1_3, Larm_1_3, etc. (level 3)
      - Used when upgrades are purchased

EXAMPLE CONFIGURATION:
======================

RUN ANIMATION (3 frames):
- Frame 1: Core_1_0_frame1, Larm_1_0_frame1, Rarm_1_0_frame1, etc.
- Frame 2: Core_1_0_frame2, Larm_1_0_frame2, Rarm_1_0_frame2, etc.
- Frame 3: Core_1_0_frame3, Larm_1_0_frame3, Rarm_1_0_frame3, etc.

UPGRADED RUN ANIMATION (Core level 1):
- Frame 1: Core_1_1_frame1, Larm_1_0_frame1, Rarm_1_0_frame1, etc.
- Frame 2: Core_1_1_frame2, Larm_1_0_frame2, Rarm_1_0_frame2, etc.
- Frame 3: Core_1_1_frame3, Larm_1_0_frame3, Rarm_1_0_frame3, etc.

INSPECTOR SETUP:
===============

For each Body Part Frame, you'll now see:
├── Body Part: Core, LeftArm, RightArm, LeftLeg, RightLeg
├── Sprite Renderer: [Assign your SpriteRenderer]
├── Base Sprite: [Core_1_0_frame1] (single sprite)
└── Upgraded Sprites: [Expandable list]
    ├── Element 0: Core_1_1_frame1 (level 1)
    ├── Element 1: Core_1_2_frame1 (level 2)
    └── Element 2: Core_1_3_frame1 (level 3)

KEY POINTS:
===========

✅ Animation frames cycle through (0, 1, 2, 3, ...)
✅ Upgrade levels determine which sprite set to use
✅ Each upgrade level has its own complete set of animation frames
✅ System automatically picks the right sprite for current frame + upgrade level

SETUP STEPS:
============

1. Create Animation Sequences in inspector
2. For each sequence, add frames
3. For each frame, configure all body parts
4. For each body part, set:
   - Animation Frames (fallback)
   - Base Upgrade Sprites (level 0)
   - Upgraded Sprites (level 1+)
5. Test the animations!

This system now properly cycles through animation frames while respecting upgrade levels!
";

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private void Start()
    {
        if (debug)
        {
            Debug.Log("[FrameBasedAnimationGuide] Setup guide loaded. Check inspector for detailed instructions.");
        }
    }

    /// <summary>
    /// Display the setup guide in the console
    /// </summary>
    [ContextMenu("Show Setup Guide")]
    public void ShowSetupGuide()
    {
        Debug.Log(setupGuide);
    }
}
