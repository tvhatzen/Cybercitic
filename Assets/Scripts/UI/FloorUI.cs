using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorUI : MonoBehaviour
{
    public TextMeshProUGUI floorText; 
    
    public bool debug = false;

    void OnEnable()
    {
        // Subscribe to centralized Event Bus
        GameEvents.OnFloorChanged += UpdateFloorText;
        
        if (FloorManager.Instance != null)
        {
            UpdateFloorText(FloorManager.Instance.CurrentFloor);
            if(debug) Debug.Log($"[FloorUI] Updated floor display on enable to: Floor {FloorManager.Instance.CurrentFloor}");
        }
    }

    void OnDisable() => GameEvents.OnFloorChanged -= UpdateFloorText;

    void Start()
    {
        // show initial floor
        if (FloorManager.Instance != null)
            UpdateFloorText(FloorManager.Instance.CurrentFloor);
    }

    public void UpdateFloorText(int floor)
    {
        if (floorText != null) floorText.text = $"Floor {floor} / 5";
    }
}
