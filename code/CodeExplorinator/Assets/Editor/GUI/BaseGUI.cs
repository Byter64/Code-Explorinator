using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public abstract class BaseGUI
    {
        public VisualElement VisualElement { get; protected set; }

        protected GraphManager graphManager;

        protected BaseGUI(GraphManager graphManager)
        {
            this.graphManager = graphManager;
        }

        public abstract void GenerateVisualElement();
    }
}