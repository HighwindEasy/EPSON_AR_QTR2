﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shotgun : Survivor
{
    public enum Shotgun_State
    {
        S_IDLE = 1,
        S_PANIC,
        S_ATTACK,
        S_DEAD,
    }

    public GameObject target;
    public Shotgun_State shotgunstate;

    void Awake()
    {
        target = null;
        shotgunstate = Shotgun_State.S_IDLE;
    }

	// Use this for initialization
	void Start ()
    {
        this.atkRange = 15.0f;
	}
	
	// Update is called once per frame
	void Update ()
    {
        RunFSM();
	}

    public override void RunFSM()
    {
        if(this.HP <= 0)
        {
            shotgunstate = Shotgun_State.S_DEAD;
        }
        switch (shotgunstate)
        {
            case Shotgun_State.S_IDLE:
                {
                    if (this.Enemynear(this.atkRange))
                    {
                        target = SelectTarget();
                        shotgunstate = Shotgun_State.S_ATTACK;
                    }
                    break;
                }
            case Shotgun_State.S_PANIC:
                {
                    break;
                }
            case Shotgun_State.S_ATTACK:
                {
                    if (target != null)
                    {
                        Vector3 V3_Direction = (target.transform.position - this.transform.position).normalized;
                        Attackenemy(V3_Direction, 10, true) ;
                    }
                    else
                    {
                        shotgunstate = Shotgun_State.S_IDLE;
                    }
                    break;
                }
            case Shotgun_State.S_DEAD:
                {
                    Destroy(this.gameObject);
                    break;
                }
        }

    }

    GameObject SelectTarget()
    {
        GameObject[] AllEnemies = GameObject.FindGameObjectsWithTag("test");
        List<GameObject> Targetables = new List<GameObject>();
        List<GameObject> TargetList = new List<GameObject>();
        if (AllEnemies != null)
        {
            foreach (GameObject go in AllEnemies)
            {
                if (go == null)
                {
                    continue;
                }
                else if (Vector3.Angle(go.transform.position - this.transform.position, this.GetDirection().transform.position - this.transform.position)
                    <= 45 && Vector3.Distance(go.transform.position, this.transform.position) <= atkRange)
                {
                    Targetables.Add(go);
                }
            }

            foreach (GameObject go in Targetables)
            {
                for (int i = 0; i < go.GetComponent<Zombie>().GetThreat(); i++)
                {
                    TargetList.Add(go);
                }
            }
            int targetindex = Random.Range(0, TargetList.Count);

            return TargetList[targetindex];
        }
        return null;
    }

    void Attackenemy(Vector3 Direction, int shellCount, bool spread = false)
    {
        attackRate -= Time.deltaTime;
        if (attackRate <= 0)
        {
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

                bullet = Instantiate(EProjectile, pew, Quaternion.identity) as GameObject;
                bullet.GetComponent<Rigidbody>().AddForce(Direction * bullet.GetComponent<Projectile>().ProjectileSpeed, ForceMode.Impulse);
                bullet.GetComponent<Projectile>().Sender = this.gameObject;
                bullet.GetComponent<Projectile>().ProjectileLifetime = 0.65f;
            }
            bullet.transform.parent = this.transform.parent;
            attackRate = atkSpd;
        }
    }
}
