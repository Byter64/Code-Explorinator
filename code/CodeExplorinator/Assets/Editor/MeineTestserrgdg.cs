using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MeineTestserrgdg : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/MeineTestserrgdg")]
    public static void ShowExample()
    {
        MeineTestserrgdg wnd = GetWindow<MeineTestserrgdg>();
        wnd.titleContent = new GUIContent("MeineTestserrgdg");
    }

    public void CreateGUI()
    {
        // Each editor window contains a target classGUI object
        VisualElement root = rootVisualElement;

        root.Children();

        // VisualElements objects can contain other classGUI following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}
