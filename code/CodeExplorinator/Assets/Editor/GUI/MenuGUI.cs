
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
        private SearchRadiusSliderBehaviour slider;
        private TextField searchInput;
        private Vector2Int size;
        public MenuGUI(GraphManager graphManager, Vector2Int size) : base(graphManager) 
        {
            this.size = size;

            classNodes = new Dictionary<string, ClassNode>();
            searchListEntries = new Dictionary<string, SearchListEntry>();
            searchInput = new TextField();
            searchInput.RegisterCallback<KeyDownEvent>(KeyDownHandler);
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

            slider = CreateSearchRadiusSlider();
            VisualElement.Add(slider.target);

            Label searchtext = new Label("Search");
            searchtext.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            VisualElement.Add(searchtext);
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
            graphManager.FocusOnSelectedClasses();
            foreach(SearchListEntry entry in searchListEntries.Values)
            {
                entry.SetUnselected();
            }
        }

        public void SetShownDepth(int depth)
        {
            graphManager.ChangeDepth(depth);
        }

        private void KeyDownHandler(KeyDownEvent context)
        {
            if (context.keyCode != KeyCode.Return) { return; }

            string query = searchInput.text.ToLower();
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

        private SearchRadiusSliderBehaviour CreateSearchRadiusSlider()
        {
            SliderInt slider = new SliderInt(0, 10);
            SearchRadiusSliderBehaviour sliderBehaviour = new SearchRadiusSliderBehaviour(slider, this, 0);
            slider.style.marginBottom = 20;
            slider.style.marginTop = 20;

            return sliderBehaviour;
        }
    }
}