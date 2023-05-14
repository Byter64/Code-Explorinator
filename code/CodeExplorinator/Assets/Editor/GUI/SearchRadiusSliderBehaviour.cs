using CodeExplorinator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class SearchRadiusSliderBehaviour : PointerManipulator
    {
        private int value = 2;
        private MenuGUI menu;
        private SliderInt slider;

        public SearchRadiusSliderBehaviour(SliderInt slider, MenuGUI menu, int startValue)
        {
            this.slider = slider;
            this.menu = menu;
            target = slider;
            value = startValue;
            slider.value = startValue;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        /// <summary>
        /// Slider behaves weirdly. 
        /// You get PointerMoveEvents only while the pointer is NOT pressed down.
        /// After a PointerDownEvent is fired, a PointerCaptureOutEvent is directly fired.
        /// After the pointer is released a POinterCaptureOutEvent is once more fired.
        /// A PointerUpEvent is never fired.
        /// </summary>
        /// <param name="context"></param>
        private void PointerCaptureOutHandler(PointerCaptureOutEvent context)
        {
            if (slider.value != value)
            {
                value = slider.value;
                menu.SetShownDepth(value);
            }

            if (target.HasPointerCapture(context.pointerId))
            {
                target.ReleasePointer(context.pointerId);
            }
        }
    }
}