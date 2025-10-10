using UnityEngine;
using UnityEngine.UI;

// level indicator squares.
[RequireComponent(typeof(Image))]
public class LevelSquare : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color squareColor = Color.white;
    [SerializeField] private Vector2 size = new Vector2(20, 20);
    
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        
        if (image != null)
        {
            image.color = squareColor;
        }

        // Set size
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = size;
        }
    }

    public void SetColor(Color color)
    {
        squareColor = color;
        if (image != null)
        {
            image.color = color;
        }
    }
}

