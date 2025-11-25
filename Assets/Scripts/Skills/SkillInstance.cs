using System;
using UnityEngine;
using Game.Skills;

namespace Game.Skills
{
    /// <summary>
    /// Runtime state machine for a skill instance.
    /// Tracks cooldown, charges, state, and usage per character.
    /// </summary>
    public class SkillInstance
    {
        private readonly Skill skillData;
        private SkillStates currentState;
        private float currentCooldown;
        private float currentSkillDuration;
        private int currentCharges;
        private bool isUnlocked;
        private bool hasBeenUsed;

        // Events
        public event Action<SkillInstance> OnSkillActivated;
        public event Action<SkillInstance> OnSkillCooldownStarted;
        public event Action<SkillInstance> OnSkillCooldownFinished;

        public enum SkillStates 
        {
            ReadyToUse,
            Casting,
            Cooldown,
            Locked
        }

        // Properties
        public Skill SkillData => skillData;
        public SkillStates CurrentState => currentState;
        public float CurrentCooldown => currentCooldown;
        public float CooldownProgress => skillData.CooldownDuration > 0 ? currentCooldown / skillData.CooldownDuration : 0f;
        public float CurrentSkillDuration => currentSkillDuration;
        public float SkillDurationProgress => skillData.SkillDuration > 0 ? currentSkillDuration / skillData.SkillDuration : 0f;
        public int CurrentCharges => currentCharges;
        public bool IsOnCooldown => currentState == SkillStates.Cooldown;
        public bool IsReady => currentState == SkillStates.ReadyToUse && isUnlocked;
        public bool IsUnlocked => isUnlocked;
        public bool HasBeenUsed => hasBeenUsed;
        public Sprite Icon => currentState == SkillStates.Casting ? skillData.CastIcon : skillData.SkillIcon;

        public SkillInstance(Skill skillData, bool isUnlocked = false)
        {
            this.skillData = skillData ?? throw new ArgumentNullException(nameof(skillData));
            this.isUnlocked = isUnlocked;
            Initialize();
        }

        public void Initialize()
        {
            currentState = isUnlocked ? SkillStates.ReadyToUse : SkillStates.Locked;
            currentCooldown = 0f;
            currentSkillDuration = 0f;
            currentCharges = skillData.MaxCharges;
            hasBeenUsed = false;
        }

        public bool CanActivate()
        {
            return isUnlocked && currentState == SkillStates.ReadyToUse && currentCharges > 0 && !hasBeenUsed;
        }

        public void SetState(SkillStates newState)
        {
            currentState = newState;
        }

        public void SetCasting()
        {
            currentState = SkillStates.Casting;
            OnSkillActivated?.Invoke(this);
        }

        public void SetCooldown(float duration)
        {
            currentState = SkillStates.Cooldown;
            currentCooldown = duration;
            OnSkillCooldownStarted?.Invoke(this);
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (currentState == SkillStates.Cooldown && currentCooldown > 0)
            {
                currentCooldown -= deltaTime;
                if (currentCooldown <= 0)
                {
                    FinishCooldown();
                }
            }
        }

        public void UpdateSkillDuration(float deltaTime)
        {
            if (currentSkillDuration > 0)
            {
                currentSkillDuration -= deltaTime;
                if (currentSkillDuration < 0)
                {
                    currentSkillDuration = 0f;
                }
            }
        }

        public void SetSkillDuration(float duration)
        {
            currentSkillDuration = duration;
        }

        public void ConsumeCharge()
        {
            if (currentCharges > 0)
            {
                currentCharges--;
            }
        }

        private void FinishCooldown()
        {
            currentCooldown = 0f;
            if (!hasBeenUsed)
            {
                currentState = SkillStates.ReadyToUse;
                currentCharges = skillData.MaxCharges;
            }
            OnSkillCooldownFinished?.Invoke(this);
        }

        public void Unlock()
        {
            isUnlocked = true;
            currentState = SkillStates.ReadyToUse;
        }

        public void Lock()
        {
            isUnlocked = false;
            currentState = SkillStates.Locked;
            currentCooldown = 0f;
            currentCharges = 0;
        }

        public void ResetCooldown()
        {
            if (isUnlocked)
            {
                currentState = SkillStates.ReadyToUse;
                currentCooldown = 0f;
                currentSkillDuration = 0f;
                currentCharges = skillData.MaxCharges;
                hasBeenUsed = false;
            }
        }

        public void MarkAsUsed()
        {
            hasBeenUsed = true;
        }

        public void ResetUsage()
        {
            hasBeenUsed = false;
        }
    }
}
