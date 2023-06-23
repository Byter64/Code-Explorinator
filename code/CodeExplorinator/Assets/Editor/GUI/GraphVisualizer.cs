﻿using Codice.Client.BaseCommands.CheckIn;
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
            foreach(ClassGUI classGUI in methodLayer)
            {
                classGUI.SetVisible(isVisible);
                classGUI.SetIsExpanded(true);
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
            foreach (ClassGUI classGUI in classLayerUnfocused)
            {
                classGUI.SetVisible(isVisible);
                classGUI.SetIsExpanded(false);
            }
            foreach(ClassGUI classGUI in classLayerFocused)
            {
                classGUI.SetVisible(isVisible);
                classGUI.SetIsExpanded(true);
            }

            foreach (ConnectionGUI connectionGUI in classLayerConnections)
            {
                connectionGUI.SetVisible(isVisible);
            }
        }

        public void SetMethodLayer(HashSet<ClassGUI> methodLayer, HashSet<ConnectionGUI> connections, HashSet<MethodGUI> focusedMethods, HashSet<MethodGUI> unfocusedMethods)
        {
            TryRemoveGUIsFromRoot(this.methodLayer, methodLayerRoot);
            TryRemoveGUIsFromRoot(this.methodLayerConnections, methodLayerRoot);

            foreach(MethodGUI methodGUI in focusedMethods)
            {
                methodGUI.SetFocused(true);
            }
            foreach(MethodGUI methodGUI in unfocusedMethods)
            {
                methodGUI.SetFocused(false);
            }

            this.methodLayer = methodLayer;
            this.methodLayerConnections = connections;
            this.methodLayerFocused = focusedMethods;
            this.methodLayerUnfocused = unfocusedMethods;

            TryAddGUIsToRoot(connections, methodLayerRoot);
            TryAddGUIsToRoot(methodLayer, methodLayerRoot);
        }

        public void SetClassLayer(HashSet<ClassGUI> focusedClasses, HashSet<ClassGUI> unfocusedClasses, HashSet<ConnectionGUI> connections)
        {
            TryRemoveGUIsFromRoot(this.classLayerUnfocused, classLayerRoot);
            TryRemoveGUIsFromRoot(this.classLayerConnections, classLayerRoot);
            TryRemoveGUIsFromRoot(this.classLayerFocused, classLayerRoot);

            foreach(ClassGUI classGUI in focusedClasses)
            {
                classGUI.SetFocused(true);
            }
            foreach(ClassGUI classGUI in unfocusedClasses)
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