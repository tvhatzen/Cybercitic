using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
    #region Variables
    // references
    public PlayerStats playerStats;
    // skills can be obtained by beating mini-bosses and bosses. they are held in the inventory
    //  icons
    [SerializeField] private SpriteRenderer skillIcon;
    [SerializeField] private SpriteRenderer castIcon;
    //  cooldowns
    [SerializeField] private bool isOnCooldown;
    [SerializeField] private float cooldown;
    //  duration
    [SerializeField] private float duration;
    //  animation
    [SerializeField] private AnimationClip animation;
    //  sound effect
    [SerializeField] private AudioClip skillSound;
    //  casting time ?
    [SerializeField] private float castingTime;
    //  damage
    [SerializeField] private int skillDamage;
    //  range
    [SerializeField] private float skillRange;
    // charges
    [SerializeField] private int charges;

    #endregion

    // handle states 
    public enum SkillStates 
    {
        ReadyToUse,
        Casting,
        Cooldown
    }

    public virtual void Start()
    {
        isOnCooldown = true;
    }

    public virtual void Activate()
    {
        // check if state is ready to use
        if (!isOnCooldown)
        {
            // cast
            //StartCoroutine(Cast()); saying it doesnt exist in current context ?
        }

        // if not, change state to cooldown
    }

    public IEnumerator Cast()
    {
        Debug.Log("Casting Skill " + Time.time);

        // check if cooldown is over

        // change state to casting 

        // change icon to casting
        // play animation
        // apply damage to enemies

        yield return new WaitForSeconds(castingTime);
        Debug.Log("Finished Casting Skill " + Time.time);
    }
}
