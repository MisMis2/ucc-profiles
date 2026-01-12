using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace MisPaint
{
    public partial class ToolBar : UserControl
    {
        public event EventHandler? BrushSelected;
        public event EventHandler? EraserSelected;
        public event EventHandler? FillSelected;
        public event EventHandler? PickerSelected;
        public event EventHandler? UndoRequested;
        public event EventHandler? RedoRequested;

        private Button? _activeButton;

        public ToolBar()
        {
            InitializeComponent();
            _activeButton = BrushButton;
            _activeButton.Classes.Add("active");
        }

        private void SetActiveButton(Button button)
        {
            _activeButton?.Classes.Remove("active");
            button.Classes.Add("active");
            _activeButton = button;
        }

        private void OnToolRightClick(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                var button = (Button)sender!;
                var props = new ToolProperties();
                
                Tool tool = Tool.Brush;
                if (button == BrushButton)
                    tool = Tool.Brush;
                else if (button == EraserButton)
                    tool = Tool.Eraser;
                else
                    tool = Tool.Fill;
                
                props.SetTool(tool);
                
                var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;
                if (mainWindow != null)
                {
                    var canvas = mainWindow.FindControl<DrawingCanvas>("Canvas");
                    if (canvas != null)
                    {
                        var sizeSlider = props.FindControl<Slider>("SizeSlider");
                        var opacitySlider = props.FindControl<Slider>("OpacitySlider");
                        var currentColorBorder = props.FindControl<Border>("CurrentColorBorder");
                        
                        if (sizeSlider != null) sizeSlider.Value = canvas.GetBrushSize();
                        if (opacitySlider != null) opacitySlider.Value = canvas.GetBrushOpacity();
                        if (currentColorBorder != null)
                        {
                            var colorHex = canvas.GetBrushColor();
                            currentColorBorder.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(colorHex));
                        }
                        
                        props.SizeChanged += (s, size) => canvas.SetBrushSize(size);
                        props.ColorChanged += (s, color) => canvas.SetBrushColor($"#{color.R:X2}{color.G:X2}{color.B:X2}");
                        props.OpacityChanged += (s, opacity) => canvas.SetBrushOpacity((byte)(opacity * 255 / 100));
                    }
                }

                var window = new Window
                {
                    Content = props,
                    Width = 150,
                    Height = 220,
                    CanResize = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    SystemDecorations = SystemDecorations.None,
                    Topmost = true,
                    Background = Avalonia.Media.Brushes.Transparent
                };
                
                bool isPinned = false;
                EventHandler? deactivatedHandler = null;
                
                deactivatedHandler = (s, ev) => { if (!isPinned && window.Topmost) window.Close(); };
                window.Deactivated += deactivatedHandler;
                
                props.CloseRequested += (s, ev) => window.Close();
                props.PinToggled += (s, pinned) => { isPinned = pinned; };
                props.ToolChanged += (s, t) => {
                    var canvas = mainWindow?.FindControl<DrawingCanvas>("Canvas");
                    canvas?.SetTool(t);
                };
                
                var parentWindow = TopLevel.GetTopLevel(this) as Window;
                if (parentWindow != null)
                {
                    void UpdatePosition()
                    {
                        var buttonPos = button.TranslatePoint(new Point(button.Bounds.Width + 8, 0), parentWindow);
                        if (buttonPos.HasValue)
                        {
                            var screenPos = parentWindow.PointToScreen(buttonPos.Value);
                            window.Position = screenPos;
                        }
                    }
                    
                    UpdatePosition();
                    
                    parentWindow.PositionChanged += (s, ev) => { if (!isPinned) window.Close(); else UpdatePosition(); };
                    parentWindow.Closing += (s, ev) => window.Close();
                }
                
                window.Show();
                
                e.Handled = true;
            }
        }

        private void OnBrushClick(object? sender, RoutedEventArgs e)
        {
            SetActiveButton((Button)sender!);
            BrushSelected?.Invoke(this, EventArgs.Empty);
        }

        private void OnEraserClick(object? sender, RoutedEventArgs e)
        {
            SetActiveButton((Button)sender!);
            EraserSelected?.Invoke(this, EventArgs.Empty);
        }

        private void OnFillClick(object? sender, RoutedEventArgs e)
        {
            SetActiveButton((Button)sender!);
            FillSelected?.Invoke(this, EventArgs.Empty);
        }

        private void OnPickerClick(object? sender, RoutedEventArgs e)
        {
            SetActiveButton((Button)sender!);
            PickerSelected?.Invoke(this, EventArgs.Empty);
        }

        private void OnUndoClick(object? sender, RoutedEventArgs e) => UndoRequested?.Invoke(this, EventArgs.Empty);
        private void OnRedoClick(object? sender, RoutedEventArgs e) => RedoRequested?.Invoke(this, EventArgs.Empty);
    }
}
