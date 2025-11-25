using UnityEngine;
using Game.Skills;

namespace Game.Skills
{
    /// <summary>
    /// Data-only ScriptableObject containing skill configuration.
    /// No runtime state is stored here - that logic was moved to SkillInstance
    /// </summary>
    [CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
    public class Skill : ScriptableObject
    {
        [Header("Skill Info")]
        [SerializeField] private string skillName;
        [SerializeField] private string skillDescription;
        [SerializeField] private Sprite skillIcon;
        [SerializeField] private Sprite castIcon;
        
        [Header("Cooldown Settings")]
        [SerializeField] private float cooldownDuration = 5f;
        [SerializeField] private float castingTime = 1f;
        [SerializeField] private float skillDuration = 2f;
        
        [Header("Skill Effects")]
        [SerializeField] private int skillDamage = 50;
        [SerializeField] private float skillRange = 5f;
        [SerializeField] private int maxCharges = 1;
        
        [Header("Audio/Visual")]
        [SerializeField] protected AudioClip skillSound;
        [SerializeField] protected ParticleSystem skillEffect;
        
        [Header("DEBUG")]
        [SerializeField] private bool debug = false;

        // Properties - read-only access to data
        public string SkillName => skillName;
        public string Description => skillDescription;
        public Sprite SkillIcon => skillIcon;
        public Sprite CastIcon => castIcon;
        public float CooldownDuration => cooldownDuration;
        public float CastingTime => castingTime;
        public float SkillDuration => skillDuration;
        public int SkillDamage => skillDamage;
        public float SkillRange => skillRange;
        public int MaxCharges => maxCharges;
        public AudioClip SkillSound => skillSound;
        public ParticleSystem SkillEffect => skillEffect;
        public bool Debug => debug;

        public virtual SkillInstance CreateInstance(bool isUnlocked = false)
        {
            return new SkillInstance(this, isUnlocked);
        }

        protected virtual SkillInstance CreateSkillInstance(bool isUnlocked)
        {
            return new SkillInstance(this, isUnlocked);
        }
    }
}
