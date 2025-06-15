using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WIS.ApplicationData;

namespace WIS.Pages
{
    public partial class ReportsPage : Page
    {
        private WIS_Users CurrentUser;
        private string _currentFilter = "";

        public ReportsPage(WIS_Users currentUser)
        {
            InitializeComponent();
            CurrentUser = currentUser;
            LoadReports();
        }

        private void LoadReports()
        {
            try
            {
                var query = AppConnect.Model.WIS_Reports.Include("WIS_Report_Types").Include("WIS_Users").AsQueryable();

                if (!string.IsNullOrWhiteSpace(_currentFilter))
                {
                    query = query.Where(r => r.report_name.Contains(_currentFilter));
                }

                var reports = query
                    .OrderByDescending(r => r.report_creation_date)
                    .ToList();

                ReportDataGrid.ItemsSource = reports;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке отчётов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentFilter = FilterTextBox.Text.Trim();
            LoadReports();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFilter = FilterTextBox.Text.Trim();
            LoadReports();
        }

        private void CreateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new ReportPage(CurrentUser));
            }
            else
            {
                MessageBox.Show("Навигация недоступна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
