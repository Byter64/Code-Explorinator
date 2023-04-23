using System;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class ClickBehaviour : PointerManipulator
    {
        private readonly double doubleClickThreshold = 0.3;

        private double timeOfLastClick;
        private Action onMonoclick;
        private Action onDoubleclick;

        public ClickBehaviour(VisualElement target, Action onMonoclick, Action onDoubleclick)
        {
            this.target = target;
            this.onMonoclick = onMonoclick;
            this.onDoubleclick = onDoubleclick;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        }


        private void PointerDownHandler(PointerDownEvent context)
        {
            context.StopPropagation();
            //!!Warning!!
            //The way clicks are processed is prone to bugs because they are not handled globally. Example:
            //click on one class, quickly double click on another one.
            //Then the doubleclick handler will be executed first
            //And then the monoclick handler.
            if (EditorApplication.timeSinceStartup - timeOfLastClick <= doubleClickThreshold)
            {
                DoubleClickHandler();
            }
            else
            {
                EditorApplication.update += TryMonoClick;
            }

            timeOfLastClick = EditorApplication.timeSinceStartup;
        }

        private void PointerUpHandler(PointerUpEvent context)
        {
            
        }

        private void TryMonoClick()
        {
            if (EditorApplication.timeSinceStartup - timeOfLastClick > doubleClickThreshold)
            {
                EditorApplication.update -= TryMonoClick;
                MonoClickHandler();
            }
        }

        private void MonoClickHandler()
        {
            onMonoclick?.Invoke();
        }

        private void DoubleClickHandler()
        {
            EditorApplication.update -= TryMonoClick;
            onDoubleclick?.Invoke();
        }
    }
}