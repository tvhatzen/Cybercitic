using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInstance : SingletonBase<PlayerInstance>
{
    protected override void Awake()
    {
        base.Awake(); 

        if (!CompareTag("Player"))
        {
            tag = "Player";
        }

        DontDestroyOnLoad(gameObject);
    }
}
