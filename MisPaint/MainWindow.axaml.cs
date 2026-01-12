using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MisPaint
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ToolBar.UndoRequested += (s, e) => Canvas.Undo();
            ToolBar.RedoRequested += (s, e) => Canvas.Redo();
            ToolBar.BrushSelected += (s, e) => Canvas.SetTool(Tool.Brush);
            ToolBar.EraserSelected += (s, e) => Canvas.SetTool(Tool.Eraser);
            ToolBar.FillSelected += (s, e) => Canvas.SetTool(Tool.Fill);
            ToolBar.PickerSelected += (s, e) => Canvas.SetTool(Tool.Picker);
        }

        private void OnNew(object? sender, RoutedEventArgs e) => Canvas.Clear();
        private void OnOpen(object? sender, RoutedEventArgs e) { }
        private void OnSave(object? sender, RoutedEventArgs e) { }
        private void OnSaveAs(object? sender, RoutedEventArgs e) { }
        private void OnExit(object? sender, RoutedEventArgs e) => Close();
        private void OnUndo(object? sender, RoutedEventArgs e) => Canvas.Undo();
        private void OnRedo(object? sender, RoutedEventArgs e) => Canvas.Redo();
        private async void OnColorSpaceStore(object? sender, RoutedEventArgs e) => await new ColorSpaceStore().ShowDialog(this);
    }
}