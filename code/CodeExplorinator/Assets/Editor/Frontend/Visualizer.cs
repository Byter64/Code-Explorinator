using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UIElements;
using Microsoft.CodeAnalysis;

namespace CodeExplorinator
{
    public class Visualizer
    {
        public State state { get; private set; }

        public void Update(IInput input)
        {
            state = Update(state, input);
        }

        [Pure]
        private State Update(State currentState, IInput input)
        {
            State newState = currentState;
            switch(input)
            {
                case ReferenceDepth referenceDepth:
                    if(referenceDepth.referenceDepth == currentState.referenceDepth) { break; }

                    var graph = RequestNewGraph();
                    newState = newState with {  referenceDepth = referenceDepth.referenceDepth,
                                                graph = graph,
                                                nodeVisualElements = GenerateVisualElementsForNode(graph.Keys)};
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

        private ImmutableDictionary<INode, INode[]> RequestNewGraph()
        {
            throw new NotImplementedException();
        }

        private ImmutableDictionary<INode, VisualElement> GenerateVisualElementsForNode(IEnumerable<INode> nodes)
        {
            Dictionary<INode, VisualElement> visualElementNodes = new();

            foreach(INode node in nodes)
            {
                switch(node)
                {
                    case ClassNode classNode:
                        VisualElement visualElement = GenerateVisualElementForNode(classNode);
                        visualElementNodes.Add(classNode, visualElement);
                        break;

                    case PlaceholderNode placeholderNode:
                        visualElementNodes.Add(placeholderNode, null);
                        break;

                    case null:
                        throw new ArgumentNullException();

                    default:
                        throw new ArgumentException("Unknown class");
                }
            }

            return visualElementNodes.ToImmutableDictionary();
        }

        private VisualElement GenerateVisualElementForNode(ClassNode node)
        {
            VisualElement root = new Button();
            root.style.position = Position.Absolute;
            root.style.flexDirection = FlexDirection.Column;
            root.style.alignContent = Align.Stretch;

            //Header
            Label header = new Label();
            root.Add(header);
            header.text = node.typeData.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            //Fields, Properties and methods
            VisualElement attributes = new VisualElement();
            VisualElement methods = new VisualElement();
            root.Add(attributes);
            root.Add(methods);

            foreach(ISymbol member in node.typeData.GetMembers())
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

        private IEnumerable<(INode, INode)> GenerateConnections(IEnumerable<INode> nodes)
        {
            List<(INode, INode)> connections = new();

            foreach (INode node in nodes)
            {
                ClassNode classNode = node as ClassNode;
                if (classNode == null) { continue; }

                foreach (ISymbol member in classNode.typeData.GetMembers())
                {
                    INamedTypeSymbol typeSymbol = member as INamedTypeSymbol;
                    if(typeSymbol == null) { continue; }

                    //This second foreach on the same list could cause problems
                    foreach (INode node2 in nodes)
                    {
                        if(node2 == node)
                        {
                            connections.Add((node, node2));
                            continue;
                        }
                    }
                }
            }

            return connections.ToImmutableList();
        }

        private ImmutableDictionary<(INode, INode), VisualElement> GenerateVisualElementsForConnections(IEnumerable<(INode, INode)> connections)
        {
            Dictionary<(INode, INode), VisualElement> visualElementConnections = new();

            //Hier nächstes mal weitermachen

            return visualElementConnections.ToImmutableDictionary();
        }

        public record State(
            int referenceDepth,
            ImmutableDictionary<INode, INode[]> graph,
            ImmutableDictionary<INode, VisualElement> nodeVisualElements,
            ImmutableDictionary<(INode, INode), VisualElement> ConnectionVisualElements);

        public interface IInput { }

        public record ReferenceDepth(int referenceDepth) : IInput;

        public record FocusClassesAdded(PlaceholderNode[] addedFocusClasses) : IInput; 

        public record FocusClassesRemoved(PlaceholderNode[] removedFocusClasses) : IInput;
    }
}