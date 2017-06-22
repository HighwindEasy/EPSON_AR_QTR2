﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Melee : Survivor
{
    public enum Melee_State
    {
        S_IDLE = 1,
        S_ATTACK,
        S_DEAD,
    }

    public GameObject target;
    public Melee_State meleestate;

    void Awake()
    {
        target = null;
        meleestate = Melee_State.S_IDLE;
    }
    // Use this for initialization
    void Start ()
    {
        this.atkRange = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        RunFSM();
    }

    public override void RunFSM()
    {
        if (this.HP <= 0)
        {
            meleestate = Melee_State.S_DEAD;
        }
        switch (meleestate)
        {
            case Melee_State.S_IDLE:
                {
                    if (this.Enemynear(this.atkRange))
                    {
                        target = SelectTarget(this.atkRange);
                        meleestate = Melee_State.S_ATTACK;
                    }
                    break;
                }
            case Melee_State.S_ATTACK:
                {
                    if (target != null)
                    { 
                        Attackenemy(target);
                    }
                    else
                    {
                        meleestate = Melee_State.S_IDLE;
                    }
                    break;
                }
            case Melee_State.S_DEAD:
                {
                    Destroy(this.gameObject);
                    break;
                }
        }

    }

   

    void Attackenemy(GameObject target)
    {
        attackRate -= Time.deltaTime;
        if (attackRate <= 0)
        {
            int tempHP = target.GetComponent<Zombie>().HP;
            tempHP -= atkDmg;
            target.GetComponent<Zombie>().SetHealth(tempHP);
            attackRate = atkSpd;
        }
    }
}
