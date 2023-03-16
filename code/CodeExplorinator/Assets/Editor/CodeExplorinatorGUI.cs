using UnityEditor;
using UnityEngine;
namespace CodeExplorinator
{
    public class CodeExplorinatorGUI : EditorWindow
    {
        private static Texture2D kirby;

        [MenuItem("Window/CodeExplorinator")]
        public static void OnShowWindow()
        {
            kirby = Resources.Load<Texture2D>("KSA_Kirby");
            
            GetWindow(typeof(CodeExplorinatorGUI));
        }

        private void OnGUI()
        {
            EditorGUI.DrawTextureTransparent(new Rect(10, 10, kirby.width, kirby.height), kirby);
            
        }
    }
}