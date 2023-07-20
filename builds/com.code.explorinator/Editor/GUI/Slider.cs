using System;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class Slider : PointerManipulator
    {
        private int value = 2;
        private Action<int> onValueChange;
        private SliderInt slider;

        public Slider(int min, int max, int startValue, Action<int> onValueChange)
        {
            slider = new SliderInt(min, max);
            target = slider;

            value = startValue;
            slider.value = startValue;

            this.onValueChange = onValueChange;
        }

        public void SetValue(int value)
        {
            slider.value = value;
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
        /// After the pointer is released a PointerCaptureOutEvent is once more fired.
        /// A PointerUpEvent is never fired.
        /// </summary>
        /// <param name="context"></param>
        private void PointerCaptureOutHandler(PointerCaptureOutEvent context)
        {
            if (slider.value != value)
            {
                value = slider.value;
                onValueChange.Invoke(value);
            }

            if (target.HasPointerCapture(context.pointerId))
            {
                target.ReleasePointer(context.pointerId);
            }
        }
    }
}