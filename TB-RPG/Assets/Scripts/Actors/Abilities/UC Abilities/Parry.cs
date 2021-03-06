﻿using UnityEngine;

//This ability is used when a userControllable attacks a monster
//see monsterAttack for when a monster attacks a userControllable
public class Parry : Ability
{


    public void showAttackAnimation(Monster m)
    {
        //Program animation here
        //We might have a static class of generic animations that this can refer to
        //Also each monster will contain a reference to its image, to make things easier
    }

    public Parry() : base() { }

    public Parry(Actor Owner) : base("Counter Attack", "Counter-attack an enemy that dealt melee damage to you. Damages by the amount that the enemy hits you with.", -1, true, Owner)
    {

    }
}
