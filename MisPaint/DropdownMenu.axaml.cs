using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;

namespace MisPaint
{
    public partial class DropdownMenu : UserControl
    {
        public DropdownMenu()
        {
            InitializeComponent();
        }

        public void AddItem(string text, Action action)
        {
            var button = new Button
            {
                Content = text,
                Classes = { "menu-option" }
            };
            button.Click += (s, e) => action();
            MenuItems.Children.Add(button);
        }
    }
}
