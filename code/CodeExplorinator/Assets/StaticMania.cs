using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticMania
{
    private static Player player;


    public static void ThisMethodIsProblematic()
    {
        player.GetAttacked(2);
    }

    public static int ThisMayBeNot()
    {
        player.GetAttacked(34); 
        return -1;
    }

    private static int PrivateStatic()
    {
        player.GetAttacked(34);
        return -1;
    }
}
