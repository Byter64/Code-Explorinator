﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class GraphVisualizer
    {
        private HashSet<ClassGUI> methodLayer = new();
        private HashSet<ClassGUI> classLayerUnfocused = new();
        private HashSet<ClassGUI> classLayerFocused = new();
        private HashSet<MethodGUI> methodLayerUnfocused = new();
        private HashSet<MethodGUI> methodLayerFocused = new();
        private HashSet<ConnectionGUI> methodLayerConnections = new();
        private HashSet<ConnectionGUI> classLayerConnections = new();
        private VisualElement methodLayerRoot = new();
        private VisualElement classLayerRoot = new();

        public GraphVisualizer(VisualElement sceneRoot)
        {
            sceneRoot.Add(methodLayerRoot);
            sceneRoot.Add(classLayerRoot);
        }

        public void ShowMethodLayer(bool isVisible, HashSet<MethodGUI> toBeHighlightedMethods = null)
        {
            foreach (ClassGUI classGUI in methodLayer)
            {
                classGUI.SetVisible(isVisible);
                if (isVisible) classGUI.SetIsExpanded(true);
            }

            foreach (MethodGUI methodGUI in methodLayerFocused)
            {
                methodGUI.SetHighlight(isVisible);
            }
            foreach (MethodGUI methodGUI in methodLayerUnfocused)
            {
                methodGUI.SetHighlight(isVisible);
            }

            foreach (ConnectionGUI connectionGUI in methodLayerConnections)
            {
                connectionGUI.SetVisible(isVisible);
            }
        }

        public void ShowClassLayer(bool isVisible)
        {
            foreach (ClassGUI classGUI in classLayerUnfocused)
            {
                classGUI.SetVisible(isVisible);
                classGUI.SetIsExpanded(false);
            }
            foreach (ClassGUI classGUI in classLayerFocused)
            {
                classGUI.SetVisible(isVisible);
                classGUI.SetIsExpanded(true);
            }

            foreach (ConnectionGUI connectionGUI in classLayerConnections)
            {
                connectionGUI.SetVisible(isVisible);
            }
        }

        public void ExpandAllClasses(bool isExpanded)
        {
            foreach (ClassGUI classGUI in methodLayer.Concat(classLayerUnfocused.Concat(classLayerFocused)))
            {
                classGUI.SetIsExpanded(isExpanded);
            }
        }

        public void SetMethodLayer(HashSet<ClassGUI> methodLayer, HashSet<ConnectionGUI> connections, HashSet<MethodGUI> focusedMethods, HashSet<MethodGUI> unfocusedMethods)
        {
            foreach (MethodGUI methodGUI in methodLayerFocused)
            {
                methodGUI.SetHighlight(false);
            }
            foreach (MethodGUI methodGUI in methodLayerUnfocused)
            {
                methodGUI.SetHighlight(false);
            }

            TryRemoveGUIsFromRoot(this.methodLayer, methodLayerRoot);
            TryRemoveGUIsFromRoot(this.methodLayerConnections, methodLayerRoot);


            this.methodLayer = methodLayer;
            this.methodLayerConnections = connections;
            this.methodLayerFocused = focusedMethods;
            this.methodLayerUnfocused = unfocusedMethods;

            foreach (MethodGUI methodGUI in focusedMethods)
            {
                methodGUI.SetFocused(true, methodGUI.data.MethodNode.distanceFromFocusMethod);
            }
            foreach (MethodGUI methodGUI in unfocusedMethods)
            {
                methodGUI.SetFocused(false, methodGUI.data.MethodNode.distanceFromFocusMethod);
            }
            TryAddGUIsToRoot(connections, methodLayerRoot);
            TryAddGUIsToRoot(methodLayer, methodLayerRoot);
        }

        public void SetClassLayer(HashSet<ClassGUI> focusedClasses, HashSet<ClassGUI> unfocusedClasses, HashSet<ConnectionGUI> connections)
        {
            TryRemoveGUIsFromRoot(this.classLayerUnfocused, classLayerRoot);
            TryRemoveGUIsFromRoot(this.classLayerConnections, classLayerRoot);
            TryRemoveGUIsFromRoot(this.classLayerFocused, classLayerRoot);

            foreach (ClassGUI classGUI in focusedClasses)
            {
                classGUI.SetFocused(true);
            }
            foreach (ClassGUI classGUI in unfocusedClasses)
            {
                classGUI.SetFocused(false);
            }

            this.classLayerUnfocused = unfocusedClasses;
            this.classLayerFocused = focusedClasses;
            this.classLayerConnections = connections;

            TryAddGUIsToRoot(connections, classLayerRoot);
            TryAddGUIsToRoot(unfocusedClasses, classLayerRoot);
            TryAddGUIsToRoot(focusedClasses, classLayerRoot);
        }

        private void TryRemoveGUIsFromRoot(HashSet<ClassGUI> guis, VisualElement root)
        {
            foreach (BaseGUI gui in guis)
            {
                if (root.Contains(gui.VisualElement))
                {
                    root.Remove(gui.VisualElement);
                }
            }
        }

        private void TryRemoveGUIsFromRoot(HashSet<ConnectionGUI> guis, VisualElement root)
        {
            foreach (BaseGUI gui in guis)
            {
                if (root.Contains(gui.VisualElement))
                {
                    root.Remove(gui.VisualElement);
                }
            }
        }

        private void TryAddGUIsToRoot(HashSet<ClassGUI> guis, VisualElement root)
        {
            foreach (BaseGUI gui in guis)
            {
                if (!root.Contains(gui.VisualElement))
                {
                    root.Add(gui.VisualElement);
                }
            }
        }

        private void TryAddGUIsToRoot(HashSet<ConnectionGUI> guis, VisualElement root)
        {
            foreach (BaseGUI gui in guis)
            {
                if (!root.Contains(gui.VisualElement))
                {
                    root.Add(gui.VisualElement);
                }
            }
        }
    }
}