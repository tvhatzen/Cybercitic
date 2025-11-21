using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Piercing Shot", menuName = "Scriptable Objects/Skills/Piercing Shot")]
public class PiercingShot : Skill
{
    [Header("Piercing Shot Specific")]
    [SerializeField] private int additionalDamage = 50; // high damage bonus
    [SerializeField] private float pierceRange = 10f; // how far the shot travels
    [SerializeField] private float pierceWidth = 0.5f; // width of the piercing line
    [SerializeField] private LayerMask enemyLayerMask = -1; // what layers can be hit
    [SerializeField] private GameObject effectPrefab; // visual effect prefab (optional - if set, uses its rotation)

    protected override void ApplySkillEffects()
    {
        base.ApplySkillEffects();
        
        if(debug) Debug.Log($"Piercing Shot activated! Firing high-damage shot with {skillDamage + additionalDamage} damage!");
        
        // Find and rotate the particle effect that was just created
        // Use a coroutine to wait a frame so it's fully instantiated
        if (PlayerInstance.Instance != null)
        {
            Debug.Log("[PiercingShot] Starting RotateParticleEffectCoroutine");
            PlayerInstance.Instance.StartCoroutine(RotateParticleEffectCoroutine());
        }
        else
        {
            Debug.LogError("[PiercingShot] PlayerInstance.Instance is null, cannot start coroutine");
        }
        
        // fire piercing shot
        FirePiercingShot();
    }
    
    private IEnumerator RotateParticleEffectCoroutine()
    {
        Debug.Log("[PiercingShot] RotateParticleEffectCoroutine started");
        
        // Wait a couple frames for the particle effect to be instantiated
        yield return null;
        yield return null;
        
        Debug.Log("[PiercingShot] After waiting frames, searching for particle effect");
        
        if (PlayerInstance.Instance == null)
        {
            Debug.LogError("[PiercingShot] PlayerInstance.Instance is null in coroutine");
            yield break;
        }
        
        Vector3 playerPos = PlayerInstance.Instance.transform.position;
        Debug.Log($"[PiercingShot] Player position: {playerPos}");
        
        // Find ALL particle systems in the scene
        ParticleSystem[] allParticleSystems = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        Debug.Log($"[PiercingShot] Found {allParticleSystems.Length} ParticleSystem objects in scene");
        
        ParticleSystem closestParticle = null;
        float closestDistance = float.MaxValue;
        
        foreach (ParticleSystem ps in allParticleSystems)
        {
            GameObject obj = ps.gameObject;
            Transform rootTransform = obj.transform.root;
            GameObject rootObj = rootTransform.gameObject;
            
            // Check if it's a clone (check both the object and root)
            bool isClone = obj.name.Contains("(Clone)") || rootObj.name.Contains("(Clone)");
            
            if (isClone)
            {
                float distance = Vector3.Distance(playerPos, rootObj.transform.position);
                Debug.Log($"[PiercingShot] Found clone: {rootObj.name} at distance {distance}");
                
                // Increase search radius to 20 units
                if (distance < 20f && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestParticle = ps;
                }
            }
        }
        
        if (closestParticle != null)
        {
            // Apply ONLY 90 degrees rotation on Y axis to the root object
            Quaternion yAxis90Rotation = Quaternion.Euler(0, 90, 0);
            GameObject rootObj = closestParticle.transform.root.gameObject;
            rootObj.transform.rotation = yAxis90Rotation;
            Debug.Log($"[PiercingShot] Rotated particle effect root {rootObj.name} to 90° Y rotation: {yAxis90Rotation.eulerAngles}");
        }
        else
        {
            Debug.LogWarning("[PiercingShot] Could not find particle effect to rotate");
        }
    }
    
    private void FirePiercingShot()
    {
        // get player position and rotation
        Vector3 playerPos = Vector3.zero;
        Quaternion playerRotation = Quaternion.identity;
        
        if (PlayerInstance.Instance != null)
        {
            playerPos = PlayerInstance.Instance.transform.position;
            playerRotation = PlayerInstance.Instance.transform.rotation;
        }
        
        // Determine the firing direction and rotation based on prefab's rotation
        // Add 90 degrees on Y axis to correct the direction (shooting forward instead of behind)
        Vector3 firingDirection = Vector3.forward;
        Quaternion effectRotation = Quaternion.identity;
        Quaternion yAxis90Rotation = Quaternion.Euler(0, 90, 0); // 90 degree rotation on Y axis
        
        if (effectPrefab != null)
        {
            // Use the prefab's exact rotation with 90 degree Y-axis offset
            effectRotation = effectPrefab.transform.rotation * yAxis90Rotation;
            // Get the prefab's forward direction rotated 90 degrees on Y axis
            firingDirection = effectRotation * Vector3.forward;
            if(debug) Debug.Log($"[PiercingShot] Using effect prefab rotation (with 90° Y offset): {effectRotation.eulerAngles}, forward: {firingDirection}");
        }
        else
        {
            // Fallback: use player's rotation with 90 degree Y-axis offset
            effectRotation = playerRotation * yAxis90Rotation;
            firingDirection = effectRotation * Vector3.forward;
            if(debug) Debug.Log($"[PiercingShot] Using player rotation (with 90° Y offset): {effectRotation.eulerAngles}, forward: {firingDirection}");
        }
        
        // cast a line to find all enemies in the piercing path
        // Use the prefab's rotation to match its exact orientation
        RaycastHit[] hits = Physics.BoxCastAll(
            playerPos, 
            Vector3.one * pierceWidth, 
            firingDirection, 
            effectRotation, 
            pierceRange, 
            enemyLayerMask
        );
        
        if(debug) Debug.Log($"[PiercingShot] Found {hits.Length} enemies in piercing path");
        
        // apply damage to all enemies hit
        foreach (var hit in hits)
        {
            HealthSystem enemyHealth = hit.collider.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                int totalDamage = skillDamage + additionalDamage;
                enemyHealth.TakeDamage(totalDamage);
                if(debug) Debug.Log($"[PiercingShot] Dealt {totalDamage} damage to {hit.collider.name}");
            }
        }
    }
    
    private Transform FindClosestEnemy()
    {
        // get player position
        Vector3 playerPos = Vector3.zero;
        if (PlayerInstance.Instance != null)
        {
            playerPos = PlayerInstance.Instance.transform.position;
        }
        
        // find all enemies in range
        Collider[] enemies = Physics.OverlapSphere(playerPos, skillRange, LayerMask.GetMask("Enemy"));
        
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(playerPos, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }
        return closestEnemy;
    }
}
