using System.Collections.Generic;
using UnityEngine;
using Game.Skills;

namespace Game.Skills
{
    /// <summary>
    /// Manages skill instances and controllers for a character.
    /// Handles skill activation, updates, and lifecycle.
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        [SerializeField] private List<Skill> availableSkills = new List<Skill>();
        
        private Dictionary<Skill, SkillInstance> skillInstances = new Dictionary<Skill, SkillInstance>();
        private Dictionary<SkillInstance, SkillController> skillControllers = new Dictionary<SkillInstance, SkillController>();
        private ISkillDependencies dependencies;

        private void Awake()
        {
            // Initialize dependencies - can be injected or created from singletons
            dependencies = SkillDependencies.FromSingletons();
            
            // Initialize all skill instances
            foreach (var skill in availableSkills)
            {
                if (skill != null)
                {
                    var instance = skill.CreateInstance(false);
                    skillInstances[skill] = instance;
                    
                    // Create controller for this skill
                    var controller = gameObject.AddComponent<SkillController>();
                    controller.Initialize(instance, dependencies);
                    skillControllers[instance] = controller;
                }
            }
        }

        private void Start()
        {
            // Refresh dependencies in Start() in case PlayerInstance wasn't ready in Awake()
            RefreshDependencies();
        }

        /// <summary>
        /// Refreshes dependencies if they're null or player transform is missing.
        /// </summary>
        private void RefreshDependencies()
        {
            if (dependencies == null || dependencies.PlayerTransform == null)
            {
                UnityEngine.Debug.Log("[SkillManager] Refreshing dependencies...");
                dependencies = SkillDependencies.FromSingletons();
                
                // Update all controllers with new dependencies
                foreach (var controller in skillControllers.Values)
                {
                    controller.SetDependencies(dependencies);
                }
            }
        }

        private void Update()
        {
            // Update skill durations only (cooldowns are handled by SkillController coroutines)
            foreach (var instance in skillInstances.Values)
            {
                instance.UpdateSkillDuration(Time.deltaTime);
            }
        }

        public bool ActivateSkill(Skill skill)
        {
            if (skill == null || !skillInstances.ContainsKey(skill))
            {
                return false;
            }

            var instance = skillInstances[skill];
            if (!skillControllers.ContainsKey(instance))
            {
                return false;
            }

            return skillControllers[instance].ActivateSkill();
        }

        public SkillInstance GetSkillInstance(Skill skill)
        {
            return skillInstances.TryGetValue(skill, out var instance) ? instance : null;
        }

        public void UnlockSkill(Skill skill)
        {
            EnsureSkillInstance(skill);
            if (skillInstances.TryGetValue(skill, out var instance))
            {
                instance.Unlock();
            }
        }

        public void LockSkill(Skill skill)
        {
            if (skillInstances.TryGetValue(skill, out var instance))
            {
                instance.Lock();
            }
        }

        /// <summary>
        /// Ensures a skill instance exists for the given skill.
        /// Creates it if it doesn't exist.
        /// </summary>
        private void EnsureSkillInstance(Skill skill)
        {
            if (skill == null) return;
            
            if (!skillInstances.ContainsKey(skill))
            {
                var instance = skill.CreateInstance(false);
                skillInstances[skill] = instance;
                
                // Create controller for this skill
                var controller = gameObject.AddComponent<SkillController>();
                controller.Initialize(instance, dependencies);
                skillControllers[instance] = controller;
            }
        }

        public void ResetAllCooldowns()
        {
            foreach (var instance in skillInstances.Values)
            {
                instance.ResetCooldown();
            }
        }

        public void SetDependencies(ISkillDependencies deps)
        {
            dependencies = deps;
            // Update all controllers with new dependencies
            foreach (var controller in skillControllers.Values)
            {
                // Note: This requires adding a SetDependencies method to SkillController
            }
        }
    }
}

