using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    // set up transition with image moving horizontally across screen for a set duration
    public Image transitionImage;
    public float transitionDuration;

    // make a coroutine to play the animation
    private IEnumerator TransitionScene()
    {


        yield return null;
    }

}
