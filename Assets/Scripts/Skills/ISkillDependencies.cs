using UnityEngine;

namespace Game.Skills
{
    /// <summary>
    /// Interface for skill dependencies to enable dependency injection.
    /// </summary>
    public interface ISkillDependencies
    {
        Transform PlayerTransform { get; }
        MonoBehaviour PlayerHealthSystem { get; }
        IAudioManager AudioManager { get; }
    }

    /// <summary>
    /// Interface for audio management to decouple from AudioManager singleton.
    /// </summary>
    public interface IAudioManager
    {
        void PlaySound(AudioClip clip);
    }
}

