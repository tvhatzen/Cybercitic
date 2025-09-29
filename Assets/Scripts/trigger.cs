using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider))]
public class trigger : MonoBehaviour
{
    [Header("Next Floor Settings")]
    public string nextFloorSceneName; // scene to load when triggered

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player reached floor end! Loading {nextFloorSceneName}");
            LoadNextFloor();
        }
    }

    private void LoadNextFloor()
    {
        if (!string.IsNullOrEmpty(nextFloorSceneName))
        {
            SceneManager.LoadScene(nextFloorSceneName);
            FloorManager.Instance.LoadNextFloor();
        }
        else
            Debug.LogWarning("Next floor scene name not set on trigger!");
    }
}
// fire floor chang event 