using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.Windows;

namespace CodeExplorinator
{
    public class Visualizer
    {
        public State state { get; private set; }

        public void Update(Input input)
        {
            state = Update(state, input);

            object foo = null;
            var fooString = foo switch
            {
                int i => i.ToString(),
                char c => c.ToString()
            };
        }

        [Pure]
        private State Update(State currentState, Input input)
        {

        }

        public record State
        {

        }

        public record Input
        {

        }

        public abstract record Node
        {
            public object classData;
            public Node[] ingoings;
            public Node[] outgoings;
        }

        public abstract record ClassNode : Node
        {

        }

        public abstract record PlaceholderNode : Node
        {
            
        }
    }
}