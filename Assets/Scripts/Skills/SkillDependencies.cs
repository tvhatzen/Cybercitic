using UnityEngine;

namespace Game.Skills
{
    /// <summary>
    /// Concrete implementation of ISkillDependencies.
    /// </summary>
    public class SkillDependencies : ISkillDependencies
    {
        public Transform PlayerTransform { get; private set; }
        public MonoBehaviour PlayerHealthSystem { get; private set; }
        public IAudioManager AudioManager { get; private set; }

        public SkillDependencies(Transform playerTransform, MonoBehaviour playerHealthSystem, IAudioManager audioManager)
        {
            PlayerTransform = playerTransform;
            PlayerHealthSystem = playerHealthSystem;
            AudioManager = audioManager;
        }

        public static SkillDependencies FromSingletons()
        {
            Transform playerTransform = null;
            MonoBehaviour playerHealthSystem = null;
            IAudioManager audioManager = null;

            // Try multiple approaches to find PlayerInstance
            
            // Approach 1: Try reflection to get Instance property
            MonoBehaviour playerInstance = null;
            var playerInstanceType = System.Type.GetType("PlayerInstance");
            if (playerInstanceType == null)
            {
                playerInstanceType = System.Type.GetType("Game.Characters.PlayerInstance") ?? 
                                    System.Type.GetType("PlayerInstance, Assembly-CSharp");
            }
            
            if (playerInstanceType != null)
            {
                // Try different binding flag combinations
                var instanceProperty = playerInstanceType.GetProperty("Instance", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                
                if (instanceProperty == null)
                {
                    // Try without FlattenHierarchy
                    instanceProperty = playerInstanceType.GetProperty("Instance", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                }
                
                if (instanceProperty == null)
                {
                    // Try with all flags
                    instanceProperty = playerInstanceType.GetProperty("Instance", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                }
                
                if (instanceProperty != null)
                {
                    playerInstance = instanceProperty.GetValue(null) as MonoBehaviour;
                }
            }
            
            // Approach 2: If reflection failed, try FindObjectsByType and search for PlayerInstance
            if (playerInstance == null)
            {
                UnityEngine.Debug.Log("[SkillDependencies] Reflection failed, trying FindObjectsByType...");
                var allMonoBehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var mb in allMonoBehaviours)
                {
                    var typeName = mb.GetType().Name;
                    if (typeName == "PlayerInstance" || typeName.Contains("PlayerInstance"))
                    {
                        playerInstance = mb;
                        UnityEngine.Debug.Log($"[SkillDependencies] Found PlayerInstance via FindObjectsByType: {typeName}");
                        break;
                    }
                }
            }
            
            // Approach 3: Try finding by GameObject name
            if (playerInstance == null)
            {
                UnityEngine.Debug.Log("[SkillDependencies] Trying to find player by GameObject name...");
                var playerObj = GameObject.Find("Player");
                if (playerObj == null)
                {
                    playerObj = GameObject.Find("PlayerInstance");
                }
                if (playerObj != null)
                {
                    playerInstance = playerObj.GetComponent<MonoBehaviour>();
                }
            }
            
            if (playerInstance != null)
            {
                playerTransform = playerInstance.transform;
                UnityEngine.Debug.Log($"[SkillDependencies] Found PlayerInstance: {playerInstance.name} at {playerTransform.position}");
                
                // Search for component with SetShieldImmunity method (HealthSystem)
                var components = playerInstance.GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp.GetType().GetMethod("SetShieldImmunity") != null)
                    {
                        playerHealthSystem = comp;
                        UnityEngine.Debug.Log($"[SkillDependencies] Found HealthSystem: {comp.GetType().Name}");
                        break;
                    }
                }
                
                if (playerHealthSystem == null)
                {
                    UnityEngine.Debug.LogWarning("[SkillDependencies] HealthSystem not found on PlayerInstance");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[SkillDependencies] Could not find PlayerInstance using any method");
            }

            // Try to get AudioManager singleton 
            object audioManagerInstance = null;
            var audioManagerType = System.Type.GetType("AudioManager");
            if (audioManagerType == null)
            {
                audioManagerType = System.Type.GetType("AudioManager, Assembly-CSharp");
            }
            
            if (audioManagerType != null)
            {
                // Try different binding flag combinations
                var instanceProperty = audioManagerType.GetProperty("Instance",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                
                if (instanceProperty == null)
                {
                    instanceProperty = audioManagerType.GetProperty("Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                }
                
                if (instanceProperty == null)
                {
                    instanceProperty = audioManagerType.GetProperty("Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                }
                
                if (instanceProperty != null)
                {
                    audioManagerInstance = instanceProperty.GetValue(null);
                }
            }
            
            // Fallback: Try FindObjectsByType
            if (audioManagerInstance == null)
            {
                var allMonoBehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var mb in allMonoBehaviours)
                {
                    if (mb.GetType().Name == "AudioManager")
                    {
                        audioManagerInstance = mb;
                        UnityEngine.Debug.Log("[SkillDependencies] Found AudioManager via FindObjectsByType");
                        break;
                    }
                }
            }
            
            if (audioManagerInstance != null)
            {
                audioManager = new AudioManagerAdapter(audioManagerInstance);
                UnityEngine.Debug.Log("[SkillDependencies] Found AudioManager");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[SkillDependencies] AudioManager not found");
            }

            if (playerTransform == null)
            {
                UnityEngine.Debug.LogError("[SkillDependencies] WARNING: PlayerTransform is null! Skills will not work properly.");
            }

            return new SkillDependencies(playerTransform, playerHealthSystem, audioManager);
        }
    }

    /// <summary>
    /// Adapter to bridge existing AudioManager singleton to IAudioManager interface.
    /// </summary>
    internal class AudioManagerAdapter : IAudioManager
    {
        private readonly object audioManager;

        public AudioManagerAdapter(object audioManager)
        {
            this.audioManager = audioManager;
        }

        public void PlaySound(AudioClip clip)
        {
            if (audioManager != null && clip != null)
            {
                try
                {
                    // Specify parameter types to avoid ambiguous match
                    var playSoundMethod = audioManager.GetType().GetMethod("PlaySound",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null,
                        new System.Type[] { typeof(AudioClip) },
                        null);
                    
                    if (playSoundMethod != null)
                    {
                        playSoundMethod.Invoke(audioManager, new object[] { clip });
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[AudioManagerAdapter] PlaySound(AudioClip) method not found on {audioManager.GetType().Name}");
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"[AudioManagerAdapter] Error calling PlaySound: {e.Message}");
                }
            }
        }
    }
}

