using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adder : IAdder {
    int IAdder.Add(int a, int b) => a + b;
}
