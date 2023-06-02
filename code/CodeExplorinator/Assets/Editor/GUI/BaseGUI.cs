using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public abstract class BaseGUI
    {
        public VisualElement VisualElement { get; protected set; }

        protected GraphVisualizer graphManager;

        protected BaseGUI(GraphVisualizer graphManager)
        {
            this.graphManager = graphManager;
        }

        public abstract void GenerateVisualElement();
    }
}