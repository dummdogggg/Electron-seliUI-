using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SeliwareAPI;

namespace Electron
{
    public partial class MainWindow : Window
    {
        private Settings _Settings;

        public MainWindow()
        {
            InitializeComponent();



            _Settings = new Settings();

            PositionSettings();
            this.LocationChanged += MainWindow_LocationChanged;
            this.SizeChanged += MainWindow_SizeChanged;

            Editor.Navigate(new Uri($"file:///{Directory.GetCurrentDirectory()}/monaco/index.html"));

            LoadScripts();

            // Initialize Seliware
            Seliware.Initialize();

            // Set up event handler for successful injection
            Seliware.Injected += delegate
            {
                MessageBox.Show("Successfully injected!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            };
        }

        private void LoadScripts()
        {
            foreach (string extension in new[] { "*.txt", "*.lua" })
            {
                foreach (FileInfo file in new DirectoryInfo("./scripts").GetFiles(extension))
                {
                    ListBoxItem item = new ListBoxItem
                    {
                        Content = file.Name,
                        Style = (Style)FindResource("CustomListBoxItemStyle")
                    };
                    Scripts.Items.Add(item);
                }
            }
        }

        private void Scripts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Scripts.SelectedItem is ListBoxItem selectedItem)
            {
                string fileName = selectedItem.Content.ToString();
                string script = File.ReadAllText(Path.Combine("./scripts", fileName));

                Editor.InvokeScript("setValue", script);
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            PositionSettings();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionSettings();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Editor.InvokeScript("setValue", "");
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Filter = "Txt files (*.txt)|*.txt|Lua Files (*.lua)|*.lua|All Files (*.*)|*.*";

            if (Dialog.ShowDialog() == true)
            {
                string Script = File.ReadAllText(Dialog.FileName);
                Editor.InvokeScript("setValue", Script);
            }
        }

        private void Attach_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string result = Seliware.Inject();
                if (result == "Success")
                {
                    MessageBox.Show("Successfully attached to Roblox!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to attach: {result}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string luaScriptContent = Editor.InvokeScript("getValue").ToString();
                await ExecuteScript(luaScriptContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error trying to execute script: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteScript(string script)
        {
            await Task.Run(() =>
            {
                bool executed = Seliware.Execute(script);
                if (!executed)
                {
                    MessageBox.Show("Failed to execute script. Make sure Seliware is injected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void PositionSettings()
        {
            _Settings.Left = this.Left + this.Width + 10;
            _Settings.Top = this.Top;
            _Settings.Height = this.Height;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_Settings.IsVisible)
            {
                _Settings.Hide();
            }
            else
            {
                PositionSettings();
                _Settings.Show();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _Settings.Close();
        }
    }
}