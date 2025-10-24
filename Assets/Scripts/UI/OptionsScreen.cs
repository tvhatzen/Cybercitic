using UnityEngine;
using UnityEngine.UI;


public class OptionsScreen : MonoBehaviour
{

    public Slider volumeSlider;

    void Start()
    {
        
    }
    public void ChangeVolume()
    {
        AudioListener.volume = volumeSlider.value;
        Debug.Log("Volume changed");
    }

    private void SaveSettings()
    {

    }

    private void LoadSettings()
    {

    }
}
