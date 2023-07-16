
using Codice.Client.BaseCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class MenuGUI : BaseGUI
    {
        public static MenuGUI Instance { 
            get
            {
                return instance;
            }
        }

        private const string classDepthText = "Class Depth";
        private const string methodDepthText = "Method Depth";
        private static MenuGUI instance;

        private bool isCaseSensitive = false;
        private bool isClassExpanded = false;
        private bool useExactSearch = false;
        private string lastQuery;
        private Vector2Int size;
        private Label classDepth;
        private Label methodDepth;
        private Slider classDepthSlider;
        private Slider methodDepthSlider;
        private TextField searchInput;
        private ScrollView scrollView;
        private Button recompileButton;
        private Button collapseOrExpandAllClassesButton;
        private Button focusOnSelectedButton;
        private Button addSelectedClassesButton;
        private Toggle caseSensitiveToggle;
        private Toggle exactSearchToggle;
        private Dictionary<string, ClassNode> classNodes;
        private Dictionary<string, SearchListEntry> searchListEntries;
        private CodeExplorinatorGUI codeExplorinatorGUI;
        private HashSet<SearchListEntry> focusedEntries;
        public MenuGUI(GraphManager graphManager, Vector2Int size, CodeExplorinatorGUI codeExplorinatorGUI) : base(graphManager)
        {
            if(instance != null)
            {
                throw new Exception("A second MenuGUI was instantiated. Something went horribly wrong");
            }

            instance = this;

            this.size = size;
            this.codeExplorinatorGUI = codeExplorinatorGUI;
            classNodes = new Dictionary<string, ClassNode>();
            searchListEntries = new Dictionary<string, SearchListEntry>();
            focusedEntries = new HashSet<SearchListEntry>();
            UpdateDataBase();
        }

        public void Reinitialize()
        {
            //Update classNodes
            UpdateDataBase();

            //Update SearchlistEntries
            foreach(SearchListEntry entry in searchListEntries.Values)
            {
                if(scrollView.Contains(entry))
                {
                    scrollView.Remove(entry);
                }
            }
            searchListEntries.Clear();
            foreach (string className in classNodes.Keys)
            { 
                SearchListEntry label = new SearchListEntry(className, this);
                label.style.marginBottom = 2;
                label.style.marginTop = 2;
                scrollView.Add(label);
                searchListEntries.Add(className, label);
            }
            
            //Update FocusedEntries
            UpdateFocusedEntries();

            //Update search results
            UpdateResultEntries(lastQuery);
        }

        public override void GenerateVisualElement()
        {
            VisualElement = new VisualElement();
            VisualElement.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.5f, 0.5f, 0.5f));
            VisualElement.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(size.x, size.y));
            VisualElement.style.width = size.x;
            VisualElement.style.height = size.y;
            VisualElement.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);

            
            #region Buttons
            
            recompileButton = new Button();
            recompileButton.text = "Refresh Project";
            recompileButton.clickable.clicked += OnClickRecompileProject;
            VisualElement.Add(recompileButton);

            collapseOrExpandAllClassesButton = new Button();
            collapseOrExpandAllClassesButton.text = isClassExpanded ? "Collapse All" : "Expand All";
            collapseOrExpandAllClassesButton.clickable.clicked += OnCollapseOrExpandAll;
            VisualElement.Add(collapseOrExpandAllClassesButton);

            focusOnSelectedButton = new Button();
            focusOnSelectedButton.text = "Focus On Selected Classes";
            focusOnSelectedButton.clickable.clicked += OnFocusOnSelectedClasses;
            VisualElement.Add(focusOnSelectedButton);

            addSelectedClassesButton = new Button();
            addSelectedClassesButton.text = "Add Selected Classes";
            addSelectedClassesButton.clickable.clicked += OnAddSelectedClasses;
            VisualElement.Add(addSelectedClassesButton);

            #endregion


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

            
            #region Toggle

            caseSensitiveToggle = new Toggle();
            caseSensitiveToggle.text = "Case Sensitive Search";
            caseSensitiveToggle.value = false;
            caseSensitiveToggle.RegisterValueChangedCallback(OnToggleCaseSensitivity);
            VisualElement.Add(caseSensitiveToggle);

            exactSearchToggle = new Toggle();
            exactSearchToggle.text = "Exact Search";
            exactSearchToggle.value = false;
            exactSearchToggle.RegisterValueChangedCallback(OnToggleExactSearch);
            VisualElement.Add(exactSearchToggle);

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

        public void SetClassSelected(ClassNode node)
        {
            searchListEntries[GetClassNodeKey(node)].SetSelected();
        }

        private void OnClickRecompileProject()
        {
            codeExplorinatorGUI.Reinitialize();
            /*
            List<ClassData> classData = codeExplorinatorGUI.GenerateClassDataFromProject();
            graphManager.UpdateGraphManager(classData);
            
            UpdateMenuGUI();

            GenerateVisualElement();
            */
        }
        
        private void OnFocusOnSelectedClasses()
        {
            ApplySelectedClasses();
        }

        private void OnAddSelectedClasses()
        {
            graphManager.AddSelectedClasses(graphManager.FocusedClassNodes);
            ApplySelectedClasses();
        }

        private void OnToggleCaseSensitivity(ChangeEvent<bool> evt)
        {
            isCaseSensitive = evt.newValue;
            UpdateResultEntries(lastQuery);
        }

        private void OnToggleExactSearch(ChangeEvent<bool> evt)
        {
            useExactSearch = evt.newValue;
            UpdateResultEntries(lastQuery);
        }

        private void OnCollapseOrExpandAll()
        {
            if (isClassExpanded)
            {
                graphManager.graphVisualizer.ExpandAllClasses(false);
            }
            else
            {
                graphManager.graphVisualizer.ExpandAllClasses(true);
            }
            
            isClassExpanded = !isClassExpanded;
            collapseOrExpandAllClassesButton.text = isClassExpanded ? "Collapse All" : "Expand All";
        }

        public override void SetVisible(bool isVisible)
        {
            VisualElement.visible = isVisible;
        }

        public void UpdateDataBase()
        {
            classNodes.Clear();

            foreach(ClassNode classNode in graphManager.ClassNodes)
            {
                classNodes.Add(GetClassNodeKey(classNode), classNode);
            }
        }

        public void AddSelectedClass(string name)
        {
            graphManager.AddSelectedClass(classNodes[name]);
        }

        public void ApplySelectedClasses()
        {
            graphManager.ApplySelectedClasses();
        }

        public void UpdateFocusedEntries()
        {
            foreach(SearchListEntry entry in focusedEntries)
            {
                entry.SetUnselected();
            }

            focusedEntries.Clear();

            foreach (ClassNode node in graphManager.FocusedClassNodes)
            {
                string nodeKey = GetClassNodeKey(node);
                if (searchListEntries.ContainsKey(nodeKey))
                {
                    SearchListEntry entry = searchListEntries[nodeKey];
                    entry.SetFocused();
                    focusedEntries.Add(entry);
                }
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

            lastQuery = query;
            UpdateResultEntries(query);
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
            if(query == null) { query = string.Empty; }
             
            query = isCaseSensitive ? query : query.ToLower();
            Func<string, string, bool> matchFunction = useExactSearch ? DoesInputMatchExactly : DoesInputMatchJumpy;

            foreach (string entry in searchListEntries.Keys)
            {
                string text = isCaseSensitive ? entry : entry.ToLower();
                bool isVisible = matchFunction(text, query);
                if (!isVisible && scrollView.Contains(searchListEntries[entry]))
                {
                    searchListEntries[entry].parent.Remove(searchListEntries[entry]);
                }
                if(isVisible && !scrollView.Contains(searchListEntries[entry]))
                {
                    scrollView.Add(searchListEntries[entry]);
                }
            }

            OrderEntriesByAlphabet();
        }

        private bool DoesInputMatchJumpy(string text, string matcher)
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

        private bool DoesInputMatchExactly(string text, string matcher)
        {
            return text.Contains(matcher);
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

        private string GetClassNodeKey(ClassNode classNode)
        {
            return classNode.classGUI.ClassModifiers + " " + classNode.classGUI.ClassName;
        }
    }
}