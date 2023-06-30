using CodeExplorinator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    
    int health;
    int maxHealth;

    int magicAttack;
    int attack;

    float hungriness;
    Enemy mother;
    Enemy father;

    public Sister lover;

    uint actionCounter;
    public string noteFromPlayer;
    private Player player;
    
    public abstract void Attack();
#if true
    
#endif

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
    /*

    private void BreatheねこÄÖÜßir()
    {
        RegisterAction();
    }

    private void DoABarrelRole(int slimeness)
    {

    }

    protected void iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii() { }
    protected void mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm() { }

    public static string ActivateRegginator(int level) { return ""; }

    public virtual void GlaubenSieDassIchVerrücktBin() { }

    //public async void Diese_Frage_stellte_mir_Elon_Musk_gegen_Ende_eines_langen_Abendessens_in_einem_edlen_Fischrestaurant_im_Silicon_Valley() { }

    private CodeExplorinatorGUI Your_Princess_In_Another_Castle() { return null; }

    private void WellExcuuuuuseMePrincess() {}

    private void ExcuseThePrincess(int gift, int giftButItIsGerman) { }
    */
    
}
