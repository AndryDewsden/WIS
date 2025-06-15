using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WIS.ApplicationData;
using WIS.Pages;

namespace WIS
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private string _appStatus = "Стабилен";
        public MainWindow()
        {
            InitializeComponent();

            InitializeDatabaseConnection();

            InitializeMainFrame();

            // Инициализация таймера для обновления времени
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateStatus("Стабилен");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TbDate.Text = $"Дата: {DateTime.Now:dd.MM.yyyy}";
            TbTime.Text = $"Время: {DateTime.Now:HH:mm:ss}";
        }

        private void InitializeDatabaseConnection()
        {
            try
            {
                AppConnect.Model = new WIS_databaseEntities();

                _ = AppConnect.Model.Database.Connection;

                UpdateStatus("Стабилен");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка подключения: {ex.Message}");
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void InitializeMainFrame()
        {
            AppFrame.frameMain = FrMain;

            FrMain.Navigated += (sender, e) =>
            {

            };

            FrMain.Navigate(new AutorisationPage());
        }

        public void UpdateStatus(string status)
        {
            _appStatus = status;
            TbStatus.Text = _appStatus;
        }
    }
}
