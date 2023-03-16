using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    int health;
    int maxHealth;

    int magicAttack;
    int attack;

    float hungriness;
    Enemy mother;
    Enemy father;

    uint actionCounter;
    public string noteFromPlayer;
    private Player player;
 
    void Start()
    {
        noteFromPlayer = "I was faster";
        player = Player.instance;
    }
    void Update()
    {
        RegisterAction();
    }
    public void AttackPlayer(Player player)
    {
        player.GetAttacked(attack);
        string bob = player.nickname;
    }
    public void AttackPlayerMagicly(Player player)
    {
        player.GetAttacked(magicAttack);
    }
    public void RegisterAction()
    {
        actionCounter++;
    }
    
    private static Enemy Reproduce(Enemy mother, Enemy father)
    {
        mother.RegisterAction();
        father.RegisterAction();
        Enemy child = Instantiate(mother);
        child.mother = mother;
        child.father = father;

        return child;
    }

    private void BreatheAir()
    {
        RegisterAction();
    }
}
