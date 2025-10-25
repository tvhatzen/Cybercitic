using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSlideshow : MonoBehaviour
{
    public Button finishButton;
    public Image slideImage;
    public Sprite[] slides;

    private int currentSlideIndex = 0;

    void Start()
    {
        finishButton.IsDestroyed();
        UpdateSlide();
    }

    public void NextSlide()
    {
        currentSlideIndex++;
        if (currentSlideIndex >= slides.Length)
        {
            currentSlideIndex = 0;
        }
        UpdateSlide();
    }

    private void UpdateSlide()
    {
        if (slideImage != null && slides.Length > 0)
        {
            slideImage.sprite = slides[currentSlideIndex];
        }
    }
}
