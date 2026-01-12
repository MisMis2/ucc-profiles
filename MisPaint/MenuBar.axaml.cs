using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace MisPaint
{
    public partial class MenuBar : UserControl
    {
        private Popup? _currentPopup;

        public MenuBar()
        {
            InitializeComponent();
        }

        private void ShowMenu(Control target, DropdownMenu menu)
        {
            _currentPopup?.Close();
            
            var popup = new Popup
            {
                Child = menu,
                PlacementTarget = target,
                IsLightDismissEnabled = true,
                Placement = PlacementMode.Bottom
            };
            
            popup.Closed += (s, e) => _currentPopup = null;
            popup.Open();
            _currentPopup = popup;
        }

        private void OnFileClick(object? sender, RoutedEventArgs e)
        {
            var menu = new DropdownMenu();
            menu.AddItem("Создать", () => { });
            menu.AddItem("Открыть...", () => { });
            menu.AddItem("Сохранить", () => { });
            menu.AddItem("Сохранить как...", () => { });
            menu.AddItem("Выход", () => { });
            ShowMenu((Control)sender!, menu);
        }

        private void OnEditClick(object? sender, RoutedEventArgs e)
        {
            var menu = new DropdownMenu();
            menu.AddItem("Отменить", () => { });
            menu.AddItem("Повторить", () => { });
            menu.AddItem("Вырезать", () => { });
            menu.AddItem("Копировать", () => { });
            menu.AddItem("Вставить", () => { });
            ShowMenu((Control)sender!, menu);
        }

        private void OnViewClick(object? sender, RoutedEventArgs e)
        {
            var menu = new DropdownMenu();
            menu.AddItem("Увеличить", () => { });
            menu.AddItem("Уменьшить", () => { });
            menu.AddItem("100%", () => { });
            ShowMenu((Control)sender!, menu);
        }

        private void OnToolsClick(object? sender, RoutedEventArgs e)
        {
            var menu = new DropdownMenu();
            menu.AddItem("Кисть", () => { });
            menu.AddItem("Ластик", () => { });
            menu.AddItem("Заливка", () => { });
            menu.AddItem("Пипетка", () => { });
            ShowMenu((Control)sender!, menu);
        }
    }
}
