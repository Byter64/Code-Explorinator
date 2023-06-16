
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class MenuGUI : BaseGUI
    {
        private Dictionary<string, ClassNode> classNodes;
        private Dictionary<string, SearchListEntry> searchListEntries;
        private ScrollView scrollView;
        private Slider classDepthSlider;
        private Slider methodDepthSlider;
        private TextField searchInput;
        private Vector2Int size;

        public MenuGUI(GraphManager graphManager, Vector2Int size) : base(graphManager) 
        {
            this.size = size;

            classNodes = new Dictionary<string, ClassNode>();
            searchListEntries = new Dictionary<string, SearchListEntry>();
            UpdateDataBase();
        }

        public override void GenerateVisualElement()
        {
            VisualElement = new VisualElement();
            VisualElement.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.5f, 0.5f, 0.5f));
            VisualElement.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(size.x, size.y));
            VisualElement.style.width = size.x;
            VisualElement.style.height = size.y;
            VisualElement.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);

            #region Sliders

            classDepthSlider = new Slider(0, 10, 0, SetClassDepth);
            methodDepthSlider = new Slider(0, 10, 0, SetMethodDepth);
            RegisterControlKeyChecks(classDepthSlider.target);
            RegisterControlKeyChecks(methodDepthSlider.target);
            VisualElement.Add(new Label("Class Depth"));
            VisualElement.Add(classDepthSlider.target);
            VisualElement.Add(new Label("Method Depth"));
            VisualElement.Add(methodDepthSlider.target);

            #endregion


            Label searchtext = new Label("Search");
            searchtext.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            VisualElement.Add(searchtext);

            searchInput = new TextField();
            searchInput.RegisterCallback<KeyDownEvent>(KeyDownHandler);
            RegisterControlKeyChecks(searchInput);
            VisualElement.Add(searchInput);

            scrollView = new ScrollView();
            scrollView.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
            VisualElement.Add(scrollView);
            foreach(string className in classNodes.Keys)
            {
                SearchListEntry label = new SearchListEntry(className, this);
                label.style.marginBottom = 2;
                label.style.marginTop = 2;
                scrollView.Add(label);
                searchListEntries.Add(className, label);
            }

            OrderEntriesByAlphabet();
        }

        public override void SetVisible(bool isVisible)
        {
            VisualElement.visible = isVisible;
        }

        public void UpdateDataBase()
        {
            foreach(ClassNode classnode in graphManager.ClassNodes)
            {
                classNodes.Add(classnode.ClassData.GetName(), classnode);
            }
        }

        public void AddSelectedClass(string name)
        {
            graphManager.AddSelectedClass(classNodes[name]);
        }

        public void ApplySelectedClasses()
        {
            graphManager.ChangeToClassLayer();
            graphManager.AdjustGraphToSelectedClasses();
            foreach(SearchListEntry entry in searchListEntries.Values)
            {
                entry.SetUnselected();
            }
        }

        public void SetClassDepth(int depth)
        {
            graphManager.ChangeClassDepth(depth);
            classDepthSlider.SetValue(depth);
        }

        public void SetMethodDepth(int depth)
        {
            graphManager.ChangeMethodDepth(depth);
            methodDepthSlider.SetValue(depth);
        }

        private void KeyDownHandler(KeyDownEvent context)
        {
            string query = searchInput.text;

            if (char.GetUnicodeCategory(context.character) != System.Globalization.UnicodeCategory.Control)
            {
                query += context.character;
            }

            if(context.keyCode == KeyCode.Backspace && query.Length > 0)
            {
                query = query.Substring(0, query.Length - 1);
            }

            query = query.ToLower();
            UpdateResultEntries(query);
            OrderEntriesByAlphabet();
        }

        private void UpdateResultEntries(string query)
        {

            foreach(string name in searchListEntries.Keys)
            {
                bool isVisible = name.ToLower().Contains(query);
                if (!isVisible && scrollView.Contains(searchListEntries[name]))
                {
                    searchListEntries[name].parent.Remove(searchListEntries[name]);
                }
                if(isVisible && !scrollView.Contains(searchListEntries[name]))
                {
                    scrollView.Add(searchListEntries[name]);
                }
            }
        }
        private void OrderEntriesByAlphabet()
        {
            foreach(var entry in searchListEntries.OrderBy(x => x.Key.ToLower()))
            {
                entry.Value.BringToFront();
            }
        }

        private void RegisterControlKeyChecks(VisualElement target)
        {
            target.RegisterCallback<KeyDownEvent>((KeyDownEvent x) => { CodeExplorinatorGUI.SetControlKey(true); });
            target.RegisterCallback<KeyUpEvent>((KeyUpEvent x) => { CodeExplorinatorGUI.SetControlKey(false); });
        }
    }
}