
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
        private string classDepthText = "Class Depth";
        private string methodDepthText = "Method Depth";
        private Vector2Int size;
        private Label classDepth;
        private Label methodDepth;
        private Slider classDepthSlider;
        private Slider methodDepthSlider;
        private TextField searchInput;
        private ScrollView scrollView;
        private Dictionary<string, ClassNode> classNodes;
        private Dictionary<string, SearchListEntry> searchListEntries;

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
            classDepth = new Label(classDepthText);
            VisualElement.Add(classDepth);
            VisualElement.Add(classDepthSlider.target);
            methodDepth = new Label(methodDepthText);
            VisualElement.Add(methodDepth);
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
                classNodes.Add(classnode.classGUI.ClassModifiers + " " + classnode.classGUI.ClassName, classnode);
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
            classDepth.text = classDepthText + ": " + depth;
        }

        public void SetMethodDepth(int depth)
        {
            graphManager.ChangeMethodDepth(depth);
            methodDepthSlider.SetValue(depth);
            methodDepth.text = methodDepthText + ": " + depth;
        }

        private void KeyDownHandler(KeyDownEvent context)
        {
            string query = searchInput.text;

            if (char.GetUnicodeCategory(context.character) != System.Globalization.UnicodeCategory.Control)
            {
                query = query.Insert(searchInput.cursorIndex, context.character.ToString());
            }

            if(context.keyCode == KeyCode.Backspace && query.Length > 0)
            {
                if (searchInput.textSelection.HasSelection())
                {
                    query = RemoveSelection(query);
                }
                else if(searchInput.cursorIndex > 0)
                {
                    query = query.Remove(searchInput.cursorIndex - 1, 1);
                }
            }

            query = query.ToLower();
            UpdateResultEntries(query);
            OrderEntriesByAlphabet();
        }

        private string RemoveSelection(string query)
        {
            int lowIndex = searchInput.selectIndex;
            int highIndex = searchInput.cursorIndex;

            if (highIndex < lowIndex)
            {
                int temp = highIndex;
                highIndex = lowIndex;
                lowIndex = temp;
            }

            int selectionLength = highIndex - lowIndex;

            return query.Remove(lowIndex, selectionLength);
        }

        private void UpdateResultEntries(string query)
        {

            foreach(string name in searchListEntries.Keys)
            {
                bool isVisible = DieserEineAlgoDessenNamenIchNichtWeiß(name.ToLower(), query);
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

        private bool DieserEineAlgoDessenNamenIchNichtWeiß(string text, string matcher)
        {
            if(matcher.Length == 0) { return true; }

            int index = 0;
            for(int i = 0; i < text.Length; i++)
            {
                if (text[i] == matcher[index])
                {
                    index++;
                }

                if(index == matcher.Length) { return true; }
            }

            return false;
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