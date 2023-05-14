using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class ClickBehaviour : PointerManipulator
    {

        private readonly double doubleClickThreshold = 0.3;

        private double timeOfLastClick;
        private Action onMonoclick;
        private Action onDoubleclick;
        private Action onControlMonoClick;

        public ClickBehaviour(VisualElement target, Action onMonoclick, Action onDoubleclick = null)
        {
            this.target = target;
            this.onMonoclick = onMonoclick;
            this.onDoubleclick = onDoubleclick;
        }

        public void RegisterOnControlMonoClick(Action onControlMonoClick)
        {
            this.onControlMonoClick = onControlMonoClick;
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
                if (CodeExplorinatorGUI.isControlDown)
                {
                    EditorApplication.update += TryControlMonoClick;
                }
                else
                {
                    EditorApplication.update += TryMonoClick;
                }
                
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

        private void TryControlMonoClick()
        {
            if (EditorApplication.timeSinceStartup - timeOfLastClick > doubleClickThreshold)
            {
                EditorApplication.update -= TryControlMonoClick;
                ControlMonoClickHandler();
            }
        }

        private void MonoClickHandler()
        {
            onMonoclick?.Invoke();
        }

        private void ControlMonoClickHandler()
        {
            onControlMonoClick?.Invoke();
        }

        private void DoubleClickHandler()
        {
            EditorApplication.update -= TryMonoClick;
            onDoubleclick?.Invoke();
        }
    }
}