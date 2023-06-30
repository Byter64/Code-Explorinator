using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : Enemy
{

    public Sister owner;
    //public HashSet<Bunker> Enemies;

    public List<Mom> Moms;
    

    public smallMom[] MomArray;
    
    public HashSet<Bunker> ProEnemies { get; set; }

    public List<Mom> ProMoms { get; set; }

    public smallMom[] ProMomArray { get; set; }
    
    
    public override void Attack()
    {
        
    }

    public void Bark()
    {
        owner.GoForAWalk();
    }
    
    
    
}
