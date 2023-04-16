using CodeExplorinator;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class testFürAlgo : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/testFürAlgo")]
    public static void ShowExample()
    {
        testFürAlgo wnd = GetWindow<testFürAlgo>();
        wnd.titleContent = new GUIContent("testFürAlgo");
    }

    public void CreateGUI()
    {
        // Each editor window contains a target classGUI object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other classGUI following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

    }
}
