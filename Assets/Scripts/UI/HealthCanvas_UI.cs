using UnityEngine;

public class HealthCanvas_UI : MonoBehaviour
{
    public static HealthCanvas_UI Instance { get; private set; }

    [Header("World Space Canvas")]
    public Transform worldSpaceCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
}
