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
    // Start is called before the first frame update
    void Start()
    {
        noteFromPlayer = "I was faster";
        player = Player.instance;
    }

    // Update is called once per frame
    void Update()
    {
        RegisterAction();
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

    public void AttackPlayer(Player player)
    {
        player.GetAttacked(attack);
    }

    public void AttackPlayerMagicly(Player player)
    {
        player.GetAttacked(magicAttack);
    }

    public void RegisterAction()
    {
        actionCounter++;
    }

    private void BreatheAir()
    {
        RegisterAction();
    }
}
