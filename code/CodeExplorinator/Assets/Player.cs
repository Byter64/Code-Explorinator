using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{/*
    public static Player instance;
    public virtual string nickname { get;  set; }

    private int health;
    private int maxHealth;
    private int magicAttack;
    private int attack;
    string favouriteWord;
    Enemy archEnemy;
    Sister Sister;

    void Start()
    {
        archEnemy = FindObjectOfType<Enemy>();
        archEnemy.noteFromPlayer = "I don't know who you are or where you are. But you cannot use a bike there";
    }

    void Update()
    {
        archEnemy.RegisterAction();
    }

    public void GetAttacked(int attack)
    {
        archEnemy.noteFromPlayer = "Help my, I got attacked";
        health -= attack;
        if (IsDead())
        {
            Die();
        }
    }

    public virtual void Help()
    {
        
    }

    private bool IsDead()
    {
        archEnemy.noteFromPlayer = "I might be dead, but so should you be soon";
        return health <= 0;
    }

    protected void Die()
    {
        archEnemy.noteFromPlayer = "You shall never forget me";
        Destroy(gameObject);
    }
    */
}