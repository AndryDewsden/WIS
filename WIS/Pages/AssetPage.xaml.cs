using WIS.ApplicationData;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Reflection;
using System.Globalization;

namespace WIS.Pages
{
    public partial class AssetPage : Page
    {
        private WIS_Users currentUser;

        public AssetPage(WIS_Users user)
        {
            InitializeComponent();
            currentUser = user;
            InitializeUI();
            InitializePlaceholder();
            LoadAssets();
        }

        private void InitializePlaceholder()
        {
            Searcher.GotFocus += (s, e) =>
            {
                if (Searcher.Text == "Поиск...")
                {
                    Searcher.Text = "";
                    Searcher.Foreground = Brushes.Black;
                }
            };

            Searcher.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(Searcher.Text))
                {
                    Searcher.Text = "Поиск...";
                    Searcher.Foreground = Brushes.Gray;
                }
            };
        }

        private void InitializeUI()
        {
            userDisplay.Content = currentUser.user_firstname;
            ConfigureUserPermissions();
            InitializeFilters();
        }

        private void ConfigureUserPermissions()
        {
            bool isAdmin = currentUser.user_role_ID == 1;
            bool isManager = currentUser.user_role_ID == 2;
            bool isUser = currentUser.user_role_ID == 3;
            bool isF = currentUser.user_role_ID == 4;
            bool isIT = currentUser.user_role_ID == 5;

            addButton.Visibility = isAdmin || isManager ? Visibility.Visible : Visibility.Collapsed;
            editButton.Visibility = isAdmin || isManager ? Visibility.Visible : Visibility.Collapsed;

            addConMenu.Visibility = isAdmin || isManager ? Visibility.Visible : Visibility.Collapsed;
            editConMenu.Visibility = isAdmin || isManager ? Visibility.Visible : Visibility.Collapsed;

            userDisplay.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;
        }

        private void InitializeFilters()
        {
            Filter.ItemsSource = new[]
            {
                "Всё оборудование",
                "Рабочее оборудование",
                "Только свободное",
                "Эксплуатируемое",
                "На ремонте",
                "Списанное оборудование"
            };
            
            Filter.SelectedIndex = 1;
        }

        private void LoadAssets()
        {
            if (listAssets == null)
            {
                return;
            }

            try
            {
                var assets = FillAssets();
                listAssets.ItemsSource = assets;
                Counter.Text = $"Найдено: {assets.Length}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        WIS_Assets[] FillAssets()
        {
            try
            {
                if (AppConnect.Model?.WIS_Assets != null)
                {
                    var query = AppConnect.Model.WIS_Assets.AsQueryable();

                    if (!string.IsNullOrEmpty(Searcher.Text) && Searcher.Text != "Поиск...")
                    {
                        query = query.Where(x => x.asset_name.Contains(Searcher.Text) || x.asset_serial_number.Contains(Searcher.Text));
                    }

                    switch (Filter.SelectedIndex)
                    {
                        case 1:
                            query = query.Where(x => x.asset_status_ID == 2 || x.asset_status_ID == 3);
                            break;
                        case 2:
                            query = query.Where(x => x.asset_status_ID == 3);
                            break;
                        case 3:
                            query = query.Where(x => x.asset_status_ID == 2);
                            break;
                        case 4:
                            query = query.Where(x => x.asset_status_ID == 4);
                            break;
                        case 5:
                            query = query.Where(x => x.asset_status_ID == 1);
                            break;
                    }

                    switch (Sorter.SelectedIndex)
                    {
                        case 1:
                            query = query.OrderBy(x => x.asset_name);
                            break;
                        case 2:
                            query = query.OrderByDescending(x => x.asset_name);
                            break;
                        case 3:
                            query = query.OrderByDescending(x => x.ID_asset);
                            break; // по ID как по дате добавления
                        case 4:
                            query = query.OrderBy(x => x.ID_asset);
                            break;
                        case 5:
                            query = query.OrderBy(x => x.asset_status_ID);
                            break;
                    }

                    return query.ToArray();
                }
                else
                {
                    return new WIS_Assets[0];
                }
            }
            catch
            {
                return new WIS_Assets[0];
            }
        }

        private void Searcher_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Searcher.Text != "Поиск...")
            {
                LoadAssets();
            }
        }

        private void Searcher_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Searcher.Text))
            {
                Searcher.Visibility = Visibility.Collapsed;
                SearcherPlaceHolder.Visibility = Visibility.Visible;
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAssets();
        }

        private void Sorter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAssets();
        }

        private void userDisplay_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new UserProfilePage(currentUser));
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new AddEditPage(currentUser, null));
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            if (listAssets.SelectedItem is WIS_Assets asset)
            {
                NavigateToEditPage(asset);
            }
            else 
            {
                MessageBox.Show("Выберите оборудование для редактирования.", "Внимание", MessageBoxButton.OK,MessageBoxImage.Warning);
            }
        }
        private void bt_go_request_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new RequestPage(currentUser));
        }

        private void listAssets_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listAssets.SelectedItem is WIS_Assets asset)
            {
                NavigateToEditPage(asset);
            }
            else
            {
                MessageBox.Show("Выберите оборудование для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void addConMenu_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new AddEditPage(currentUser, null));
        }

        private void editConMenu_Click(object sender, RoutedEventArgs e)
        {
            if (listAssets.SelectedItem is WIS_Assets asset)
            {
                NavigateToEditPage(asset);
            }
            else
            {
                MessageBox.Show("Выберите оборудование для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearcherPlaceHolder_GotFocus(object sender, RoutedEventArgs e)
        {
            SearcherPlaceHolder.Visibility = Visibility.Collapsed;
            Searcher.Visibility = Visibility.Visible;
            Searcher.Focus();
        }
        private void NavigateToEditPage(WIS_Assets asset = null)
        {
            AppFrame.frameMain.Navigate(new AddEditPage(currentUser, asset));
        }
    }
}