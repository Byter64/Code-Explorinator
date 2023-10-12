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

namespace CodeExplorinator
{
    public class Visualizer : EditorWindow
    {
        private const string graphicsFolder = "Asset/Editor/Frontend/Graphics/";
        public State state { get; private set; }

        [MenuItem("Code Explorinator")]
        public static void OnShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(Visualizer));
            editorWindow.titleContent = new GUIContent("Code Explorinator");
        }

        private void CreateGUI()
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

            class1.fields.Concat(fields);
            class1.properties.Concat(properties);
            class1.methods.Concat(methods);
            class2.properties.Concat(propertiesReifen);

            HashSet<INamedTypeSymbol> classes = new() { class1, class2, baseType };
            var vsNodes = CreateVisualElementsForNodes(classes);
            var vsConnections = CreateVisualElementsForConnections(CreateConnections(classes), vsNodes);

            //Connect all visual elements to the root and see if they are shown as intended
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

                    var graph = RequestNewGraph();
                    var connections = CreateConnections(graph.Keys);
                    var nodeVisualElements = CreateVisualElementsForNodes(graph.Keys);
                    newState = newState with {  ReferenceDepth = referenceDepth.referenceDepth,
                                                Graph = graph,
                                                NodeVisualElements = nodeVisualElements,
                                                ConnectionVisualElements = CreateVisualElementsForConnections(connections, nodeVisualElements)};
                    break;

                case FocusClassesAdded focusClassesAdded:

                    break;

                case FocusClassesRemoved focusClassesRemoved:

                    break;

                case null:
                    throw new ArgumentNullException();

                default:
                    throw new ArgumentException("argument does not exist");
            };

            throw new Exception("Not implemented yet");
            return newState;
        }

        private ImmutableDictionary<INamedTypeSymbol, INamedTypeSymbol[]> RequestNewGraph()
        {
            throw new NotImplementedException();
        }

        [Pure]
        private ImmutableDictionary<INamedTypeSymbol, VisualElement> CreateVisualElementsForNodes(IEnumerable<INamedTypeSymbol> classes)
        {
            Dictionary<INamedTypeSymbol, VisualElement> visualElementNodes = new();

            foreach (INamedTypeSymbol @class in classes)
            {
                VisualElement visualElement = CreateVisualElementForNode(@class);
                visualElementNodes.Add(@class, visualElement);
            }

            return visualElementNodes.ToImmutableDictionary();
        }

        [Pure]
        private VisualElement CreateVisualElementForNode(INamedTypeSymbol @class)
        {
            VisualElement root = new Button();
            Sprite classBackground = AssetDatabase.LoadAssetAtPath<Sprite>(graphicsFolder + "Rectangle_Blue");
            root.style.position = Position.Absolute;
            root.style.flexDirection = FlexDirection.Column;
            root.style.alignContent = Align.Stretch;
            root.style.backgroundImage = new StyleBackground(classBackground);

            //Header
            Label header = new Label();
            root.Add(header);
            header.text = @class.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            //Fields, Properties and methods
            VisualElement attributes = new VisualElement();
            VisualElement methods = new VisualElement();
            root.Add(attributes);
            root.Add(methods);

            foreach(ISymbol member in @class.GetMembers())
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
                        throw new NotImplementedException("This type of ISymbol was not considered");
                }
            }

            return root;
        }

        [Pure]
        private IEnumerable<(INamedTypeSymbol, INamedTypeSymbol)> CreateConnections(IEnumerable<INamedTypeSymbol> classes)
        {
            List<(INamedTypeSymbol, INamedTypeSymbol)> connections = new();

            foreach (INamedTypeSymbol class1 in classes)
            {
                foreach (ISymbol member in class1.GetMembers())
                {
                    INamedTypeSymbol namedTypeMember = member as INamedTypeSymbol;
                    if(namedTypeMember == null) { continue; }

                    //This second foreach on the same list could cause problems
                    foreach (INamedTypeSymbol class2 in classes)
                    {
                        if(namedTypeMember == class2)
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
        private ImmutableDictionary<(INamedTypeSymbol, INamedTypeSymbol), VisualElement> CreateVisualElementsForConnections
            (IEnumerable<(INamedTypeSymbol, INamedTypeSymbol)> connections, 
            IImmutableDictionary<INamedTypeSymbol, VisualElement> nodeVisualElements)
        {
            Dictionary<(INamedTypeSymbol, INamedTypeSymbol), VisualElement> visualElementConnections = new();

            foreach (var connection in connections)
            {
                VisualElement visualElementConnection = CreateVisualElementForConnection(nodeVisualElements[connection.Item1], nodeVisualElements[connection.Item2]);
                visualElementConnections.Add(connection, visualElementConnection);
            }

            return visualElementConnections.ToImmutableDictionary();
        }

        [Pure]
        private VisualElement CreateVisualElementForConnection(VisualElement class1, VisualElement class2)
        {
            VisualElement line = new VisualElement();
            VisualElement arrowhead = new VisualElement();
            Sprite lineBackground = AssetDatabase.LoadAssetAtPath<Sprite>(graphicsFolder + "ArrowBody.png");
            Sprite arrowHeadBackground = AssetDatabase.LoadAssetAtPath<Sprite>( graphicsFolder + "ArrowHead.png");
            line.style.position = Position.Absolute;
            line.style.backgroundImage = new StyleBackground(lineBackground);
            line.style.transformOrigin = new TransformOrigin(new Length(0, LengthUnit.Percent), new Length(50, LengthUnit.Percent));
            line.style.width = class2.style.marginLeft.value.value - class1.style.marginLeft.value.value;
            line.style.height = lineBackground.bounds.size.y;

            line.Add(arrowhead);
            arrowhead.style.position = Position.Absolute;
            line.style.backgroundImage = new StyleBackground(arrowHeadBackground);
            line.style.width = arrowHeadBackground.bounds.size.x;
            line.style.height = arrowHeadBackground.bounds.size.y;
            arrowhead.style.translate = new Translate(new Length(100, LengthUnit.Percent), 0);
            return line;
        }

        public record State(
            int ReferenceDepth,
            ImmutableDictionary<INamedTypeSymbol, INamedTypeSymbol[]> Graph,
            ImmutableDictionary<INamedTypeSymbol, VisualElement> NodeVisualElements,
            ImmutableDictionary<(INamedTypeSymbol, INamedTypeSymbol), VisualElement> ConnectionVisualElements);

        public interface IInput { }

        public record ReferenceDepth(int referenceDepth) : IInput;

        public record FocusClassesAdded(PlaceholderNode[] addedFocusClasses) : IInput; 

        public record FocusClassesRemoved(PlaceholderNode[] removedFocusClasses) : IInput;
    }
}