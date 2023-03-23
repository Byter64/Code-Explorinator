using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/* This code was copied from https://answers.unity.com/questions/1865976/ui-toolkit-text-best-fit.html on 23.03.2023
 * More precisely from curbols answer.
 * A Constructor with a string argument was added by using System.Linq; (<-- auto complete. Actually it was added by me)
 * The UpdateFontSize method was fully rewritten as it did not behave as needed
*/

public class AutoFitLabel : Label
{
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<AutoFitLabel, UxmlTraits> { }

    [UnityEngine.Scripting.Preserve]
    public new class UxmlTraits : Label.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

        public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext)
        {
            base.Init(visualElement, attributes, creationContext);
        }
    }

    public AutoFitLabel()
    {
        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    }

    public AutoFitLabel(string text)
    {
        this.text = text;
        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    }

    private void OnAttachToPanel(AttachToPanelEvent e)
    {
        UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent e)
    {
        UpdateFontSize();
    }

    private void UpdateFontSize()
    {
        UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        try
        {
            //Everything in pixels
            float labelWidth = resolvedStyle.width;
            float labelHeight = resolvedStyle.height;            
            Vector2 currentTextSize = MeasureTextSize(text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);

            Vector2 relativeLabelSize = new Vector2(labelWidth / currentTextSize.x, labelHeight / currentTextSize.y);

            float maxFactor = Mathf.Min(relativeLabelSize.x, relativeLabelSize.y);
            float newFontSize = Mathf.Round(maxFactor * style.fontSize.value.value);
            style.fontSize = 30;
            //if (Mathf.RoundToInt(currentTextSize.y) != newFontSize)
            //    style.fontSize = new StyleLength(new Length(newFontSize));
        }
        finally
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
    }
}