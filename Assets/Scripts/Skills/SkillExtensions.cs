using UnityEngine;

namespace Game.Skills
{
    /// <summary>
    /// Extension methods for Skill to provide backward compatibility with code
    /// that expects Skill to have runtime properties.
    /// These methods delegate to SkillManager/SkillInstance.
    /// </summary>
    public static class SkillExtensions
    {
        /// <summary>
        /// Gets the SkillInstance for this skill from the first available SkillManager.
        /// This is a compatibility method - prefer using SkillManager.GetSkillInstance directly.
        /// </summary>
        private static SkillInstance GetInstance(Skill skill)
        {
            if (skill == null) return null;
            
            // Try to find SkillManager in the scene
            SkillManager manager = Object.FindFirstObjectByType<SkillManager>();
            return manager?.GetSkillInstance(skill);
        }

        /// <summary>
        /// Compatibility property - gets current state from SkillInstance.
        /// </summary>
        public static SkillInstance.SkillStates GetCurrentState(this Skill skill)
        {
            var instance = GetInstance(skill);
            return instance?.CurrentState ?? SkillInstance.SkillStates.Locked;
        }

        /// <summary>
        /// Compatibility property - gets cooldown from SkillInstance.
        /// </summary>
        public static float GetCurrentCooldown(this Skill skill)
        {
            var instance = GetInstance(skill);
            return instance?.CurrentCooldown ?? 0f;
        }

        /// <summary>
        /// Compatibility property - gets cooldown progress from SkillInstance.
        /// </summary>
        public static float GetCooldownProgress(this Skill skill)
        {
            var instance = GetInstance(skill);
            return instance?.CooldownProgress ?? 0f;
        }

        /// <summary>
        /// Compatibility property - gets skill duration from SkillInstance.
        /// </summary>
        public static float GetCurrentSkillDuration(this Skill skill)
        {
            var instance = GetInstance(skill);
            return instance?.CurrentSkillDuration ?? 0f;
        }

        /// <summary>
        /// Compatibility property - gets skill duration progress from SkillInstance.
        /// </summary>
        public static float GetSkillDurationProgress(this Skill skill)
        {
            var instance = GetInstance(skill);
            return instance?.SkillDurationProgress ?? 0f;
        }

        /// <summary>
        /// Compatibility property - checks if skill is ready from SkillInstance.
        /// </summary>
        public static bool GetIsReady(this Skill skill)
        {
            var instance = GetInstance(skill);
            return instance?.IsReady ?? false;
        }

        /// <summary>
        /// Compatibility property - checks if skill is unlocked from SkillInstance.
        /// </summary>
        public static bool GetIsUnlocked(this Skill skill)
        {
            var instance = GetInstance(skill);
            return instance?.IsUnlocked ?? false;
        }

        /// <summary>
        /// Compatibility property - gets icon from SkillInstance.
        /// </summary>
        public static Sprite GetIcon(this Skill skill)
        {
            var instance = GetInstance(skill);
            return instance?.Icon ?? skill?.SkillIcon;
        }
    }
}

