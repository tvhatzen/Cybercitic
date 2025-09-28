using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorUI : MonoBehaviour
{
    public TextMeshProUGUI floorText; 

    void OnEnable()
    {
        FloorManager.OnFloorChanged += UpdateFloorText;
    }

    void OnDisable()
    {
        FloorManager.OnFloorChanged -= UpdateFloorText;
    }

    void Start()
    {
        // show initial floor
        if (FloorManager.Instance != null)
            UpdateFloorText(FloorManager.Instance.CurrentFloor);
    }

    private void UpdateFloorText(int floor)
    {
        if (floorText != null)
            floorText.text = $"Floor {floor}";
    }
}
