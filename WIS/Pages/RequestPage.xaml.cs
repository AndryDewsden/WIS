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
using WIS.ApplicationData;

namespace WIS.Pages
{
    public partial class RequestPage : Page
    {
        private WIS_Users _currentUser;

        public RequestPage(WIS_Users user)
        {
            InitializeComponent();
            _currentUser = user;
            InitializeUI();
            InitializePlaceholder();
            LoadRequests();
        }

        private bool IsPrivilegedUser()
        {
            // 1 - Администратор, 2 - Менеджер, 5 - IT-Специалист
            return _currentUser.user_role_ID == 1 || _currentUser.user_role_ID == 2 || _currentUser.user_role_ID == 5;
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
            userDisplay.Content = _currentUser.user_firstname;
            ConfigureUserPermissions();
            UpdateCounter();
        }

        private void ConfigureUserPermissions()
        {
            bool isPrivileged = IsPrivilegedUser();

            approveButton.Visibility = isPrivileged ? Visibility.Visible : Visibility.Collapsed;
            rejectButton.Visibility = isPrivileged ? Visibility.Visible : Visibility.Collapsed;
            approveConMenu.Visibility = isPrivileged ? Visibility.Visible : Visibility.Collapsed;
            rejectConMenu.Visibility = isPrivileged ? Visibility.Visible : Visibility.Collapsed;

            // Если пользователь не имеет прав на просмотр всех заявок — скрываем фильтр "Мои заявки"
            if (!isPrivileged)
            {
                // Устанавливаем фильтр на "Мои заявки" и скрываем ComboBox
                Filter.SelectedIndex = 4;
                Filter.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadRequests()
        {
            if (listRequests == null)
            {
                return;
            }
            try
            {
                using (var context = new WIS_databaseEntities())
                {
                    var query = context.WIS_Requests
                    .Include("WIS_Assets")
                    .Include("WIS_Assets.WIS_Asset_Types")
                    .Include("WIS_Request_Statuses")
                    .Include("WIS_Users")
                    .AsQueryable();

                    // Ограничение по ролям
                    if (!IsPrivilegedUser())
                    {
                        // Только свои заявки
                        query = query.Where(r => r.request_user_ID == _currentUser.ID_user);
                    }
                    else
                    {
                        // Применение фильтров только для привилегированных пользователей
                        switch (Filter.SelectedIndex)
                        {
                            case 1:
                                query = query.Where(r => r.request_status_ID == 1); // Новые
                                break;
                            case 2:
                                query = query.Where(r => r.request_status_ID == 2); // Одобренные
                                break;
                            case 3:
                                query = query.Where(r => r.request_status_ID == 3); // Отклоненные
                                break;
                            case 4:
                                query = query.Where(r => r.request_user_ID == _currentUser.ID_user); // Мои заявки
                                break;
                        }
                    }

                    // Поиск
                    if (!string.IsNullOrWhiteSpace(Searcher.Text) && Searcher.Text != "Поиск...")
                    {
                        string searchText = Searcher.Text.ToLower();
                        query = query.Where(r =>
                            r.WIS_Assets.asset_name.ToLower().Contains(searchText) ||
                            r.WIS_Assets.asset_serial_number.ToLower().Contains(searchText) ||
                            r.WIS_Users.user_firstname.ToLower().Contains(searchText) ||
                            r.request_note.ToLower().Contains(searchText));
                    }

                    // Сортировка по дате
                    switch (DateFilterType.SelectedIndex)
                    {
                        case 0: // Дата создания
                            query = query.OrderByDescending(r => r.request_date);
                            break;
                        case 1: // Дата одобрения
                            query = query.OrderByDescending(r => r.request_approval_date);
                            break;
                        default:
                            query = query.OrderByDescending(r => r.request_date);
                            break;
                    }

                    listRequests.ItemsSource = query.ToList();

                    UpdateCounter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            Searcher.Text = string.Empty;
            SearcherPlaceHolder.Visibility = Visibility.Visible;
            SearcherPlaceHolder.IsEnabled = true;
            Filter.SelectedIndex = 0;
            DateFilterType.SelectedIndex = 0;
            LoadRequests();
        }

        private void UpdateCounter()
        {
            int total = listRequests.Items.Count;
            Counter.Text = $"Всего: {total}";
        }

        private void Searcher_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadRequests();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRequests();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new RequestCreateWindow(_currentUser);
            createWindow.ShowDialog();
            LoadRequests();
        }

        private void approveButton_Click(object sender, RoutedEventArgs e)
        {
            if (listRequests.SelectedItem is WIS_Requests selectedRequest)
            {
                ChangeRequestStatus(selectedRequest, 2); // 2 - Одобрено
            }
            else
            {
                MessageBox.Show("Выберите заявку для одобрения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void rejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (listRequests.SelectedItem is WIS_Requests selectedRequest)
            {
                ChangeRequestStatus(selectedRequest, 3); // 3 - Отклонено
            }
            else
            {
                MessageBox.Show("Выберите заявку для отклонения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ChangeRequestStatus(WIS_Requests request, int statusId)
        {
            try
            {
                using (var context = new WIS_databaseEntities())
                {
                    var dbRequest = context.WIS_Requests.Find(request.ID_request);
                    if (dbRequest != null)
                    {
                        dbRequest.request_status_ID = statusId;
                        dbRequest.request_approval_date = DateTime.Now;
                        dbRequest.request_approved_by_user_ID = _currentUser.ID_user;

                        // Добавим запись в историю, если заявка одобрена
                        if (statusId == 2) // 2 - Одобрено
                        {
                            var history = new WIS_Asset_Histories
                            {
                                history_asset_ID = dbRequest.request_asset_ID,
                                history_event_date = DateTime.Now,
                                history_event_type = "Заявка одобрена",
                                history_description = $"Заявка #{dbRequest.ID_request} одобрена пользователем {_currentUser.user_firstname}",
                                history_user_ID = _currentUser.ID_user
                            };
                            context.WIS_Asset_Histories.Add(history);
                        }

                        context.SaveChanges();
                        LoadRequests();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void listRequests_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listRequests.SelectedItem is WIS_Requests selectedRequest)
            {
                var detailsWindow = new RequestDetailsWindow(selectedRequest.ID_request, _currentUser);
                detailsWindow.ShowDialog();
                LoadRequests();
            }
        }

        private void bt_go_assets_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AssetPage(_currentUser));
        }

        private void userDisplay_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new UserProfilePage(_currentUser);
            AppFrame.frameMain.Navigate(new UserProfilePage(_currentUser));
            InitializeUI(); // Обновляем интерфейс после возможных изменений
        }

        private void Searcher_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Searcher.Text))
            {
                SearcherPlaceHolder.Visibility = Visibility.Visible;
            }
        }

        private void SearcherPlaceHolder_GotFocus(object sender, RoutedEventArgs e)
        {
            SearcherPlaceHolder.Visibility = Visibility.Collapsed;
            Searcher.Focus();
        }

        private void addConMenu_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new RequestCreateWindow(_currentUser);
            createWindow.ShowDialog();
            LoadRequests();
        }

        private void approveConMenu_Click(object sender, RoutedEventArgs e)
        {
            if (listRequests.SelectedItem is WIS_Requests selectedRequest)
            {
                ChangeRequestStatus(selectedRequest, 2); // Одобрено
            }
        }

        private void rejectConMenu_Click(object sender, RoutedEventArgs e)
        {
            if (listRequests.SelectedItem is WIS_Requests selectedRequest)
            {
                ChangeRequestStatus(selectedRequest, 3); // Отклонено
            }
        }

        private void DateFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRequests();
        }
    }
}