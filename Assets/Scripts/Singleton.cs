using UnityEngine;

/// <summary>
/// Generic Singleton base. Any MonoBehaviour inheriting this becomes a singleton.
/// </summary>
public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<T>();
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // Check if another instance already exists
        if (_instance != null && _instance != this)
        {
            // Destroy this duplicate immediately without calling DontDestroyOnLoad
            Destroy(gameObject);
            return;
        }
        
        // This is the first/only instance
        _instance = this as T;
        
        // DontDestroyOnLoad only works on root GameObjects
        // If this object has a parent, detach it first
        if (transform.parent != null)
        {
            Debug.LogWarning($"[Singleton] {typeof(T).Name} is a child object. Detaching from parent to apply DontDestroyOnLoad.");
            transform.SetParent(null);
        }
        
        // Apply DontDestroyOnLoad to persist across scene loads
        DontDestroyOnLoad(gameObject);
    }
}
