using UnityEngine;

namespace Game.Skills
{
    /// <summary>
    /// Piercing Shot skill with custom rotation for particle effects.
    /// </summary>
    [CreateAssetMenu(fileName = "PiercingShotSkill", menuName = "Scriptable Objects/Skills/Piercing Shot Skill")]
    public class PiercingShotSkill : Skill, ISkillEffect
    {
        [Header("Piercing Shot Settings")]
        [SerializeField] private float rotationY = 90f;

        public void ApplyEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            // Default damage application is handled by SkillController
            if (Debug)
            {
                UnityEngine.Debug.Log($"[PiercingShotSkill] Applying piercing shot effects");
            }
        }

        public void CleanupEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
        }

        public Quaternion GetEffectRotation(Vector3 playerForward, Quaternion defaultRotation)
        {
            // Apply 90 degrees rotation on Y axis for piercing shot
            if (Debug)
            {
                UnityEngine.Debug.Log($"[PiercingShotSkill] Using {rotationY}° Y rotation for particle effect");
            }
            return Quaternion.Euler(0, rotationY, 0);
        }
    }
}

