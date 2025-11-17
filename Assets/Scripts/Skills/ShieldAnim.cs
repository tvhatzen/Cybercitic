using UnityEngine;

public class ShieldAnim : MonoBehaviour
{
    public Sprite[] shieldAnim;
    public bool animating = false;


    // Update is called once per frame
    public void AnimateShield()
    {
        // llopp through frames
        for (int i = 0; i < shieldAnim.Length; i++)
        {


            animating = true;
        }
    }
}
