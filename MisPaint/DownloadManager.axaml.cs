using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MisPaint
{
    public partial class DownloadManager : Window
    {
        private readonly TextBlock _status, _speed;
        private readonly ProgressBar _progress;
        private CancellationTokenSource _cts = new();

        public DownloadManager(string name, string url, string filePath)
        {
            InitializeComponent();

            _status = this.FindControl<TextBlock>("StatusText")!;
            _speed = this.FindControl<TextBlock>("SpeedText")!;
            _progress = this.FindControl<ProgressBar>("Progress")!;
            var cancelBtn = this.FindControl<Button>("CancelBtn")!;

            _status.Text = $"Загрузка: {name}";
            cancelBtn.Click += (s, e) => { _cts.Cancel(); Close(); };

            _ = StartDownload(url, filePath);
        }

        private async Task StartDownload(string url, string filePath)
        {
            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                var total = response.Content.Headers.ContentLength ?? 0;
                
                using var stream = await response.Content.ReadAsStreamAsync(_cts.Token);
                using var file = File.Create(filePath);
                
                var buffer = new byte[8192];
                long downloaded = 0;
                var sw = Stopwatch.StartNew();

                while (true)
                {
                    var read = await stream.ReadAsync(buffer, _cts.Token);
                    if (read == 0) break;

                    await file.WriteAsync(buffer.AsMemory(0, read), _cts.Token);
                    downloaded += read;

                    var percent = total > 0 ? (downloaded * 100.0 / total) : 0;
                    var speed = downloaded / sw.Elapsed.TotalSeconds / 1024;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _progress.Value = percent;
                        _speed.Text = $"{downloaded / 1024} KB / {total / 1024} KB ({speed:F1} KB/s)";
                    });
                }

                _status.Text = "Загрузка завершена!";
                _status.Foreground = new SolidColorBrush(Color.Parse("#4EC9B0"));
                await Task.Delay(1000);
                Close();
            }
            catch (Exception ex)
            {
                _status.Text = $"Ошибка: {ex.Message}";
                _status.Foreground = Brushes.Red;
            }
        }
    }
}
