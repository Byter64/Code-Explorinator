using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class SearchListEntry : Label
    {
        private string originalText;
        private ClickBehaviour clickBehaviour;
        private MenuGUI menu;
        public SearchListEntry(string text, MenuGUI menu) : base(text)
        {
            clickBehaviour = new ClickBehaviour(this, null, OnDoubleClick);
            clickBehaviour.RegisterOnControlMonoClick(OnControlMonoClick);
            this.menu = menu;
            originalText = text;
        }

        public void SetUnselected()
        {
            text = originalText;
        }
        public void SetFocused()
        {
            text = "<b><color=green>" + originalText + " F </color=green></b>";
        }

        public void SetSelected()
        {
            text = "<color=green>" + originalText + " </color=green>";
        }

        private void OnDoubleClick()
        {
            menu.AddSelectedClass(originalText);
            menu.ApplySelectedClasses();
        }

        private void OnControlMonoClick()
        {
            menu.AddSelectedClass(originalText);
            SetSelected();
        }

    }
}