using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class Mom
{
    public static Player son;
    public static Sister daughter;
    public smallMom SmallMom;

    public void GETCHOASSOVERHERE()
    {
        SmallMom = new smallMom();
        SmallMom.ParameterTest(bacon: 3, butter:4, eggs:5);
    }
    
}
