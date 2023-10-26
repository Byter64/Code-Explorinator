using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UIElements;
using Microsoft.CodeAnalysis;
using UnityEditor;
using UnityEditor.VersionControl;
using System.Linq;
using UnityEngine.UI;

namespace CodeExplorinator
{
    public class Visualizer : EditorWindow
    {
        private const string graphicsFolder = "Assets/Editor/Frontend/Graphics/";
        public State state { get; private set; }

        private VisualElement graphRoot = null;
        private VisualElement sceneRoot = null;

        [MenuItem("Code Explorinator/Code Explorinator")]
        public static void OnShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(Visualizer));
            editorWindow.titleContent = new GUIContent("Code Explorinator");
        }

        private void CreateGUI()
        {
            sceneRoot = rootVisualElement;

            UpdateVisualRepresentation();
        }

        public void MyUpdate(IInput input)
        {
            state = MyUpdate(state, input);
        }

        [Pure]
        private State MyUpdate(State currentState, IInput input)
        {
            State newState = currentState;
            switch(input)
            {
                case ReferenceDepth referenceDepth:
                    if(referenceDepth.referenceDepth == currentState.ReferenceDepth) { break; }
                    break;

                case null:
                    throw new ArgumentNullException();

                default:
                    throw new ArgumentException("argument does not exist");
            };

            throw new Exception("Not implemented yet");
            return newState;
        }

        private void UpdateVisualRepresentation()
        {
            if (graphRoot != null)
            {
                sceneRoot.Remove(graphRoot);
            }
            var graph = RequestNewGraph();
            graphRoot = CreateVisualRepresentation(graph);

            sceneRoot.Add(graphRoot);
        }

        [Pure]
        private VisualElement CreateVisualRepresentation(ImmutableDictionary<IClassData, ImmutableHashSet<IClassData>> graph)
        {
            VisualElement root = new VisualElement();
            root.name = "Graph Root";

            VisualElement nodeRoot = new VisualElement();
            nodeRoot.name = "Node Root";
            root.Add(nodeRoot);

            ImmutableDictionary<IClassData, VisualElement> nodes = CreateVisualElements(graph.Keys);
            int i = 0;
            float radius = 400;
            foreach (VisualElement node in nodes.Values)
            {
                nodeRoot.Add(node);
                node.name = "" + i;
                node.style.position = Position.Absolute;
                float radians = Mathf.PI * 2 * (i / (float)nodes.Values.Count());
                node.style.translate = new StyleTranslate(new Translate(radius * Mathf.Cos(radians) + radius, radius * Mathf.Sin(radians) + radius));
                i++;
            }
            
            AddAndCreateVisualConnections(graph, nodes, root);

            return root;
        }

        /// <summary>
        /// Because flex only generates data for the VisualElements after an Update, This method adds the connections after waiting one update
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="nodes"></param>
        /// <param name="graphRoot"></param>
        private void AddAndCreateVisualConnections
        (
            ImmutableDictionary<IClassData, ImmutableHashSet<IClassData>>  graph, 
            ImmutableDictionary<IClassData, VisualElement> nodes, 
            VisualElement graphRoot)
        {
            EditorApplication.update += AddVisualElements;

            void AddVisualElements()
            {
                VisualElement connectionRoot = new VisualElement();
                connectionRoot.name = "Connection Root";
                graphRoot.Add(connectionRoot);
                try
                {
                    var connections = CreateVisualElements();
                    foreach (var connectionArray in connections.Values)
                    {
                        foreach (VisualElement connection in connectionArray)
                        {
                            connectionRoot.Add(connection);
                        }
                    }

                }
                finally
                {
                    EditorApplication.update -= AddVisualElements;
                }
            }


            Dictionary<IClassData, VisualElement[]> CreateVisualElements()
            {
                Dictionary<IClassData, VisualElement[]> connections = new();

                foreach(IClassData class1 in graph.Keys)
                {
                    VisualElement node1 = nodes[class1];
                    VisualElement[] vs_connections = new VisualElement[graph[class1].Count];
                    int i = 0;
                    foreach (IClassData class2 in graph[class1])
                    {
                        VisualElement node2 = nodes[class2];
                        VisualElement connection = CreateVisualElement(node1, node2);
                        vs_connections[i] = connection;
                    }

                    connections.Add(class1, vs_connections);
                }

                return connections;
            }
        }

        [Pure]
        private ImmutableDictionary<IClassData, VisualElement> CreateVisualElements(IEnumerable<IClassData> classes)
        {
            Dictionary<IClassData, VisualElement> visualElementNodes = new();

            foreach (IClassData @class in classes)
            {
                VisualElement visualElement = CreateVisualElement(@class);
                visualElementNodes.Add(@class, visualElement);
            }

            return visualElementNodes.ToImmutableDictionary();
        }

        [Pure]
        private VisualElement CreateVisualElement(IClassData @class)
        {
            VisualElement root = new VisualElement();
            Sprite classBackground = AssetDatabase.LoadAssetAtPath<Sprite>(graphicsFolder + "Rectangle_Blue.png");
            root.style.position = Position.Absolute;
            root.style.flexDirection = FlexDirection.Column;
            root.style.alignContent = Align.Stretch;
            root.style.backgroundImage = new StyleBackground(classBackground);

            //Header
            Label header = new Label();
            root.Add(header);
            header.text = @class.typeData.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            //Fields, Properties and methods
            VisualElement attributes = new VisualElement();
            VisualElement methods = new VisualElement();
            root.Add(attributes);
            root.Add(methods);

            foreach(ISymbol member in @class.typeData.GetMembers())
            {
                switch(member)
                {
                    case IMethodSymbol methodSymbol:
                        methods.Add(new Label(methodSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                        break;

                    case IFieldSymbol fieldSymbol:
                        attributes.Add(new Label(fieldSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                        break;

                    case IPropertySymbol propertySymbol:
                        attributes.Add(new Label(propertySymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                        break;
                    case null:
                        throw new ArgumentNullException("A member was null");

                    default:
                        throw new NotImplementedException($"Unhandled type: {member.GetType()} of member {member.ToDisplayString()} is not supported");
                }
            }

            return root;
        }

        [Pure]
        private IEnumerable<(IClassData, IClassData)> CreateConnections(IEnumerable<IClassData> classes)
        {
            List<(IClassData, IClassData)> connections = new();

            foreach (IClassData class1 in classes)
            {
                foreach (ISymbol member in class1.typeData.GetMembers())
                {
                    ITypeSymbol namedTypeMember;
                    switch(member)
                    {
                        case IFieldSymbol fieldSymbol:
                            namedTypeMember = fieldSymbol.Type;
                            break;
                            case IPropertySymbol propSymbol:
                            namedTypeMember = propSymbol.Type;
                            break;
                        default:
                            continue;

                    }
                    
                    foreach (IClassData class2 in classes)
                    {
                        if(namedTypeMember == class2 && !connections.Contains((class1, class2)))
                        {
                            connections.Add((class1, class2));
                            break;
                        }
                    }
                }
            }

            return connections.ToImmutableList();
        }

        [Pure]
        private VisualElement CreateVisualElement(VisualElement class1, VisualElement class2)
        {
            Rect rect1 = new Rect(class1.resolvedStyle.translate,
                                  new Vector2(class1.resolvedStyle.width, class1.resolvedStyle.height));
            Rect rect2 = new Rect(class2.resolvedStyle.translate,
                                  new Vector2(class2.resolvedStyle.width, class2.resolvedStyle.height));
            var lineAttributes = CalculateShortestLine(rect1, rect2);

            VisualElement line = new VisualElement();
            VisualElement arrowhead = new VisualElement();
            Sprite lineBackground = AssetDatabase.LoadAssetAtPath<Sprite>(graphicsFolder + "ArrowBody.png");
            Sprite arrowHeadBackground = AssetDatabase.LoadAssetAtPath<Sprite>( graphicsFolder + "ArrowHead.png");
            line.style.position = Position.Absolute;
            line.style.backgroundImage = new StyleBackground(lineBackground);
            line.style.transformOrigin = new TransformOrigin(new Length(0, LengthUnit.Percent), new Length(50, LengthUnit.Percent));
            line.style.width = new StyleLength(StyleKeyword.Null);
            line.style.width = lineAttributes.length;
            line.style.height = lineBackground.texture.height;
            line.style.translate = new Translate(lineAttributes.origin.x, lineAttributes.origin.y);
            line.style.rotate = new Rotate(lineAttributes.rotation);

            line.Add(arrowhead);
            arrowhead.style.position = Position.Absolute;
            arrowhead.style.backgroundImage = new StyleBackground(arrowHeadBackground);
            arrowhead.style.width = arrowHeadBackground.texture.width;
            arrowhead.style.height = arrowHeadBackground.texture.height;
            arrowhead.style.translate = new Translate(line.style.width.value, 0);
            return line;

            (Vector2 origin , float rotation, float length) CalculateShortestLine(Rect rect1, Rect rect2)
            {
                Bounds bounds1 = new Bounds(rect1.position + rect1.size * 0.5f, rect1.size);
                Bounds bounds2 = new Bounds(rect2.position + rect2.size * 0.5f, rect2.size);

                Vector2 point1 = bounds1.ClosestPoint(bounds2.center);
                Vector2 point2 = bounds2.ClosestPoint(point1);
                Vector2 direction1To2 = point2 - point1;

                (Vector2 origin, float rotation, float length) line;
                line.origin = point1;
                line.rotation = Mathf.Atan2(direction1To2.y, direction1To2.x) * Mathf.Rad2Deg;
                line.length = direction1To2.magnitude;

                return line;
            }
        }

        private ImmutableDictionary<IClassData, ImmutableHashSet<IClassData>> RequestNewGraph()
        {
            ImmutableHashSet<ClassData> classes = BackendProgram.GetSetOfAllClasses();

            foreach(ClassData classData in classes)
            {
                Debug.Log(classData.typeData.ToDisplayString());
            }
            return BackendProgram.GenerateGraph(classes.Where(x => x.typeData.Name == "Visualizer").First(), 10);
        }

        [Pure]
        private ImmutableDictionary<IClassData, IClassData[]> GenerateTestGraph()
        {
            TestINamedTypeInterface baseType = new TestINamedTypeInterface("Base");
            TestINamedTypeInterface class1 = new TestINamedTypeInterface("Auto");
            TestINamedTypeInterface class2 = new TestINamedTypeInterface("Reifen");

            HashSet<IFieldSymbol> fields = new()
            {
                new TestIFieldSymbol("seriennummer", baseType),
                new TestIFieldSymbol("reifen", class2),
            };
            HashSet<IPropertySymbol> properties = new()
            {
                new TestIPropertySymbol("volumen", baseType),
                new TestIPropertySymbol("VerrückteMongo", baseType)
            };
            HashSet<IMethodSymbol> methods = new()
            {
                new TestIMethodSymbol("Explodieren", baseType),
                new TestIMethodSymbol("GetReifen", class2)
            };

            HashSet<IPropertySymbol> propertiesReifen = new()
            {
                new TestIPropertySymbol("form", baseType)
            };

            class1.fields.AddRange(fields);
            class1.properties.AddRange(properties);
            class1.methods.AddRange(methods);
            class2.properties.AddRange(propertiesReifen);


            HashSet<IClassData> classes =  new() { new ClassData(class1), new ClassData(class2), new ClassData(baseType) };
            Dictionary<IClassData, IClassData[]> graph = new();
            var connections = CreateConnections(classes);

            foreach (IClassData @class in classes)
            {
                IClassData[] references = new IClassData[0];

                graph.Add(@class, references);
            }

            foreach ((IClassData class1, IClassData class2) connection in connections)
            {
                IClassData[] references = graph[connection.class1];
                graph.Remove(connection.class1);

                Array.Resize(ref references, references.Length + 1);
                references[references.Length - 1] = connection.class2;
                graph.Add(connection.class1, references);
            }

            return graph.ToImmutableDictionary();

        }

        public record State(
            int ReferenceDepth,
            ImmutableDictionary<IClassData, IClassData[]> Graph,
            ImmutableDictionary<IClassData, VisualElement> NodeVisualElements,
            ImmutableDictionary<(IClassData, IClassData), VisualElement> ConnectionVisualElements);

        public interface IInput { }

        public record ReferenceDepth(int referenceDepth) : IInput;
    }
}