using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace MisPaint
{
    public partial class ToolProperties : UserControl
    {
        public new event EventHandler<int>? SizeChanged;
        public event EventHandler<Color>? ColorChanged;
        public event EventHandler<int>? OpacityChanged;
        public event EventHandler? CloseRequested;
        public event EventHandler<bool>? PinToggled;
        public event EventHandler<Tool>? ToolChanged;

        private bool _isPinned;
        private string _currentColor = "#000000";

        public ToolProperties()
        {
            InitializeComponent();
            
            var sizeSlider = this.FindControl<Slider>("SizeSlider");
            var opacitySlider = this.FindControl<Slider>("OpacitySlider");
            var sizeText = this.FindControl<TextBlock>("SizeValueText");
            var opacityText = this.FindControl<TextBlock>("OpacityValueText");
            
            if (sizeSlider != null)
            {
                sizeSlider.ValueChanged += (s, e) =>
                {
                    SizeChanged?.Invoke(this, (int)sizeSlider.Value);
                    if (sizeText != null) sizeText.Text = $"{(int)sizeSlider.Value}px";
                };
            }
            
            if (opacitySlider != null)
            {
                opacitySlider.ValueChanged += (s, e) =>
                {
                    OpacityChanged?.Invoke(this, (int)opacitySlider.Value);
                    if (opacityText != null) opacityText.Text = $"{(int)opacitySlider.Value}%";
                };
            }
        }

        public void SetTool(Tool tool)
        {
            var opacityPanel = this.FindControl<StackPanel>("OpacityPanel");
            var sizePanel = this.FindControl<StackPanel>("SizePanel");
            var colorPanel = this.FindControl<StackPanel>("ColorPanel");
            var toolCombo = this.FindControl<ComboBox>("ToolCombo");
            var sizeSlider = this.FindControl<Slider>("SizeSlider");
            
            if (opacityPanel != null)
                opacityPanel.IsVisible = tool == Tool.Brush;
            
            if (sizePanel != null)
                sizePanel.IsVisible = tool == Tool.Brush || tool == Tool.Eraser;
            
            if (colorPanel != null)
                colorPanel.IsVisible = tool == Tool.Brush;
            
            if (sizeSlider != null)
            {
                if (tool == Tool.Eraser)
                {
                    sizeSlider.Minimum = 10;
                    sizeSlider.Maximum = 100;
                    sizeSlider.Value = 20;
                    if (toolCombo != null) toolCombo.SelectedIndex = 1;
                }
                else if (tool == Tool.Fill)
                {
                    if (toolCombo != null) toolCombo.SelectedIndex = 2;
                }
                else
                {
                    sizeSlider.Minimum = 1;
                    sizeSlider.Maximum = 50;
                    sizeSlider.Value = 2;
                    if (toolCombo != null) toolCombo.SelectedIndex = 0;
                }
            }
        }

        private void OnColorClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.Tag is string colorHex)
            {
                _currentColor = colorHex;
                var currentColorBorder = this.FindControl<Border>("CurrentColorBorder");
                if (currentColorBorder != null)
                    currentColorBorder.Background = new SolidColorBrush(Color.Parse(colorHex));
                ColorChanged?.Invoke(this, Color.Parse(colorHex));
            }
        }

        private async void OnCurrentColorClick(object? sender, PointerPressedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is Window window)
            {
                window.Topmost = false;
                var picker = new SimpleColorPicker(Color.Parse(_currentColor));
                var result = await picker.ShowDialog<Color?>(window);
                window.Topmost = true;

                if (result.HasValue)
                {
                    _currentColor = $"#{result.Value.R:X2}{result.Value.G:X2}{result.Value.B:X2}";
                    var currentColorBorder = this.FindControl<Border>("CurrentColorBorder");
                    if (currentColorBorder != null)
                        currentColorBorder.Background = new SolidColorBrush(result.Value);
                    ColorChanged?.Invoke(this, result.Value);
                }
            }
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnPinClick(object? sender, RoutedEventArgs e)
        {
            _isPinned = !_isPinned;
            var pinIcon = this.FindControl<FluentIcons.Avalonia.Fluent.SymbolIcon>("PinIcon");
            if (pinIcon != null)
                pinIcon.Symbol = _isPinned ? FluentIcons.Common.Symbol.PinOff : FluentIcons.Common.Symbol.Pin;
            PinToggled?.Invoke(this, _isPinned);
        }

        private void OnToolChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox toolCombo && this.IsInitialized)
            {
                Tool tool = Tool.Brush;
                if (toolCombo.SelectedIndex == 0)
                    tool = Tool.Brush;
                else if (toolCombo.SelectedIndex == 1)
                    tool = Tool.Eraser;
                else if (toolCombo.SelectedIndex == 2)
                    tool = Tool.Fill;
                
                var opacityPanel = this.FindControl<StackPanel>("OpacityPanel");
                var sizePanel = this.FindControl<StackPanel>("SizePanel");
                var colorPanel = this.FindControl<StackPanel>("ColorPanel");
                
                if (opacityPanel != null)
                    opacityPanel.IsVisible = tool == Tool.Brush;
                
                if (sizePanel != null)
                    sizePanel.IsVisible = tool == Tool.Brush || tool == Tool.Eraser;
                
                if (colorPanel != null)
                    colorPanel.IsVisible = tool == Tool.Brush;
                
                ToolChanged?.Invoke(this, tool);
            }
        }

        private void OnDecreaseSizeClick(object? sender, RoutedEventArgs e)
        {
            var sizeSlider = this.FindControl<Slider>("SizeSlider");
            if (sizeSlider != null && sizeSlider.Value > sizeSlider.Minimum)
                sizeSlider.Value--;
        }

        private void OnIncreaseSizeClick(object? sender, RoutedEventArgs e)
        {
            var sizeSlider = this.FindControl<Slider>("SizeSlider");
            if (sizeSlider != null && sizeSlider.Value < sizeSlider.Maximum)
                sizeSlider.Value++;
        }
    }
}
