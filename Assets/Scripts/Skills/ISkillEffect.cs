namespace Game.Skills
{
    /// <summary>
    /// Interface for skills that have custom effect application logic.
    /// </summary>
    public interface ISkillEffect
    {
        void ApplyEffects(SkillInstance instance, ISkillDependencies dependencies);
        void CleanupEffects(SkillInstance instance, ISkillDependencies dependencies);
        UnityEngine.Quaternion GetEffectRotation(UnityEngine.Vector3 playerForward, UnityEngine.Quaternion defaultRotation);
    }
}

