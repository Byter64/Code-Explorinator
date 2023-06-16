using Codice.Client.BaseCommands.CheckIn;
using Codice.Client.Common.TreeGrouper;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

namespace CodeExplorinator
{
    public class GraphVisualizer
    {
        private HashSet<ClassGUI> methodLayer = new();
        private HashSet<ClassGUI> classLayer = new();
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
            foreach(ClassGUI classGUI in methodLayer)
            {
                classGUI.SetVisible(isVisible);
            }

            foreach(ConnectionGUI connectionGUI in methodLayerConnections)
            {
                connectionGUI.SetVisible(isVisible);
            }

            if(isVisible)
            {
                foreach(MethodGUI methodGUI in toBeHighlightedMethods)
                {
                    methodGUI.ShowHighlight(true);
                }
            }
        }

        public void ShowClassLayer(bool isVisible)
        {
            foreach (ClassGUI classGUI in classLayer)
            {
                classGUI.SetVisible(isVisible);
            }

            foreach (ConnectionGUI connectionGUI in classLayerConnections)
            {
                connectionGUI.SetVisible(isVisible);
            }
        }

        public void SetMethodLayer(HashSet<ClassGUI> methodLayer, HashSet<ConnectionGUI> connections)
        {
            TryRemoveGUIsFromRoot(this.methodLayer, methodLayerRoot);
            TryRemoveGUIsFromRoot(this.methodLayerConnections, methodLayerRoot);

            this.methodLayer = methodLayer;
            this.methodLayerConnections = connections;

            TryAddGUIsFromRoot(connections, methodLayerRoot);
            TryAddGUIsFromRoot(methodLayer, methodLayerRoot);
        }

        public void SetClassLayer(HashSet<ClassGUI> classLayer, HashSet<ConnectionGUI> connections)
        {
            TryRemoveGUIsFromRoot(this.classLayer, classLayerRoot);
            TryRemoveGUIsFromRoot(this.classLayerConnections, classLayerRoot);

            this.classLayer = classLayer;
            this.classLayerConnections = connections;

            TryAddGUIsFromRoot(connections, classLayerRoot);
            TryAddGUIsFromRoot(classLayer, classLayerRoot);
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

        private void TryAddGUIsFromRoot(HashSet<ClassGUI> guis, VisualElement root)
        {
            foreach (BaseGUI gui in guis)
            {
                if (!root.Contains(gui.VisualElement))
                {
                    root.Add(gui.VisualElement);
                }
            }
        }

        private void TryAddGUIsFromRoot(HashSet<ConnectionGUI> guis, VisualElement root)
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