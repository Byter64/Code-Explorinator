using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class ClickBehaviour : PointerManipulator
    {
        private enum State
        {
            Nothing,
            MonoClick,
            CheckingDoubleClick,
            DoubleClick,
            MonoClickAndControl,
            HoldingClick
        }
        private const double doubleClickThreshold = 0.3; //Maximum time between first and second PointerUp so that a double clilck is registered
        private const double clickThreshold = 0.3; //Maximum time between PointerDown and PointerUp events so that a click is registered

        private bool isFirstCallToHoldingHandler = true;
        private bool isLastCallToHoldingHandler = false;
        private double timeOfLastDownEvent;
        private double timeOfLastUpEvent;
        private Action onMonoclick;
        private Action onDoubleclick;
        private Action onControlMonoClick;
        private Action<bool, bool, float, float> onHoldingClick;
        private State state = State.Nothing;
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

        /// <summary>
        /// gets called every mouse position change while this object is "held" with mouse with the screenspace position of the mosue
        /// </summary>
        /// <param name="onHoldingClick"></param>
        public void RegisterOnHoldingClick(Action<bool, bool, float, float> onHoldingClick)
        {
            this.onHoldingClick = onHoldingClick;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerLeaveEvent>(PointerLeaveHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerLeaveEvent>(PointerLeaveHandler);
        }


        private void PointerDownHandler(PointerDownEvent context)
        {
            context.StopPropagation();
            //!!Warning!!
            //The way clicks are processed is prone to bugs because they are not handled globally. Example:
            //click on one class, quickly double click on another one.
            //Then the doubleclick handler will be executed first
            //And then the monoclick handler.

            switch (state)
            {
                case State.Nothing:
                    state = State.HoldingClick;
                    break;
                case State.MonoClick:
                    state = State.CheckingDoubleClick;
                    break;
            }

            timeOfLastDownEvent = EditorApplication.timeSinceStartup;
        }

        private void PointerUpHandler(PointerUpEvent context)
        {
            switch (state)
            {
                case State.HoldingClick:
                    if(EditorApplication.timeSinceStartup - timeOfLastDownEvent < clickThreshold)
                    {
                        state = State.MonoClick;
                        EditorApplication.update += Update;
                    }
                    else
                    {
                        ExecuteClick();
                    }
                    break;
                case State.CheckingDoubleClick:
                    if(EditorApplication.timeSinceStartup - timeOfLastUpEvent < doubleClickThreshold)
                    {
                        state = State.DoubleClick;
                        EditorApplication.update -= Update;
                        ExecuteClick();
                    }
                    break;
            }
            timeOfLastUpEvent = EditorApplication.timeSinceStartup;
        }

        private void PointerMoveHandler(PointerMoveEvent context)
        {
            if (state == State.HoldingClick)
            {
                HoldingClickHandler(isFirstCallToHoldingHandler, isLastCallToHoldingHandler, context.position.x, context.position.y);
                isFirstCallToHoldingHandler = false;
            }
        }

        private void PointerLeaveHandler(PointerLeaveEvent context)
        {
            EditorApplication.update -= Update;

            state = State.Nothing;
            ExecuteClick();
        }

        private void ExecuteClick()
        {
            if(state == State.MonoClick && CodeExplorinatorGUI.isControlDown)
            {
                state = State.MonoClickAndControl;
            }

            switch(state)
            {
                case State.HoldingClick:
                    isLastCallToHoldingHandler = true;
                    if (!isFirstCallToHoldingHandler)
                    {
                        HoldingClickHandler(isFirstCallToHoldingHandler, isLastCallToHoldingHandler, float.NaN, float.NaN);
                    }
                    break;
                case State.MonoClick:
                    MonoClickHandler();
                    break;
                case State.DoubleClick:
                    DoubleClickHandler();
                    break;
                case State.MonoClickAndControl:
                    ControlMonoClickHandler();
                    break;

                default: break;
            }

            state = State.Nothing;
            isFirstCallToHoldingHandler = true;
            isLastCallToHoldingHandler = false;
        }

        private void Update()
        {
            switch(state)
            {
                case State.MonoClick:
                    if(EditorApplication.timeSinceStartup - timeOfLastDownEvent >= doubleClickThreshold)
                    {
                        EditorApplication.update -= Update;
                        ExecuteClick();
                    }
                    break;
            }
        }

        private void HoldingClickHandler(bool isFirstCall, bool isLastCall, float x, float y)
        {
            onHoldingClick?.Invoke(isFirstCall, isLastCall, x, y);
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
            onDoubleclick?.Invoke();
        }
    }
}