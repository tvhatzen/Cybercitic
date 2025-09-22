using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class trigger : MonoBehaviour
{
    private SceneManager sceneManager;

    private void OnCollisionEnter(Collision player)
    {
        GameObject playerobj = GameObject.FindGameObjectWithTag("Player");
        // reference the player instance instead
        if (playerobj)
        {
            Debug.Log("collided with player");
            SceneManager.LoadScene("level2");
        }
    }
}
