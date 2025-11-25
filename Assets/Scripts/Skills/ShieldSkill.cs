using UnityEngine;

namespace Game.Skills
{
    /// <summary>
    /// Shield skill that provides temporary immunity to the player.
    /// Uses polymorphism instead of string-based checks.
    /// </summary>
    [CreateAssetMenu(fileName = "ShieldSkill", menuName = "Scriptable Objects/Skills/Shield Skill")]
    public class ShieldSkill : Skill, ISkillEffect
    {
        public override SkillInstance CreateInstance(bool isUnlocked = false)
        {
            return new SkillInstance(this, isUnlocked);
        }

        public void ApplyEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            if (Debug)
            {
                UnityEngine.Debug.Log($"[ShieldSkill] Applying shield immunity to player");
            }

            if (dependencies?.PlayerHealthSystem != null)
            {
                var setShieldMethod = dependencies.PlayerHealthSystem.GetType().GetMethod("SetShieldImmunity",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (setShieldMethod != null)
                {
                    setShieldMethod.Invoke(dependencies.PlayerHealthSystem, new object[] { true });
                    
                    if (Debug)
                    {
                        var isImmuneProperty = dependencies.PlayerHealthSystem.GetType().GetProperty("IsShieldImmune",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (isImmuneProperty != null)
                        {
                            bool isImmune = (bool)isImmuneProperty.GetValue(dependencies.PlayerHealthSystem);
                            if (isImmune)
                            {
                                UnityEngine.Debug.Log("[ShieldSkill] Shield immunity confirmed active");
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("[ShieldSkill] ERROR - Shield immunity not active after SetShieldImmunity(true)!");
                            }
                        }
                    }
                }
            }
            else
            {
                if (Debug)
                {
                    UnityEngine.Debug.LogError("[ShieldSkill] Player HealthSystem not found in dependencies!");
                }
            }
        }

        public void CleanupEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            if (Debug)
            {
                UnityEngine.Debug.Log($"[ShieldSkill] Removing shield immunity from player");
            }

            if (dependencies?.PlayerHealthSystem != null)
            {
                var setShieldMethod = dependencies.PlayerHealthSystem.GetType().GetMethod("SetShieldImmunity",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (setShieldMethod != null)
                {
                    setShieldMethod.Invoke(dependencies.PlayerHealthSystem, new object[] { false });
                }
            }
        }

        public Quaternion GetEffectRotation(Vector3 playerForward, Quaternion defaultRotation)
        {
            // Shield effects typically don't need special rotation
            return defaultRotation;
        }
    }
}

