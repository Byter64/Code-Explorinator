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
            clickBehaviour = new ClickBehaviour(this, OnMonoClick);
            clickBehaviour.RegisterOnControlMonoClick(OnControlMonoClick);
            this.menu = menu;
            originalText = text;
        }

        public void SetUnselected()
        {
            text = originalText;
        }

        private void OnMonoClick()
        {
            menu.AddSelectedClass(originalText);
            menu.ApplySelectedClasses();
        }

        private void OnControlMonoClick()
        {
            menu.AddSelectedClass(originalText);
            SetSelected();
        }

        private void SetSelected()
        {
            text = "<b><u>"+ originalText + "</b></u>";
        }
    }
}