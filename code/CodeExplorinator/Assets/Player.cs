using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public string nickname { get; private set; }

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
}