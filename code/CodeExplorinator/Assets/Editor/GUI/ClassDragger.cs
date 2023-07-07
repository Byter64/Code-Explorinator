using CodeExplorinator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ClassDragger : VisualElement
{
    private VisualElement target;
    private HashSet<ConnectionGUI> connections;
    private Vector2 mousePosOnStartMoving;
    private Vector2 posOnStartMoving;
    public ClassDragger(VisualElement target, Vector2 mousePosOnStartMoving, HashSet<ConnectionGUI> connections)
    {
        this.target = target;
        this.mousePosOnStartMoving = mousePosOnStartMoving;
        this.connections = connections;
        posOnStartMoving = new Vector2(target.style.marginLeft.value.value, target.style.marginTop.value.value);

        this.RegisterCallback<PointerMoveEvent>(Move);
        this.RegisterCallback<PointerLeaveEvent>(LeaveWrapper);
        this.RegisterCallback<PointerUpEvent>(UpWrapper);
    }

    void Move(PointerMoveEvent context)
    {
        Vector2 delta = (Vector2)context.position - mousePosOnStartMoving;
        target.style.marginLeft = posOnStartMoving.x + delta.x * 1 / CodeExplorinatorGUI.Scale.x;
        target.style.marginTop = posOnStartMoving.y + delta.y * 1 / CodeExplorinatorGUI.Scale.y;

        foreach (ConnectionGUI conny in connections)
        {
            conny.UpdatePosition();
        }
    }

    void LeaveWrapper(PointerLeaveEvent context)
    {
        EndMoveChild();
    }

    void UpWrapper(PointerUpEvent context)
    {
        EndMoveChild();
    }

    void EndMoveChild()
    {
        if (target.Contains(this))
        {
            target.Remove(this);
        } 
    }
}
