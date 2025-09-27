using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInstance : SingletonBase<GameManager>
{
    private static PlayerInstance instance;

    protected override void Awake()
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
