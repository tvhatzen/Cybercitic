using UnityEngine;
using UnityEngine.UI;

public class FloorProgressBar : MonoBehaviour
{
    [SerializeField] private Image _floorProgressSprite;

    int progressAmount;
    public Slider progressSlider;

    private void Start()
    {
        ResetProgress();
    }

    public void IncreaseProgressAmount(int amount)
    {
        progressAmount += amount;

        progressSlider.value = progressAmount;

        if (progressAmount >= 100)
        {
            Debug.Log("FloorProgressBar: Boss Floor beat! won game");
        }
    }

    public void ResetProgress()
    {
        progressAmount = 0;
        progressSlider.value = 0;
    }
}
