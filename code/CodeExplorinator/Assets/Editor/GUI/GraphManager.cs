using Codice.Client.BaseCommands.CheckIn;
using Codice.Client.Common.TreeGrouper;
using Codice.CM.SEIDInfo;
using JetBrains.Annotations;
using Newtonsoft.Json;
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
    public class GraphManager
    {
        public struct SerializationData
        {
            public int shownClassDepth;
            public int shownMethodDepth;
            public State state;
            public string[] focusedClassNodes;
            public string[] focusedMethodNodes;

            public SerializationData(int shownClassDepth, int shownMethodDepth, State state, HashSet<ClassNode> focusedClassNodes, HashSet<MethodNode> focusedMethodNodes)
            {
                this.shownClassDepth = shownClassDepth;
                this.shownMethodDepth = shownMethodDepth;
                this.state = state;
                this.focusedClassNodes = ToStringArray(focusedClassNodes);
                this.focusedMethodNodes = ToStringArray(focusedMethodNodes);
            }

            private static string[] ToStringArray(HashSet<ClassNode> classNodes)
            {
                ClassNode[] classNodesArray = classNodes.ToArray();
                string[] result = new string[classNodes.Count];

                for(int i = 0; i < result.Length; i++)
                {
                    result[i] = classNodesArray[i].ClassData.ClassInformation.ToDisplayString();
                }
                return result;
            }

            private static string[] ToStringArray(HashSet<MethodNode> methodNode)
            {
                MethodNode[] methodNodesArray = methodNode.ToArray();
                string[] result = new string[methodNode.Count];

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = methodNodesArray[i].MethodData.MethodSymbol.ToDisplayString();
                }
                return result;
            }

            public static HashSet<ClassNode> ToClassNodes(string[] displayStrings)
            {
                HashSet<ClassNode> result = new();
                
                foreach(string instance in displayStrings)
                {
                    IEnumerable<ClassNode> nodes = Instance.ClassNodes.Where(x => x.ClassData.ClassInformation.ToDisplayString().Equals(instance));
                    if(nodes.Count() == 1)
                    {
                        result.Add(nodes.First());
                    }
                }

                return result;
            }

            public static HashSet<MethodNode> ToMethodNodes(string[] displayStrings)
            {
                HashSet<MethodNode> result = new();

                foreach (string instance in displayStrings)
                {
                    IEnumerable<MethodNode> nodes = Instance.methodNodes.Where(x => x.MethodData.MethodSymbol.ToDisplayString().Equals(instance));
                    if (nodes.Count() == 1)
                    {
                        result.Add(nodes.First());
                    }
                }

                return result;
            }
        }

        public enum State
        {
            ClassLayer,
            MethodLayer
        }

        public static GraphManager Instance { get; private set; }

        /// <summary>
        /// all ClassNodes in the currently analyzed project
        /// </summary>
        public HashSet<ClassNode> ClassNodes { get; private set; } = new();

        /// <summary>
        /// The maximum nodePair as edges from the focused nodes until which other nodes are still shown
        /// </summary>
        private int shownClassDepth;
        private int shownMethodDepth;
        private State state;
        private HashSet<ClassNode> focusedClassNodes = new();
        private HashSet<ClassNode> selectedClassNodes = new();
        private HashSet<MethodNode> focusedMethodNodes = new();
        private HashSet<MethodNode> selectedMethodNodes = new();
        private GraphVisualizer graphVisualizer;
        /// <summary>
        /// The currently focused methodNode in the oldFocusNode. This is null if the method layer is inactive
        /// </summary>
        public MethodNode focusedMethodNode; //TODO: Delete this 

        /// <summary>
        /// The root visualElement of the editor window
        /// </summary>
        private VisualElement graphRoot;

        /// <summary>
        /// all classes that have a method from shownMethodNodes
        /// </summary>
        private ClassGraph methodGraph;

        /// <summary>
        /// all methodNodes in the currently analyzed project
        /// </summary>
        private HashSet<MethodNode> methodNodes = new();

        /// <summary>
        /// all methodNodes within the maxDistance from focusedMethodNode
        /// </summary>
        private HashSet<MethodNode> shownMethodNodes = new();

        private List<ClassGraph> classGraphs = new();
        
        /// <summary>
        /// randomize via a seed to spawn the classes. TODO: make actually random
        /// </summary>
        private Random random = new Random(0987653); 

        public GraphManager(List<ClassData> data, VisualElement graphRoot, int shownDepth)
        {
            Instance = this;
            shownClassDepth = shownDepth;
            shownMethodDepth = shownDepth;
            this.graphRoot = graphRoot;
            graphVisualizer = new(graphRoot);
            //populate the lists with data
            foreach (ClassData @class in data)
            {
                ClassNodes.Add(GenerateNode(@class));
            }

            ClassNode.CopyRerefencesFromClassData(ClassNodes);
            foreach (ClassNode classNode in ClassNodes)
            {
                foreach (MethodGUI methodGUI in classNode.classGUI.methodGUIs)
                {
                    MethodNode node = GenerateNode(methodGUI.data, methodGUI);
                    node.MethodData.ContainingClass.ClassNode.MethodNodes.Add(node);
                    methodNodes.Add(node);
                }
            }


            MethodNode.CopyRerefencesFromMethodData(methodNodes);
            state = State.ClassLayer;
        }

        public void AddSelectedClass(ClassNode selectedClass)
        {
            selectedClassNodes.Add(selectedClass);
        }

        public void AddSelectedClasses(IEnumerable<ClassNode> selectedClasses)
        {
            foreach(ClassNode node in selectedClasses)
            {
                AddSelectedClass(node);
            }
        }

        public void AdjustGraphToSelectedClasses()
        {
            focusedClassNodes.Clear();
            focusedClassNodes.UnionWith(selectedClassNodes);
            ChangeClassGraphs(selectedClassNodes, shownClassDepth);
            selectedClassNodes.Clear();
        }

        public void AddSelectedMethod(MethodNode selectedMethod)
        {
            selectedMethodNodes.Add(selectedMethod);
        }

        public void AddSelectedMethods(IEnumerable<MethodNode> selectedMethods)
        {
            foreach (MethodNode node in selectedMethods)
            {
                AddSelectedMethod(node);
            }
        }

        public void AdjustGraphToSelectedMethods()
        {
            focusedMethodNodes.Clear();
            focusedMethodNodes.UnionWith(selectedMethodNodes);
            ChangeMethodGraph(selectedMethodNodes, shownMethodDepth);
            selectedMethodNodes.Clear();
        }

        /// <summary>
        /// Changes the maxDistance for claesses and adapts the UI to it
        /// </summary>
        /// <param name="depth"></param>
        public void ChangeClassDepth(int depth)
        {
            shownClassDepth = depth;
            if(state != State.ClassLayer) { return; }

            ChangeClassGraphs(focusedClassNodes, depth);
        }

        /// <summary>
        /// Changes the maxDistance for methods and adapts the UI to it
        /// </summary>
        /// <param name="depth"></param>
        public void ChangeMethodDepth(int depth)
        {
            shownMethodDepth = depth;
            if (state != State.MethodLayer) { return; }

            ChangeMethodGraph(focusedMethodNodes, depth);
        }

        public void ChangeToMethodLayer()
        {
            graphVisualizer.ShowClassLayer(false);
            graphVisualizer.ShowMethodLayer(true, GetAllMethodGUIs(shownMethodNodes));

            state = State.MethodLayer;
        }

        public void ChangeToClassLayer()
        {
            graphVisualizer.ShowMethodLayer(false);
            graphVisualizer.ShowClassLayer(true);

            state = State.ClassLayer;
        }

        public string Serialize(bool prettyPrint)
        {
            SerializationData data = new SerializationData(shownClassDepth, shownMethodDepth, state, focusedClassNodes, focusedMethodNodes);
            string result;
            if(prettyPrint)
            {
                result = JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            else
            {
                result = JsonConvert.SerializeObject(data, Formatting.None);
            }
            return result;
        }

        public SerializationData DeSerialize(string jsonString)
        {
            if(jsonString == null)
            {
                throw new ArgumentNullException();
            }
            SerializationData data = JsonConvert.DeserializeObject<SerializationData>(jsonString);
            HashSet<ClassNode> focusClasses = SerializationData.ToClassNodes(data.focusedClassNodes);
            HashSet<MethodNode> focusMethods = SerializationData.ToMethodNodes(data.focusedMethodNodes);
            
            AddSelectedClasses(focusClasses);
            AdjustGraphToSelectedClasses();

            AddSelectedMethods(focusMethods);
            AdjustGraphToSelectedMethods();

            switch(data.state)
            {
                case State.ClassLayer:
                    ChangeToClassLayer();
                    break;

                case State.MethodLayer:
                    ChangeToMethodLayer();
                    break;
            }

            return data;
        }

        private void ChangeClassGraphs(HashSet<ClassNode> focusClasses, int shownDepth)
        {
            UpdateSubGraphs(focusClasses, shownDepth);
            HashSet<ClassGUI> classGUIs = GetAllClassGUIs(classGraphs);
            HashSet<ConnectionGUI> connectionGUIs = GetAllConnectionGUIs(classGraphs);
            HashSet<ClassGUI> focusedGUIs = new();
            HashSet<ClassGUI> unfocusedGUIs = new();

            focusedGUIs.UnionWith(GetAllClassGUIs(focusClasses));
            unfocusedGUIs.UnionWith(classGUIs);
            unfocusedGUIs.ExceptWith(focusedGUIs);

            graphVisualizer.SetClassLayer(focusedGUIs, unfocusedGUIs, connectionGUIs);
            graphVisualizer.ShowClassLayer(true);
        }

        private void ChangeMethodGraph(HashSet<MethodNode> focusMethods, int shownDepth)
        {
            UpdateMethodGraph(focusMethods, shownDepth);
            HashSet<ClassGUI> classGUIs = GetAllClassGUIs(new HashSet<ClassGraph>() { methodGraph });
            HashSet<ConnectionGUI> connectionGUIs = GetAllConnectionGUIs(new HashSet<ClassGraph>() { methodGraph });
            HashSet<MethodGUI> methodGUIs = GetAllMethodGUIs(shownMethodNodes);
            HashSet<MethodGUI> focusedMethods = GetAllMethodGUIs(focusMethods);
            HashSet<MethodGUI> unfocusedMethods = new();
            unfocusedMethods.UnionWith(methodGUIs); 
            unfocusedMethods.ExceptWith(focusedMethods);
            graphVisualizer.SetMethodLayer(classGUIs, connectionGUIs, focusedMethods, unfocusedMethods);
            graphVisualizer.ShowMethodLayer(true, methodGUIs);
        } 

        private void UpdateSubGraphs(HashSet<ClassNode> focusClasses, int shownDepth)
        {
            classGraphs.Clear();
            classGraphs = GenerateOptimalSubgraphs(ClassNodes, focusClasses, shownDepth);
            HashSet<ClassNode> nodes = new();
            foreach(ClassGraph graph in classGraphs)
            {
                nodes.UnionWith(graph.classNodes);
            }

            SpringEmbedderAlgorithm.StartAlgorithm(nodes.ToList());

            foreach (ClassGraph graph in classGraphs)
            {
                nodes.UnionWith(graph.classNodes); //Warum nochmal????

                graph.GenerateConnectionsBetweenClasses();
                graph.GenerateVisualElementGraph();
            }
        }

        private void UpdateMethodGraph(HashSet<MethodNode> focusMethods, int shownDepth)
        {
            //foreach (MethodNode node in shownMethodNodes)
            //{
            //    node.MethodGUI.ShowHighlight(false);
            //}
            BreadthSearch.Reset();
            shownMethodNodes.Clear();
            //Generate graph
            foreach (MethodNode node in focusMethods)
            {
                shownMethodNodes.UnionWith(BreadthSearch.GenerateMethodSubgraph(node, shownDepth));
            }
            //Find the containing class for each focused class
            HashSet<ClassNode> focusedClasses = new();
            foreach (MethodNode node in focusMethods)
            {
                focusedClasses.Add(node.MethodData.ContainingClass.ClassNode);
            }

            //Create classgraph that contains each class which contains a shown method
            methodGraph = new ClassGraph(this, FindClassNodes(shownMethodNodes), focusedClasses, shownDepth);

            //Calculate new positions
            SpringEmbedderAlgorithm.StartMethodAlgorithm(methodGraph.classNodes, shownMethodNodes);
            methodGraph.GenerateConnectionsBetweenMethods(shownMethodNodes);
            methodGraph.GenerateVisualElementGraph();
        }

        /// <summary>
        /// Generates subgraph so that the following holds true: 
        /// 1. Every subgraph contains at least one focused node.
        /// 2. The distance of any node to its closest focused node is smaller than maxDistance
        /// 3. Every node is in exactly one subgraph
        /// 4. No subgraph has an outer node, which has an edge that leads to a node which is in another subgraph
        /// </summary>
        /// <param name="superGraph"></param>
        /// <param name="focusNodes"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        private List<ClassGraph> GenerateOptimalSubgraphs(HashSet<ClassNode> superGraph, HashSet<ClassNode> focusNodes, int maxDistance)
        {
            //Generate a subgraph for every focus node
            List<(ClassNode, HashSet<ClassNode>)> overlappingSubGraphs = new();
            foreach(ClassNode focusNode in focusNodes)
            {
                BreadthSearch.Reset();
                HashSet<ClassNode> subgraph = BreadthSearch.GenerateClassSubgraph(superGraph, focusNode, maxDistance);
                overlappingSubGraphs.Add((focusNode, subgraph));
            }

            //Combine overlapping subgraphs
            List<(HashSet<ClassNode>, HashSet<ClassNode>)> subgraphs = new();
            foreach((ClassNode, HashSet<ClassNode>) overlappingSubgraph in overlappingSubGraphs)
            {
                if(subgraphs.Count == 0) //first element
                {
                    var newGraph = (new HashSet<ClassNode> { overlappingSubgraph.Item1 }, overlappingSubgraph.Item2);
                    subgraphs.Add(newGraph);
                    continue;
                }

                //Look for a subgraph with which the overlappingSubgraph is overlapping
                (HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) overlappedSubGraph = (null, null);
                foreach ((HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) subgraph in subgraphs)
                {
                    foreach (ClassNode focusNode in subgraph.focusNodes)
                    {
                        int distance = BreadthSearch.CalculateDistance(superGraph, focusNode, overlappingSubgraph.Item1);
                        //If two focus nodes are closer equal 2 * maxDistance, their graphs need to overlap
                        //If two focus nodes distance is equal to 2 * maxDistance + 1, their graphs are adjacent. In this case we also want to merge them
                        if (distance <= 2 * maxDistance + 1) 
                        {
                            overlappedSubGraph = subgraph;
                            goto foundOverlap;
                        }
                    }
                }

                foundOverlap:
                if (overlappedSubGraph != (null, null))
                {
                    overlappedSubGraph.graph.UnionWith(overlappingSubgraph.Item2);
                    overlappedSubGraph.focusNodes.Add(overlappingSubgraph.Item1);
                }
                else
                {
                    var newGraph = (new HashSet<ClassNode> { overlappingSubgraph.Item1 }, overlappingSubgraph.Item2);
                    subgraphs.Add(newGraph); //Darf nicht im foreach über subgraphs passieren!!!
                }
            }

            //Create ClassGraph instances
            List<ClassGraph> graphs = new();
            foreach((HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) subgraph in subgraphs)
            {
                graphs.Add(new ClassGraph(this, subgraph.graph, subgraph.focusNodes, maxDistance));
            }

            return graphs;
        }

        /// <summary>
        /// Generates a node out of a given classdata
        /// </summary>
        /// <param name="classData"></param>
        /// <returns></returns>
        private ClassNode GenerateNode(ClassData classData)
        {
            GUIStyle classStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf"),
                fontSize = 20,
                richText = true
            };

            GUIStyle methodStyle = new GUIStyle(classStyle);
            methodStyle.alignment = TextAnchor.UpperLeft;

            
            
            //The ClassGUI should not be generated in here but should rather be given to this method as a parameter!!!!
            ClassGUI classGUI = new ClassGUI(
                new Vector2(random.Next(-500, 500) - graphRoot.style.marginLeft.value.value,
                    random.Next(-500, 500) - graphRoot.style.marginTop.value.value), classData, classStyle,
                methodStyle, methodStyle, this);
            ClassNode node = new ClassNode(classData, classGUI);
            classData.ClassNode = node;
            return node;
        }

        /// <summary>
        /// Generates a methodNode out of a given methodData
        /// </summary>
        /// <param name="methodData"></param>
        /// <returns></returns>
        private MethodNode GenerateNode(MethodData methodData, MethodGUI methodGUI)
        {
            GUIStyle methodStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf"),
                fontSize = 20,
                richText = true
            };
            methodStyle.alignment = TextAnchor.UpperLeft;

            MethodNode node = new MethodNode(methodData, methodGUI);
            //Debug.Log("Visualelement: " + node.methodGUI.style.marginLeft + "/" + node.methodGUI.style.marginTop);
            methodData.MethodNode = node;

            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodNodes"></param>
        /// <returns>All ClassNodes that are parents of at least one of the given MethodNodes</returns>
        private HashSet<ClassNode> FindClassNodes(HashSet<MethodNode> methodNodes)
        {
            HashSet<ClassNode> classNodes = new HashSet<ClassNode>();

            foreach (MethodNode methodNode in methodNodes)
            {
                classNodes.Add(methodNode.MethodData.ContainingClass.ClassNode);
            }

            return classNodes;
        }

        private HashSet<ClassGUI> GetAllClassGUIs(IEnumerable<ClassGraph> classGraphs)
        {
            HashSet<ClassGUI> classGUI = new HashSet<ClassGUI>();
            foreach (ClassGraph classGraph in classGraphs)
            {
                classGUI.UnionWith(GetAllClassGUIs(classGraph.classNodes));
            }
            return classGUI;
        }

        private HashSet<ClassGUI> GetAllClassGUIs(HashSet<ClassNode> classNodes)
        {
            HashSet<ClassGUI> classGUI = new HashSet<ClassGUI>();
            foreach (ClassNode classNode in classNodes)
            {
                classGUI.Add(classNode.classGUI);
            }
            return classGUI;
        }

        private HashSet<ConnectionGUI> GetAllConnectionGUIs(IEnumerable<ClassGraph> classGraphs)
        {
            HashSet<ConnectionGUI> connectionGUIs = new HashSet<ConnectionGUI>();
            foreach (ClassGraph classGraph in classGraphs)
            {
                foreach (ConnectionGUI connection in classGraph.connections)
                {
                    connectionGUIs.Add(connection);
                }
            }
            return connectionGUIs;
        }
        private HashSet<MethodGUI> GetAllMethodGUIs(IEnumerable<MethodNode> methodNodes)
        {
            HashSet<MethodGUI> methodGUIs = new HashSet<MethodGUI>();
            foreach (MethodNode node in methodNodes)
            {
                methodGUIs.Add(node.MethodGUI);
            }
            return methodGUIs;
        }
    }
}