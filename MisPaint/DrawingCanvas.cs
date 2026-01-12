using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace MisPaint
{
    public enum Tool { Brush, Eraser, Fill, Picker }

    public class Stroke
    {
        public List<SKPoint> Points { get; set; } = [];
        public Tool Tool { get; set; }
    }

    public class DrawingCanvas : UserControl
    {
        private readonly List<Stroke> _strokes = [];
        private readonly Stack<Stroke> _undoStack = new();
        private Stroke? _currentStroke;
        private bool _isDrawing;
        private WriteableBitmap? _bitmap;
        private Tool _currentTool = Tool.Brush;
        private readonly SKPaint _brushPaint = new()
        {
            Color = SKColors.Black,
            StrokeWidth = 2,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
        private readonly SKPaint _eraserPaint = new()
        {
            Color = SKColors.White,
            StrokeWidth = 20,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        public void SetTool(Tool tool) => _currentTool = tool;

        public int GetBrushSize() => (int)(_currentTool == Tool.Eraser ? _eraserPaint.StrokeWidth : _brushPaint.StrokeWidth);
        
        public string GetBrushColor() => $"#{_brushPaint.Color.Red:X2}{_brushPaint.Color.Green:X2}{_brushPaint.Color.Blue:X2}";
        
        public int GetBrushOpacity() => _brushPaint.Color.Alpha * 100 / 255;

        public void SetBrushSize(int size)
        {
            if (_currentTool == Tool.Eraser)
                _eraserPaint.StrokeWidth = size;
            else
                _brushPaint.StrokeWidth = size;
        }

        public void SetBrushColor(string hexColor)
        {
            var color = SKColor.Parse(hexColor);
            _brushPaint.Color = color;
        }

        public void SetBrushOpacity(byte opacity)
        {
            var color = _brushPaint.Color;
            _brushPaint.Color = new SKColor(color.Red, color.Green, color.Blue, opacity);
        }

        private SKPaint GetPaintForTool(Tool tool) => tool == Tool.Eraser ? _eraserPaint : _brushPaint;

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                _bitmap = new WriteableBitmap(new PixelSize((int)e.NewSize.Width, (int)e.NewSize.Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
                using var fb = _bitmap.Lock();
                var info = new SKImageInfo(fb.Size.Width, fb.Size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                using var surface = SKSurface.Create(info, fb.Address, fb.RowBytes);
                surface.Canvas.Clear(SKColors.White);
            }
        }

        public override void Render(Avalonia.Media.DrawingContext context)
        {
            base.Render(context);
            if (_bitmap != null)
            {
                context.DrawImage(_bitmap, new Rect(0, 0, _bitmap.PixelSize.Width, _bitmap.PixelSize.Height));
            }
        }

        private static SKPath CreateCatmullRomPath(List<SKPoint> points)
        {
            var path = new SKPath();
            if (points.Count < 2) return path;
            
            path.MoveTo(points[0]);
            if (points.Count == 2)
            {
                path.LineTo(points[1]);
                return path;
            }
            
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p0 = points[Math.Max(0, i - 1)];
                var p1 = points[i];
                var p2 = points[i + 1];
                var p3 = points[Math.Min(points.Count - 1, i + 2)];
                
                for (float t = 0; t <= 1; t += 0.2f)
                {
                    float t2 = t * t;
                    float t3 = t2 * t;
                    
                    float x = 0.5f * ((2 * p1.X) + (-p0.X + p2.X) * t + (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 + (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3);
                    float y = 0.5f * ((2 * p1.Y) + (-p0.Y + p2.Y) * t + (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 + (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3);
                    
                    path.LineTo(x, y);
                }
            }
            
            return path;
        }

        private void Redraw()
        {
            if (_bitmap == null) return;
            
            using var fb = _bitmap.Lock();
            var info = new SKImageInfo(fb.Size.Width, fb.Size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info, fb.Address, fb.RowBytes);
            
            surface.Canvas.Clear(SKColors.White);
            
            foreach (var stroke in _strokes)
            {
                using var path = CreateCatmullRomPath(stroke.Points);
                var paint = GetPaintForTool(stroke.Tool);
                surface.Canvas.DrawPath(path, paint);
            }
            
            InvalidateVisual();
        }

        public void Clear()
        {
            _strokes.Clear();
            _undoStack.Clear();
            Redraw();
        }

        public void Undo()
        {
            if (_strokes.Count > 0)
            {
                var stroke = _strokes[^1];
                _strokes.RemoveAt(_strokes.Count - 1);
                _undoStack.Push(stroke);
                Redraw();
            }
        }

        public void Redo()
        {
            if (_undoStack.Count > 0)
            {
                var stroke = _undoStack.Pop();
                _strokes.Add(stroke);
                Redraw();
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isDrawing = true;
                _currentStroke = new Stroke { Tool = _currentTool };
                var point = e.GetPosition(this);
                _currentStroke.Points.Add(new SKPoint((float)point.X, (float)point.Y));
                e.Pointer.Capture(this);
                e.Handled = true;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_isDrawing && _currentStroke != null && _bitmap != null && _currentStroke.Points.Count > 0)
            {
                var point = e.GetPosition(this);
                var skPoint = new SKPoint((float)point.X, (float)point.Y);
                var prev = _currentStroke.Points[^1];
                
                if (SKPoint.Distance(prev, skPoint) > 1f)
                {
                    _currentStroke.Points.Add(skPoint);
                    
                    using var fb = _bitmap.Lock();
                    var info = new SKImageInfo(fb.Size.Width, fb.Size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                    using var surface = SKSurface.Create(info, fb.Address, fb.RowBytes);
                    
                    var paint = GetPaintForTool(_currentStroke.Tool);
                    surface.Canvas.DrawLine(prev, skPoint, paint);
                    InvalidateVisual();
                }
                
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (_isDrawing && _currentStroke != null && _bitmap != null && _currentStroke.Points.Count > 2)
            {
                _undoStack.Clear();
                _strokes.Add(_currentStroke);
                Redraw();
                
                _currentStroke = null;
                _isDrawing = false;
                e.Pointer.Capture(null);
                e.Handled = true;
            }
            else if (_isDrawing)
            {
                if (_currentStroke != null) _strokes.Add(_currentStroke);
                _currentStroke = null;
                _isDrawing = false;
                e.Pointer.Capture(null);
                e.Handled = true;
            }
        }
    }
}