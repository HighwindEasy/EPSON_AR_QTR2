﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Survivor : EntityBase
{
    
    public enum SURVIVOR_TYPE
    {
        S_RIFLE,
        S_MELEE,
        S_SHOTGUN,
        S_MEDIC,
        S_MECHANIC,
    }

    protected enum UnitState
    {
        S_HEALTHY = 1,
        S_DEATHDOOR,
        S_DEAD,
    }


    public GameObject EProjectile;

    //EntityBase test;
    public SURVIVOR_TYPE entityType = SURVIVOR_TYPE.S_RIFLE;
    [HideInInspector]
    public float attackRate;
    public float reloadRate;                                                //! Reload Rate of Survivor
    private float reloadDt;
    private float idleDt = 5.0f;

    public int maxAmmo;                                                     //! Max Ammo Survivor Have
    [HideInInspector]
    public int currAmmo;                                                    //! Current Ammo Survivor Has

    [HideInInspector]
    public float experiencePt;                                              //! Experience Pt of Survivor to Level
    [HideInInspector]
    public float timeActive;                                                //! Experience Pt of Survivor to Level
    public int level;                                                       //! Experience Pt of Survivor to Level

    [SerializeField]
    GameObject Directionpoint;
    [SerializeField]
    protected GridMap The_Grid;
    public GameObject target;
    public Vector3 V3_targetpos;


    [Header("Death Door Values")]
    #region DEATH DOOR VALUES
    [SerializeField]
    protected float PanicRange;
    [SerializeField]
    float f_enmity;
    [SerializeField]
    float f_DeathDoorrefresher;
    [SerializeField]
    float f_DeathDoortimer;
    [SerializeField]
    float f_DeathChance;
    [SerializeField]
    bool b_deathdoorfail;

    [SerializeField]
    float f_DeathdoorAttackSpeed;
    [SerializeField]
    int i_DeathdoorAttackDamage;
    [SerializeField]
    float f_OriginalAttackSpeed;
    [SerializeField]
    int i_OriginalAttackDamage;

    [SerializeField]
    protected UnitState Ustate;
    #endregion //DEATH_DOOR




    

    protected void Awake()
    {
        PanicRange = 0.5f;
        target = null;
        Ustate = UnitState.S_HEALTHY;
    }

    // Use this for initialization
    protected void Start()
    {
        reloadDt = reloadRate;
        f_DeathDoorrefresher = 60f;
        f_DeathDoortimer = f_DeathDoorrefresher;
        attackRate = GetAttackSpeed();


        The_Grid = GameObject.Find("Game Manager").GetComponent<GridMap>();
        //The_Grid.SetTargetRange(gameObject);

        i_OriginalAttackDamage = GetAttackDamage();
        f_OriginalAttackSpeed = GetAttackSpeed();
        f_DeathdoorAttackSpeed = GetAttackSpeed() * 0.66f;
        i_DeathdoorAttackDamage = GetAttackDamage() * 3  /  5 ;
        
    }

    // Update is called once per frame
    protected void Update()
    {
        //RunFSM();
        Regenerate();
        RunDeathDoor();
        f_enmity = this.HP / this.i_maxHP;
        timeActive += Time.deltaTime;
    }

    public override void RunFSM()
    {
        
    }

    protected void RunDeathDoor()
    {
        switch (Ustate)
        {
            case UnitState.S_HEALTHY:
                {
                    if (this.HP == 0)
                    {
                        DeathDoorStats();
                        Ustate = UnitState.S_DEATHDOOR;
                    }
                    break;
                }
            case UnitState.S_DEATHDOOR:
                {
                    if (this.HP > (this.i_maxHP * 0.4))
                    {
                        ReturnStats();
                        Ustate = UnitState.S_HEALTHY;
                    }
                    else
                    {
                        if (DeathDoor())
                        {
                            Ustate = UnitState.S_DEAD;
                        }
                        else
                        {

                        }
                    }
                    break;
                }
        }

    }

    protected void Reload()
    {
        reloadDt -= Time.deltaTime;
        if (reloadDt <= 0)
        {
            currAmmo = maxAmmo;
            reloadDt = reloadRate;
        }
    }
    
    protected void Regenerate()
    {
        m_TimerDT += Time.deltaTime;


        if (m_TimerDT >= 2)
        {
            HP += (int)HPRegen;
            if (HP >= i_maxHP)
                HP = i_maxHP;

            m_TimerDT = 0;
        }
    }

    protected void ShoveEnemy(GameObject target)
    {
        var magnitude = 10;
        var force = transform.position - target.transform.position;
        force.Normalize();
        target.gameObject.transform.position -= (force * magnitude);
    }

    protected void DestroyGO()
    {
        Destroy(gameObject, 3);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<Projectile>().Sender.tag == "test" && col.gameObject.tag == "EnemyBullet")
        {
            TakeDamage(col.gameObject.GetComponent<Projectile>().Sender.GetComponent<RZombie>().GetAttackDamage());
            Destroy(col.gameObject);
        }
    }

    #region Targeting
    public GameObject SelectTarget(float Radius,Vector3 pos)
    {
        GameObject[] AllEnemies = GameObject.FindGameObjectsWithTag("test");
        List<GameObject> Targetables = new List<GameObject>();
        List<GameObject> TargetList = new List<GameObject>();
        if (AllEnemies != null)
        {
            foreach (GameObject go in AllEnemies)
            {
               // Debug.Log(Vector3.Distance(go.transform.position, pos));

                if (go == null)
                {
                    continue;
                }
                //Vector3.Angle(go.transform.position - pos, this.GetDirection().transform.position - pos) <= 45 &&

                else if (Vector3.Angle(go.transform.position - pos, this.GetDirection().transform.position - pos) <= 45 && Vector3.Distance(go.transform.position, pos) <= Radius && go.GetComponent<Zombie>().GetHealth() > 0)
                {
                    Targetables.Add(go);
                }
                else
                    continue;
            }

            if (Targetables.Count > 0)
            {
                foreach (GameObject go in Targetables)
                {
                        TargetList.Add(go);
                }
                int targetindex = Random.Range(0, TargetList.Count);

                return TargetList[targetindex];
            }

        }
        return null;
    }

    protected GameObject SwitchTarget(Vector3 position)
    {
        GameObject[] AllEnemies = GameObject.FindGameObjectsWithTag("test");
        List<GameObject> Withinrange = new List<GameObject>();
        foreach(GameObject go in AllEnemies)
        {
            if (go == null)
            {
                continue;
            }
            else if (Vector3.Distance(go.transform.position, this.transform.position) <= 3)
            {
                Withinrange.Add(go);
            }
            else
                continue;
        }
        if (Withinrange.Count != 0)
        {
            float range = 3f;
            GameObject temp = null;
            foreach(GameObject go in Withinrange)
            {
                if (range > (Vector3.Distance(go.transform.position, this.transform.position)))
                {
                    temp = go;
                }
            }
            return temp;
        }
        return null;
    }

    public bool Enemynear(float distance)
    {
        float closetdistance = 900;
        GameObject[] EnemyList = GameObject.FindGameObjectsWithTag("test");
        foreach(GameObject go in EnemyList)
        {
            float dist = Vector3.Distance(go.transform.position,this.gameObject.transform.position);
            if(dist < closetdistance)
            {
                closetdistance = dist;
            }
        }
        if(closetdistance < distance)
        {
            return true;
        }
        return false;
    }

    protected GameObject GetNearestTarget()
    {
        float closetdistance = 900;
        GameObject temp = null;
        GameObject[] EnemyList = GameObject.FindGameObjectsWithTag("test");
        if(EnemyList.Length != 0)
        {
            foreach (GameObject go in EnemyList)
            {
                float dist = Vector3.Distance(go.transform.position, this.gameObject.transform.position);
                if (dist < closetdistance)
                {
                    closetdistance = dist;
                    temp = go;
                }
            }
            return temp;
        }
        return null;

    }
    #endregion

    #region Death's Door Mechanic
    protected bool DeathDoor()
    {
        f_DeathDoortimer -= Time.deltaTime;
        if(f_DeathDoortimer <= 0)
        {
            int chancetodie = Random.Range(1, 400);
            chancetodie = chancetodie / 4;
            if(chancetodie < f_DeathChance)
            {
                return true;
            }
        }
        return false;
    }

    void AttackonDeathDoor(GameObject target)
    {
        float tempattack = target.GetComponent<Zombie>().GetAttackDamage();
        tempattack = tempattack * 0.25f ;
        f_DeathChance += tempattack;
        if(f_DeathChance > 100)
        {
            f_DeathChance = 100;
        }
    }

    protected void DeathDoorStats()
    {
        this.atkDmg = i_DeathdoorAttackDamage;
        this.atkSpd = f_DeathdoorAttackSpeed;
    }

    protected void ReturnStats()
    {
        this.atkDmg = i_OriginalAttackDamage;
        this.atkSpd = f_OriginalAttackSpeed;
    }
    #endregion

    #region Getters
    public float GetEnmity()
    {
        return f_enmity;
    }
    public override float GetAttackSpeed()
    {
        return atkSpd;
    }
    public override int GetAttackDamage()
    {
        return atkDmg;
    }
    public override int GetHealth()
    {
        return HP;
    }
    public override void SetHealth(int health)
    {
        HP = health;
    }
    public override float GetHealthRegen()
    {
        return HPRegen;
    }
    public override int GetMaxHealth()
    {
        return i_maxHP;
    }
    public override void SetMaxHealth(int maxhealth)
    {
        i_maxHP = maxhealth;
    }
    public void SetDirectionPoint(GameObject Direction)
    {
        Directionpoint = Direction;
    }
    public GameObject GetDirection()
    {
        return Directionpoint;
    }
    #endregion

}
