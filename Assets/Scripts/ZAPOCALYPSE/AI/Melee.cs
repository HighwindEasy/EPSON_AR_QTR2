﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Melee : Survivor
{
    public enum Melee_State
    {
        S_IDLE = 1,
        S_SEARCH,
        S_ATTACK,
        S_DEAD,
    }

    [Header("Melee Specific Statistics")]
    private Animator Anim;
    public Melee_State meleestate;
    private Slider CCSlider;

    void Awake()
    {
        base.Awake();
        Anim = GetComponent<Animator>();
        meleestate = Melee_State.S_IDLE;
        Ustate = UnitState.S_HEALTHY;
    }
    // Use this for initialization
    void Start ()
    {
        base.Start();
        CCSlider = GameObject.FindGameObjectWithTag("CCHP").GetComponent<Slider>();
        GameObject.FindGameObjectWithTag("MeleeLVL").GetComponent<UnitGrowthResult>().Unit = this.gameObject;
        this.atkRange = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        RunFSM();
        RunDeathDoor();
        ScaleHP();
    }

    void ScaleHP()
    {
        CCSlider.value = ((float)HP / (float)i_maxHP);
    }

    public override void RunFSM()
    {
        if (this.HP <= 0 || Ustate == UnitState.S_DEAD)
        {
            meleestate = Melee_State.S_DEAD;
        }
        switch (meleestate)
        {
            case Melee_State.S_IDLE:
                {
                    if (this.Enemynear(this.atkRange))
                    {
                        meleestate = Melee_State.S_SEARCH;
                        Anim.SetTrigger("IDLE");
                    }
                    break;
                }
            case Melee_State.S_SEARCH:
                {
                    target = SelectTarget(this.atkRange,this.transform.position);
                    meleestate = Melee_State.S_ATTACK;
                    break;
                }
            case Melee_State.S_ATTACK:
                {
                    if (target != null && target.activeSelf)
                    {
                        Vector3 V3_Direction = (target.transform.position - this.transform.position).normalized;
                        Quaternion lookRotation = Quaternion.LookRotation(V3_Direction);
                        this.gameObject.transform.rotation = Quaternion.Slerp(this.gameObject.transform.rotation, lookRotation, Time.deltaTime * 5);
                        Attackenemy(target);
                        Anim.SetTrigger("ATTACK");
                    }
                    else
                    {
                        meleestate = Melee_State.S_IDLE;
                    }
                    break;
                }
            case Melee_State.S_DEAD:
                {
                    Anim.SetBool("DIE", true);
                    DestroyGO();
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, atkRange);
    }


}
