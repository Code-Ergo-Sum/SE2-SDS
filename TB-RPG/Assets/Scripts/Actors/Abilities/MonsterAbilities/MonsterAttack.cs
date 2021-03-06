﻿using UnityEngine;
using System.Collections;

public class MonsterAttack : Ability {

    public MonsterAttack(Actor Owner) : 
                   base(null, null,
                        100, false, Owner)
    {

    }

    public void showAttackAnimation()
    {
        
    }

    public override void cast(Actor act = null)
    {


       if (act == null)
        {
            //Get the party with GameMaster.instance.thePlayer.theParty
            Actor[] party = GameMaster.instance.thePlayer.theParty;
            //Pick a character at random
            System.Random rand = new System.Random();


            while (GameMaster.instance.thePlayer.partyIsDead == false)
            {
                int k = rand.Next(0, party.Length);
                if (party[k] != null && party[k].isAlive)
                {
                    act = party[k];
                    break;
                }
            }
        }

        //Debug.Log("Owner before act.damage(owner.stats[strength].effectiveLevel, owner);" + owner);
        act.damage(owner.stats["strength"].effectiveLevel, owner, Ability.damageType.melee);
        owner.stamina.subtract(stamina);


    }


}
