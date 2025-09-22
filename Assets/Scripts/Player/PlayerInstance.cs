using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInstance : MonoBehaviour
{
    private static PlayerInstance instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
