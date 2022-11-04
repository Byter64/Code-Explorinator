using CodeExplorinator;
using UnityEditor;
using UnityEngine;

public class HelloWorldBehaviour : MonoBehaviour
{
    void Start()
    {
        // Ensure that the generated code can be used
        Debug.Log(HelloWorld.Message);
        foreach (var className in HelloWorld.AllClassNames)
            Debug.Log("Found class name: " + className);

        // Ensure that the shared code can be used
        Debug.Log(SomeSharedClass.HelloWorld);
    }
}
