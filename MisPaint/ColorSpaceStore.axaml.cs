using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MisPaint
{
    public partial class ColorSpaceStore : Window
    {
        private static readonly HttpClient _http = new();
        private readonly ListBox _list;
        private readonly Button _officialTab, _communityTab, _actionBtn, _uploadBtn;
        private bool _isOfficial = true;
        
        private readonly Dictionary<string, string> _official = new()
        {
            ["sRGB IEC61966-2.1"] = "https://www.color.org/srgbprofiles.xalter",
            ["Adobe RGB (1998)"] = "https://www.adobe.com/support/downloads/iccprofiles/iccprofiles_win.html",
            ["ProPhoto RGB"] = "https://www.color.org/chardata/rgb/rommrgb.xalter",
            ["DCI-P3"] = "https://www.color.org/chardata/rgb/DCIP3.xalter",
            ["ECI RGB v2"] = "https://www.eci.org/en/downloads",
            ["Rec. 2020"] = "https://www.itu.int/rec/R-REC-BT.2020"
        };
        
        private readonly List<string> _community = new();
        private const string CLOUD_URL = "https://raw.githubusercontent.com/mispaint/icc-profiles/main/";

        public ColorSpaceStore()
        {
            InitializeComponent();
            
            _list = this.FindControl<ListBox>("ProfileList")!;
            _officialTab = this.FindControl<Button>("OfficialTab")!;
            _communityTab = this.FindControl<Button>("CommunityTab")!;
            _actionBtn = this.FindControl<Button>("ActionBtn")!;
            _uploadBtn = this.FindControl<Button>("UploadBtn")!;
            var closeBtn = this.FindControl<Button>("CloseBtn")!;

            LoadCommunityProfiles();
            ShowOfficial();

            _officialTab.Click += (s, e) => ShowOfficial();
            _communityTab.Click += (s, e) => ShowCommunity();
            _actionBtn.Click += OnAction;
            _uploadBtn.Click += OnUpload;
            closeBtn.Click += (s, e) => Close();
        }

        private async void LoadCommunityProfiles()
        {
            try
            {
                var response = await _http.GetStringAsync(CLOUD_URL + "list.txt");
                foreach (var line in response.Split('\n'))
                {
                    var name = line.Trim();
                    if (!string.IsNullOrEmpty(name)) _community.Add(name);
                }
            }
            catch { _community.Add("[Ошибка загрузки облака]"); }
        }

        private void ShowOfficial()
        {
            _isOfficial = true;
            _officialTab.Background = new SolidColorBrush(Color.Parse("#0078D4"));
            _communityTab.Background = new SolidColorBrush(Color.Parse("#3E3E42"));
            _actionBtn.Content = "Скачать";
            _actionBtn.IsVisible = true;
            _uploadBtn.IsVisible = false;
            _list.Items.Clear();
            foreach (var p in _official.Keys) _list.Items.Add(p);
        }

        private void ShowCommunity()
        {
            _isOfficial = false;
            _officialTab.Background = new SolidColorBrush(Color.Parse("#3E3E42"));
            _communityTab.Background = new SolidColorBrush(Color.Parse("#0078D4"));
            _actionBtn.Content = "Скачать из облака";
            _actionBtn.IsVisible = true;
            _uploadBtn.IsVisible = true;
            _list.Items.Clear();
            foreach (var p in _community) _list.Items.Add(p);
        }

        private async void OnAction(object? s, RoutedEventArgs e)
        {
            if (_list.SelectedItem == null) return;
            var name = _list.SelectedItem.ToString()!;

            if (_isOfficial)
            {
                await ShowMessage($"Скачайте с официального сайта:\n{_official[name]}");
            }
            else
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MisPaint", "ICC", "Community");
                Directory.CreateDirectory(folder);
                var file = Path.Combine(folder, name + ".icc");
                
                try
                {
                    var url = CLOUD_URL + name + ".icc";
                    var downloadWindow = new DownloadManager(name, url, file);
                    await downloadWindow.ShowDialog(this);
                    await ShowMessage("Профиль скачан!");
                }
                catch (Exception ex)
                {
                    await ShowMessage($"Ошибка: {ex.Message}");
                }
            }
        }

        private async void OnUpload(object? s, RoutedEventArgs e)
        {
            await ShowMessage("Чтобы загрузить свой профиль в облако:\n\n1. Создайте fork репозитория:\ngithub.com/mispaint/icc-profiles\n\n2. Добавьте .icc файл\n\n3. Добавьте имя в list.txt\n\n4. Создайте Pull Request");
        }

        

        private async Task ShowMessage(string msg)
        {
            var w = new Window { Width = 300, Height = 120, Title = "Сообщение", Background = Background };
            var stack = new StackPanel { Margin = new Thickness(20) };
            stack.Children.Add(new TextBlock { Text = msg, Foreground = new SolidColorBrush(Colors.White), TextWrapping = Avalonia.Media.TextWrapping.Wrap });
            var btn = new Button { Content = "OK", Width = 80, Margin = new Thickness(0, 15, 0, 0) };
            btn.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            btn.Click += (s, e) => w.Close();
            btn.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            stack.Children.Add(btn);
            w.Content = stack;
            await w.ShowDialog(this);
        }
    }
}
