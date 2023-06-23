using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sister
{
    private Dog gudBoi;

    public static implicit operator Mom(Sister sis)
    {
        return new Mom();
    }

    public static explicit operator smallMom(Sister sis)
    {
        return new smallMom();
    }

    public static Sister operator +(Sister sis1, Sister sis2)
    {
        return sis1;
    }

    public void gossip(Sister s1, Sister s2)
    {
        s1 += s2;
    }
}