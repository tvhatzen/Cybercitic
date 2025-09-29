using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : SingletonBase<PlayerStats>
{
    // money

    // upgrades unlocked
    public List<Skill> skills = new List<Skill>();
    // upgrade levels
    public List<Upgrade> upgrades = new List<Upgrade>(); // check if i can make a list but use inherited classes

    // stats (speed, health, dodge chance, etc)
    public int Health;
    public float speed;
    public int attack;
    public float dodgeChance;

    // have event for stats changed, link with upgrades purchased

    // either make one method taking stat argument, and amount arg
}
