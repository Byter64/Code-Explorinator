using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using UnityEngine;

public class TestClass : MonoBehaviour
{
    public int MyTestField;
    
    public int MyTestProperty { get; }

    public List<string> MyTestList;
    
    public void MyTestMethod()
    {
        Debug.Log("fsefsefsesgeg");
    }

    public static void AnalyzeConnectionsOfClass(INamedTypeSymbol classSymbol)
    {
    }
}
