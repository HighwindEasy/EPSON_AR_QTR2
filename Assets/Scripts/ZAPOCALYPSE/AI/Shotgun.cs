﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Shotgun : Survivor
{
    public enum Shotgun_State
    {
        S_IDLE = 1,
        S_PANIC,
        S_SEARCH,
        S_SWITCHSEARCH,
        S_ATTACK,
        S_DEAD,
    }
    
    [Header("Shotgun Specific Statistics")]
    public Shotgun_State shotgunstate;
    private Animator anim;

    #region SOUND
    private GameControl gameControl;
    private Slider SGSlider;
    public AudioClip shootSound;
    private AudioSource source;
    #endregion

    void Awake()
    {
        base.Awake();
        shotgunstate = Shotgun_State.S_IDLE;
        source = GetComponent<AudioSource>();
    }

	// Use this for initialization
	void Start ()
    {
        base.Start();
        SGSlider = GameObject.FindGameObjectWithTag("SGHP").GetComponent<Slider>();
        GameObject.FindGameObjectWithTag("ShotgunLVL").GetComponent<UnitGrowthResult>().Unit = this.gameObject;
        gameControl = GameObject.Find("GameControl").GetComponent<GameControl>();
        this.atkRange = 15.0f;
        anim = this.GetComponent<Animator>();
        //this.atkRange = gameControl.range_Shotgun;//25.0f;
        this.timeActive = gameControl.durationUp_Shotgun;
        this.experiencePt = gameControl.EXP_Shotgun;
        this.HP = gameControl.HP_Shotgun;
        this.level = gameControl.LVL_Shotgun;
	
    }
	
	// Update is called once per frame
	void Update ()
    {
        base.Update();
        RunFSM();
        Regenerate();
        ScaleHP();
    }

    void ScaleHP()
    {
        SGSlider.value = ((float)HP / (float)i_maxHP);
    }

    public override void RunFSM()
    {
        if(this.HP <= 0 || Ustate == UnitState.S_DEAD)
        {
            shotgunstate = Shotgun_State.S_DEAD;
        }
        switch (shotgunstate)
        {
            case Shotgun_State.S_IDLE:
                {
                    if (this.Enemynear(this.atkRange))
                    {
                        target = SelectTarget(this.atkRange, this.transform.position);
                        shotgunstate = Shotgun_State.S_ATTACK;
                    }
                    else if (this.Enemynear(1f))
                    {
                        
                        target = SelectTarget(1f, this.transform.position);
                        shotgunstate = Shotgun_State.S_PANIC;
                    }
                    else
                    {
                        anim.SetTrigger("IDLE");
                    }
                    break;
                }
            case Shotgun_State.S_PANIC:
                {
                    ShoveEnemy(target);
                    anim.SetTrigger("SHOVE");
                    break;
                }
            case Shotgun_State.S_SEARCH:
                {
                    break;
                }
            case Shotgun_State.S_SWITCHSEARCH:
                {
                    break;
                }
            case Shotgun_State.S_ATTACK:
                {

                    if (target != null && target.activeSelf)
                    {
                        anim.SetTrigger("ATTACK");
                        Vector3 V3_Direction = (target.transform.position - this.transform.position).normalized;
                        Quaternion lookRotation = Quaternion.LookRotation(V3_Direction);
                        this.gameObject.transform.rotation = Quaternion.Slerp(this.gameObject.transform.rotation, lookRotation, Time.deltaTime * 5);
                        if (currAmmo > 0)
                            Attackenemy(V3_Direction, 10, true);
                        else
                        {
                            anim.SetTrigger("RELOAD");
                            Reload();
                        }
                    }
                    else
                    {
                        shotgunstate = Shotgun_State.S_IDLE;
                    }
                    break;
                }
            case Shotgun_State.S_DEAD:
                {
                    anim.SetBool("DIE", true);
                    DestroyGO();
                    break;
                }
        }

    }

   

    void Attackenemy(Vector3 Direction, int shellCount, bool spread = false)
    {
        attackRate -= Time.deltaTime;
        if (attackRate <= 0)
        {
            source.PlayOneShot(shootSound, 1.2F);
            Vector3 pew = this.gameObject.transform.position;
            GameObject bullet = null;
            //direction.y += 2;
            //direction.y += 0.05f;
            for (int i = 0; i < shellCount; i++)
            {
                float spreadpew = 0.1f;

                float RandomX = Random.Range(-spreadpew, spreadpew);
                float RandomZ = Random.Range(-spreadpew, spreadpew);

                Vector3 offset = new Vector3(RandomX, 0, RandomZ);
                Direction = (offset + Direction).normalized;
                Direction.Normalize();

                //direction.y += 2;
                pew.y = 0.5f;
                bullet = Instantiate(EProjectile, pew, Quaternion.identity) as GameObject;
                bullet.GetComponent<Rigidbody>().AddForce(Direction * bullet.GetComponent<Projectile>().ProjectileSpeed, ForceMode.Impulse);
                bullet.GetComponent<Projectile>().Sender = this.gameObject;
                bullet.GetComponent<Projectile>().ProjectileLifetime = 0.65f;
            }
            bullet.transform.parent = this.transform.parent;
            attackRate = atkSpd;
            currAmmo--;
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, atkRange);
    }
}
