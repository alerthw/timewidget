using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Globalization;
using System.Diagnostics;
using System.Timers;
using Microsoft.Win32;

namespace TimeWidget
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer updateTimer;
        CultureInfo cultureInfo = new CultureInfo("en-US");
        private const int HWND_BOTTOM = 1;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public MainWindow()
        {
            InitializeComponent();
            SetWindowBottommost();
            LoadWindowPosition();
            CheckAndAddToAutostart();
            updateTimer = new Timer(1000);
            updateTimer.Elapsed += UpdateTimerElapsed;
            updateTimer.Start();
        }
        private void DragWindow(object sender, MouseButtonEventArgs e) { DragMove(); }
        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                DateTime currentTime = DateTime.Now;
                time.Text = currentTime.ToString("HH:mm");
                date.Text = currentTime.ToString("dddd, dd MMMM yyyy", cultureInfo);

            });
        }

        private void CheckAndAddToAutostart()
        {
            bool isFirstRun = Properties.Settings.Default.FirstRun;

            if (isFirstRun)
            {
                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.Save();

                MessageBoxResult result = MessageBox.Show("Добавить программу в автозапуск?", "Автозапуск", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AddToAutostart();
                }
            }
        }

        private void AddToAutostart()
        {
            try
            {
                string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                string appName = Path.GetFileNameWithoutExtension(executablePath);

                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key.SetValue(appName, executablePath);
                MessageBox.Show("Программа добавлена в автозапуск.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show($"Произошла ошибка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void LoadWindowPosition()
        {
            double x = Properties.Settings.Default.WindowPositionX;
            double y = Properties.Settings.Default.WindowPositionY;

            if (x >= 0 && y >= 0)
            {
                Left = x;
                Top = y;
            }
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            Properties.Settings.Default.WindowPositionX = Left;
            Properties.Settings.Default.WindowPositionY = Top;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            SetWindowBottommost();
        }

        private void SetWindowBottommost()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetWindowPos(hwnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
    }
}
