using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace MisPaint
{
    public class SimpleColorPicker : Window
    {
        public Color SelectedColor { get; private set; }
        private readonly Border _preview;
        private readonly Slider _rSlider, _gSlider, _bSlider, _aSlider, _vSlider, _hdrSlider;
        private readonly TextBlock _hexInput;
        private readonly ComboBox _colorSpaceCombo, _bitDepthCombo;

        public SimpleColorPicker(Color initialColor)
        {
            SelectedColor = initialColor;
            Width = 280;
            Height = 600;
            Title = "Выбор цвета";
            Background = new SolidColorBrush(Color.Parse("#2D2D30"));
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var stack = new StackPanel { Margin = new Thickness(15) };
            Content = stack;

            _preview = new Border
            {
                Width = 250,
                Height = 60,
                Background = new SolidColorBrush(initialColor),
                CornerRadius = new CornerRadius(4),
                BorderBrush = new SolidColorBrush(Color.Parse("#3F3F46")),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 0, 0, 15)
            };
            stack.Children.Add(_preview);

            _rSlider = CreateSlider("R", initialColor.R, stack);
            _gSlider = CreateSlider("G", initialColor.G, stack);
            _bSlider = CreateSlider("B", initialColor.B, stack);
            _aSlider = CreateSlider("A", initialColor.A, stack);
            _vSlider = CreateSlider("V", 100, stack);
            _hdrSlider = CreateSlider("HDR", 100, stack);

            _colorSpaceCombo = new ComboBox { SelectedIndex = 0, FontSize = 10, Height = 24, Margin = new Thickness(0, 5, 0, 8) };
            _colorSpaceCombo.Items.Add("sRGB");
            _colorSpaceCombo.Items.Add("Adobe RGB");
            _colorSpaceCombo.Items.Add("ProPhoto RGB");
            _colorSpaceCombo.Items.Add("DCI-P3");
            _colorSpaceCombo.SelectionChanged += (s, e) => OnValueChanged(s, e);
            stack.Children.Add(_colorSpaceCombo);

            _bitDepthCombo = new ComboBox { SelectedIndex = 2, FontSize = 10, Height = 24, Margin = new Thickness(0, 0, 0, 8) };
            _bitDepthCombo.Items.Add("2 бит");
            _bitDepthCombo.Items.Add("4 бит");
            _bitDepthCombo.Items.Add("8 бит");
            _bitDepthCombo.Items.Add("16 бит");
            _bitDepthCombo.Items.Add("32 бит");
            _bitDepthCombo.Items.Add("64 бит");
            _bitDepthCombo.SelectionChanged += (s, e) => OnValueChanged(s, e);
            stack.Children.Add(_bitDepthCombo);

            var storeBtn = new Button
            {
                Content = "Магазин ICC профилей",
                Height = 28,
                Background = new SolidColorBrush(Color.Parse("#3E3E42")),
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 5, 0, 10)
            };
            storeBtn.Click += async (s, e) => await new ColorSpaceStore().ShowDialog(this);
            stack.Children.Add(storeBtn);

            _hexInput = new TextBlock
            {
                Text = $"#{initialColor.R:X2}{initialColor.G:X2}{initialColor.B:X2}",
                Foreground = Brushes.White,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 10)
            };
            stack.Children.Add(_hexInput);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 10
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 28,
                Background = new SolidColorBrush(Color.Parse("#0078D4")),
                Foreground = Brushes.White
            };
            okButton.Click += (s, e) => Close(SelectedColor);

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 80,
                Height = 28,
                Background = new SolidColorBrush(Color.Parse("#3E3E42")),
                Foreground = Brushes.White
            };
            cancelButton.Click += (s, e) => Close(null);

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stack.Children.Add(buttonPanel);

            _rSlider.ValueChanged += OnColorChanged;
            _gSlider.ValueChanged += OnColorChanged;
            _bSlider.ValueChanged += OnColorChanged;
            _aSlider.ValueChanged += OnColorChanged;
            _vSlider.ValueChanged += OnValueChanged;
            _hdrSlider.ValueChanged += OnValueChanged;
        }

        private Slider CreateSlider(string name, byte value, StackPanel parent)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };

            var header = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };

            var label = new TextBlock
            {
                Text = name,
                Foreground = new SolidColorBrush(Color.Parse("#AAAAAA")),
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(label, 0);

            var valueText = new TextBlock
            {
                Text = value.ToString(),
                Foreground = Brushes.White,
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(valueText, 2);

            header.Children.Add(label);
            header.Children.Add(valueText);

            var slider = new Slider
            {
                Minimum = 0,
                Maximum = 255,
                Value = value,
                Margin = new Thickness(0, 2, 0, 0)
            };

            slider.ValueChanged += (s, e) => valueText.Text = ((int)slider.Value).ToString();

            panel.Children.Add(header);
            panel.Children.Add(slider);
            parent.Children.Add(panel);

            return slider;
        }

        private void OnColorChanged(object? sender, EventArgs e)
        {
            SelectedColor = Color.FromArgb((byte)_aSlider.Value, (byte)_rSlider.Value, (byte)_gSlider.Value, (byte)_bSlider.Value);
            _preview.Background = new SolidColorBrush(SelectedColor);
            _hexInput.Text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
        }

        private void OnValueChanged(object? sender, EventArgs e)
        {
            var vFactor = _vSlider.Value / 100.0;
            var hdrFactor = _hdrSlider.Value / 100.0;
            var totalFactor = vFactor * hdrFactor;
            
            var r = (byte)Math.Min(255, _rSlider.Value * totalFactor);
            var g = (byte)Math.Min(255, _gSlider.Value * totalFactor);
            var b = (byte)Math.Min(255, _bSlider.Value * totalFactor);
            
            // ICC profile color space conversion
            double rNorm = r / 255.0, gNorm = g / 255.0, bNorm = b / 255.0;
            
            if (_colorSpaceCombo.SelectedIndex == 1) // Adobe RGB (1998)
            {
                var rOut = 2.04159 * rNorm - 0.56501 * gNorm - 0.34473 * bNorm;
                var gOut = -0.96924 * rNorm + 1.87597 * gNorm + 0.04156 * bNorm;
                var bOut = 0.01344 * rNorm - 0.11836 * gNorm + 1.01517 * bNorm;
                r = (byte)Math.Clamp(rOut * 255, 0, 255);
                g = (byte)Math.Clamp(gOut * 255, 0, 255);
                b = (byte)Math.Clamp(bOut * 255, 0, 255);
            }
            else if (_colorSpaceCombo.SelectedIndex == 2) // ProPhoto RGB
            {
                var rOut = 1.3460 * rNorm - 0.2556 * gNorm - 0.0511 * bNorm;
                var gOut = -0.5446 * rNorm + 1.5082 * gNorm + 0.0205 * bNorm;
                var bOut = 0.0000 * rNorm + 0.0000 * gNorm + 1.2123 * bNorm;
                r = (byte)Math.Clamp(rOut * 255, 0, 255);
                g = (byte)Math.Clamp(gOut * 255, 0, 255);
                b = (byte)Math.Clamp(bOut * 255, 0, 255);
            }
            else if (_colorSpaceCombo.SelectedIndex == 3) // DCI-P3
            {
                var rOut = 2.4934 * rNorm - 0.9313 * gNorm - 0.4027 * bNorm;
                var gOut = -0.8295 * rNorm + 1.7626 * gNorm + 0.0236 * bNorm;
                var bOut = 0.0358 * rNorm - 0.0762 * gNorm + 0.9569 * bNorm;
                r = (byte)Math.Clamp(rOut * 255, 0, 255);
                g = (byte)Math.Clamp(gOut * 255, 0, 255);
                b = (byte)Math.Clamp(bOut * 255, 0, 255);
            }
            
            // Bit depth quantization
            int levels = _bitDepthCombo.SelectedIndex switch
            {
                0 => 4,    // 2-bit
                1 => 16,   // 4-bit
                2 => 256,  // 8-bit
                3 => 256,  // 16-bit (display as 8-bit)
                4 => 256,  // 32-bit (display as 8-bit)
                5 => 256,  // 64-bit (display as 8-bit)
                _ => 256
            };
            
            if (levels < 256)
            {
                r = (byte)(Math.Round(r / 255.0 * (levels - 1)) * 255 / (levels - 1));
                g = (byte)(Math.Round(g / 255.0 * (levels - 1)) * 255 / (levels - 1));
                b = (byte)(Math.Round(b / 255.0 * (levels - 1)) * 255 / (levels - 1));
            }
            
            SelectedColor = Color.FromArgb((byte)_aSlider.Value, r, g, b);
            _preview.Background = new SolidColorBrush(SelectedColor);
            _hexInput.Text = $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
