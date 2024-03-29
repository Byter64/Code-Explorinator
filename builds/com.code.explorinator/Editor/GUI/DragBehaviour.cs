using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class DragBehaviour : PointerManipulator
    {
        private bool isDragging;

        public DragBehaviour(VisualElement target)
        {
            isDragging = false;
            this.target = target;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        private void PointerDownHandler(PointerDownEvent context)
        {
            context.StopPropagation();
            isDragging = true;
            target.CapturePointer(context.pointerId);
        }

        private void PointerMoveHandler(PointerMoveEvent context)
        {
            if (isDragging)
            {
                Vector3 delta = context.deltaPosition;

                target.style.marginLeft = delta.x + target.style.marginLeft.value.value;
                target.style.marginTop = delta.y + target.style.marginTop.value.value;
            }
        }

        private void PointerUpHandler(PointerUpEvent context)
        {
            isDragging = false;
            if (target.HasPointerCapture(context.pointerId))
            {
                target.ReleasePointer(context.pointerId);
            }
        }

        private void PointerCaptureOutHandler(PointerCaptureOutEvent context)
        {
            isDragging = false;
        }
    }
}