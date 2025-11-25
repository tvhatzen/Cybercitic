using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Skills;

namespace Game.Skills
{
    [CreateAssetMenu(fileName = "Shield", menuName = "Scriptable Objects/Skills/Shield")]
    public class Shield : Skill, ISkillEffect
    {
        [Header("Shield Specific")]
        [SerializeField] private float damageReduction = 0.5f; // 50% damage reduction
        [SerializeField] private float shieldDuration = 8f; // How long the shield lasts
        public GameObject shieldPrefab; // Shield GameObject with ShieldAnim component
        [SerializeField] private Vector3 shieldScale = Vector3.one; // Scale of the shield sprite 

        // Static dictionary to track shield state per instance 
        private static Dictionary<SkillInstance, ShieldState> shieldStates = new Dictionary<SkillInstance, ShieldState>();

        private class ShieldState
        {
            public bool isShieldActive = false;
            public float originalDefense = 0f;
            public bool hasOriginalDefenseBeenSaved = false;
            public MonoBehaviour playerEntityData;
            public MonoBehaviour playerHealthSystem;
            public ShieldAnim currentShieldAnim;
            public Coroutine durationCoroutine;
        }

        public void ApplyEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            UnityEngine.Debug.LogError("[Shield] ===== SHIELD ApplyEffects() CALLED =====");
            
            try
            {
                UnityEngine.Debug.Log($"[Shield] ApplyEffects called! Providing {damageReduction * 100}% damage reduction for {shieldDuration} seconds!");
                
                // Get or create shield state for this instance
                if (!shieldStates.ContainsKey(instance))
                {
                    shieldStates[instance] = new ShieldState();
                }
                var state = shieldStates[instance];
                
                // Apply shield effect
                ApplyShield(instance, dependencies, state);
                
                // Verify shield was applied
                if (state.playerHealthSystem != null)
                {
                    var isImmuneProperty = state.playerHealthSystem.GetType().GetProperty("IsShieldImmune",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (isImmuneProperty != null)
                    {
                        bool isImmune = (bool)isImmuneProperty.GetValue(state.playerHealthSystem);
                        UnityEngine.Debug.Log($"[Shield] Verification - playerHealthSystem.IsShieldImmune = {isImmune}");
                        if (!isImmune)
                        {
                            UnityEngine.Debug.LogError("[Shield] WARNING: Shield immunity was NOT set correctly!");
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("[Shield] ERROR: playerHealthSystem is NULL after ApplyShield()!");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[Shield] EXCEPTION in ApplyEffects: {e.Message}\n{e.StackTrace}");
            }
        }

        public void CleanupEffects(SkillInstance instance, ISkillDependencies dependencies)
        {
            if (shieldStates.TryGetValue(instance, out var state))
            {
                RemoveShield(instance, dependencies, state);
            }
        }

        public Quaternion GetEffectRotation(Vector3 playerForward, Quaternion defaultRotation)
        {
            // Shield doesn't need special rotation
            return defaultRotation;
        }
    
        private void ApplyShield(SkillInstance instance, ISkillDependencies dependencies, ShieldState state)
        {
            if (dependencies?.PlayerTransform == null)
            {
                if (Debug) UnityEngine.Debug.LogError("[Shield] PlayerInstance not found!");
                return;
            }

            GameObject player = dependencies.PlayerTransform.gameObject;
            
            // Get EntityData and HealthSystem using reflection
            var entityDataType = System.Type.GetType("EntityData") ?? System.Type.GetType("EntityData, Assembly-CSharp");
            var healthSystemType = System.Type.GetType("HealthSystem") ?? System.Type.GetType("HealthSystem, Assembly-CSharp");
            
            if (entityDataType != null)
            {
                state.playerEntityData = player.GetComponent(entityDataType) as MonoBehaviour;
            }
            
            if (healthSystemType != null)
            {
                state.playerHealthSystem = player.GetComponent(healthSystemType) as MonoBehaviour;
                if (state.playerHealthSystem == null)
                {
                    state.playerHealthSystem = player.GetComponentInChildren(healthSystemType) as MonoBehaviour;
                }
            }
            
            UnityEngine.Debug.Log($"[Shield] Looking for HealthSystem on {player.name}...");
            
            // Set shield immunity
            if (state.playerHealthSystem != null)
            {
                var setShieldMethod = state.playerHealthSystem.GetType().GetMethod("SetShieldImmunity",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (setShieldMethod != null)
                {
                    setShieldMethod.Invoke(state.playerHealthSystem, new object[] { true });
                    state.isShieldActive = true;
                    UnityEngine.Debug.Log($"[Shield] Player shield immunity enabled on {player.name}");
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"[Shield] Player HealthSystem not found on {player.name}!");
            }
            
            // Apply defense boost if EntityData exists
            if (state.playerEntityData != null)
            {
                var currentDefenseProperty = state.playerEntityData.GetType().GetProperty("currentDefense",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (currentDefenseProperty != null)
                {
                    if (!state.hasOriginalDefenseBeenSaved)
                    {
                        state.originalDefense = (float)currentDefenseProperty.GetValue(state.playerEntityData);
                        state.hasOriginalDefenseBeenSaved = true;
                        if (Debug) UnityEngine.Debug.Log($"[Shield] Original defense saved: {state.originalDefense:F2}");
                    }
                    
                    float shieldDefense = state.originalDefense + damageReduction;
                    shieldDefense = Mathf.Clamp01(shieldDefense);
                    currentDefenseProperty.SetValue(state.playerEntityData, shieldDefense);
                    
                    if (Debug) UnityEngine.Debug.Log($"[Shield] Applied shield! Defense: {state.originalDefense:F2} -> {shieldDefense:F2}");
                }
            }
            
            // Start shield duration timer using SkillController (handled by SkillController's duration system)
            // The SkillController will call CleanupEffects when duration expires
            
            // Create visual effect
            CreateShieldEffect(dependencies, state);
        }
    
        private void RemoveShield(SkillInstance instance, ISkillDependencies dependencies, ShieldState state)
        {
            if (!state.isShieldActive) return;
            
            // Stop coroutine if running
            if (state.durationCoroutine != null && dependencies?.PlayerTransform != null)
            {
                var monoBehaviour = dependencies.PlayerTransform.GetComponent<MonoBehaviour>();
                if (monoBehaviour != null)
                {
                    monoBehaviour.StopCoroutine(state.durationCoroutine);
                }
            }
            
            // Disable shield immunity
            if (state.playerHealthSystem != null)
            {
                var setShieldMethod = state.playerHealthSystem.GetType().GetMethod("SetShieldImmunity",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (setShieldMethod != null)
                {
                    setShieldMethod.Invoke(state.playerHealthSystem, new object[] { false });
                    if (Debug) UnityEngine.Debug.Log("[Shield] Player shield immunity disabled");
                }
            }
            
            // Restore original defense
            if (state.playerEntityData != null && state.hasOriginalDefenseBeenSaved)
            {
                var currentDefenseProperty = state.playerEntityData.GetType().GetProperty("currentDefense",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (currentDefenseProperty != null)
                {
                    currentDefenseProperty.SetValue(state.playerEntityData, state.originalDefense);
                    if (Debug) UnityEngine.Debug.Log($"[Shield] Shield expired! Defense restored to {state.originalDefense:F2}");
                }
            }
            
            state.isShieldActive = false;
            state.hasOriginalDefenseBeenSaved = false;
            
            // Remove visual effect
            RemoveShieldEffect(state);
        }
    
        private void CreateShieldEffect(ISkillDependencies dependencies, ShieldState state)
        {
            if (dependencies?.PlayerTransform == null)
            {
                UnityEngine.Debug.LogWarning("[Shield] PlayerInstance not found - cannot create shield effect");
                return;
            }

            // Remove any existing shield animation first
            if (state.currentShieldAnim != null)
            {
                state.currentShieldAnim.StopShieldAnimation();
                if (state.currentShieldAnim.gameObject != null)
                {
                    Object.Destroy(state.currentShieldAnim.gameObject);
                }
                state.currentShieldAnim = null;
            }

            if (shieldPrefab != null)
            {
                GameObject shieldObj = Object.Instantiate(shieldPrefab, dependencies.PlayerTransform);
                shieldObj.name = "ShieldEffect";
                
                shieldObj.transform.localPosition = Vector3.zero;
                shieldObj.transform.localRotation = Quaternion.identity;
                shieldObj.transform.localScale = shieldScale;
                shieldObj.SetActive(true);
                
                state.currentShieldAnim = shieldObj.GetComponent<ShieldAnim>();
                if (state.currentShieldAnim == null)
                {
                    UnityEngine.Debug.LogWarning("[Shield] ShieldAnim component not found on prefab, adding it");
                    state.currentShieldAnim = shieldObj.AddComponent<ShieldAnim>();
                }
                
                SpriteRenderer spriteRenderer = shieldObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = shieldObj.AddComponent<SpriteRenderer>();
                }
                spriteRenderer.sortingOrder = 10;
                spriteRenderer.sortingLayerName = "Default";
                
                if (state.currentShieldAnim.shieldAnim == null || state.currentShieldAnim.shieldAnim.Length == 0)
                {
                    UnityEngine.Debug.LogWarning("[Shield] ShieldAnim sprite array is empty!");
                }
                else
                {
                    state.currentShieldAnim.AnimateShield();
                    UnityEngine.Debug.Log($"[Shield] Shield frame animation started with {state.currentShieldAnim.shieldAnim.Length} sprites");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[Shield] Shield prefab is not assigned!");
            }
        }
    
        private void RemoveShieldEffect(ShieldState state)
        {
            if (state.currentShieldAnim != null)
            {
                state.currentShieldAnim.StopShieldAnimation();
                if (state.currentShieldAnim.gameObject != null)
                {
                    Object.Destroy(state.currentShieldAnim.gameObject);
                }
                state.currentShieldAnim = null;
            }
        }

        public override SkillInstance CreateInstance(bool isUnlocked = false)
        {
            var instance = base.CreateInstance(isUnlocked);
            return instance;
        }
    }
}
