using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ZoomBehaviour : PointerManipulator
{
    public Vector2 Scale { get; private set; }
    private long lastTimeStamp = 0;
    private readonly float scaleFactor;
    private readonly float minScale = 0.001f;
    public ZoomBehaviour(VisualElement target, float scaleFactor = 1.01f)
    {
        this.target = target;
        if ((Vector2)target.style.scale.value.value == Vector2.zero)
        {
            throw new System.Exception($"the scale of the visual element {target} is (0, 0) you should initialize it somewhere");
        }

        this.scaleFactor = scaleFactor;
        Scale = target.style.scale.value.value;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<WheelEvent>(WheelHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<WheelEvent>(WheelHandler);
    }

    private void WheelHandler(WheelEvent context)
    {
        //context.timestamp seems to be in milliseconds
        long elapsedTimeWheelHandler = context.timestamp - lastTimeStamp;
        Vector2 scale = target.style.scale.value.value;

        float scaleFactor = /*context.delta.y * */this.scaleFactor;

        //Quick zoom scale
        if(elapsedTimeWheelHandler <= 50)
        {
            //each milisecond reduces the factor by 10 %. The maximum is 100% + 10%. the minimum 100% + 0%
            scaleFactor *= 1 + (50 -elapsedTimeWheelHandler) / 500f;
        }

        if(context.delta.y > 0)
        {
            scaleFactor = 1 / scaleFactor;
        }

        Vector2 newScale = scale * scaleFactor;

        if (newScale.x < minScale)
        {
            newScale = new Vector2(minScale, minScale);
        }

        #region ImageReadjustment
        //centered position of the target
        Vector2 targetPos = new Vector2(target.style.marginLeft.value.value, target.style.marginTop.value.value) 
            + new Vector2(target.style.width.value.value, target.style.height.value.value) * 0.5f;
        //mouse position with the origin at the middle of the target
        Vector2 relativeMousePos = context.mousePosition - targetPos;
        
        //When the scale decreases, the mouse pos will be further away from the center so we need the reciprocal of the new scale instead of newScale
        Vector2 scaledRelativeMousePos = relativeMousePos * scale.x / newScale.x;
        Vector2 mousPosDelta = scaledRelativeMousePos - relativeMousePos;
        target.style.marginLeft = target.style.marginLeft.value.value + mousPosDelta.x;
        target.style.marginTop = target.style.marginTop.value.value + mousPosDelta.y;
        #endregion

        Scale = newScale;
        target.style.scale = new StyleScale(new Scale(newScale));
        lastTimeStamp = context.timestamp;
    }
}
