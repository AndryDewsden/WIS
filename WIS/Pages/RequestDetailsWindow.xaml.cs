using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using WIS.ApplicationData;

namespace WIS.Pages
{
    public partial class RequestDetailsWindow : Window
    {
        private WIS_Users _currentUser;
        private WIS_Requests _request;

        public RequestDetailsWindow(int requestId, WIS_Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            using (var context = new WIS_databaseEntities())
            {
                _request = context.WIS_Requests
                    .Include("WIS_Assets")
                    .Include("WIS_Assets.WIS_Asset_Types")
                    .Include("WIS_Assets.WIS_Asset_Locations")
                    .Include("WIS_Request_Statuses")
                    .Include("WIS_Users")
                    .FirstOrDefault(r => r.ID_request == requestId);
            }

            if (_request == null)
            {
                MessageBox.Show("Заявка не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            DataContext = _request;

            ConfigureUI();
        }

        private void ConfigureUI()
        {
            bool isAdmin = _currentUser.user_role_ID == 1 || _currentUser.user_role_ID == 2 || _currentUser.user_role_ID == 5;
            bool isOwner = _currentUser.ID_user == _request.request_user_ID;

            // Панель с кнопками видна только для админа или владельца
            adminPanel.Visibility = (isAdmin || isOwner) ? Visibility.Visible : Visibility.Collapsed;

            // Кнопки "Одобрить" и "Отклонить" — только для админа
            btnApprove.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            btnReject.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            // Кнопка "Удалить" — для владельца или админа
            btnDelete.Visibility = (isAdmin || isOwner) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadRequestDetails()
        {
            try
            {
                using (var context = new WIS_databaseEntities())
                {
                    var request = context.WIS_Requests
                        .Include(r => r.WIS_Assets)
                        .Include(r => r.WIS_Users)
                        .Include(r => r.WIS_Request_Statuses)
                        .FirstOrDefault(r => r.ID_request == _request.ID_request);

                    if (request != null)
                    {
                        DataContext = request;
                    }
                    else
                    {
                        MessageBox.Show("Заявка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            UpdateRequestStatus(2); // Одобрено
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            UpdateRequestStatus(3); // Отклонено
        }

        private void UpdateRequestStatus(int newStatusId)
        {
            try
            {
                using (var context = new WIS_databaseEntities())
                {
                    var request = context.WIS_Requests.FirstOrDefault(r => r.ID_request == _request.ID_request);
                    if (request != null)
                    {
                        request.request_status_ID = newStatusId;
                        request.request_approval_date = DateTime.Now;
                        request.request_approved_by_user_ID = _currentUser.ID_user;

                        // Если заявка одобрена, отклонить другие заявки на то же оборудование
                        if (newStatusId == 2) // Одобрено
                        {
                            int assetId = request.request_asset_ID;

                            var otherPendingRequests = context.WIS_Requests
                                .Where(r =>
                                    r.ID_request != request.ID_request &&
                                    r.request_asset_ID == assetId &&
                                    r.request_status_ID == 1) // Ожидает
                                .ToList();

                            foreach (var otherRequest in otherPendingRequests)
                            {
                                otherRequest.request_status_ID = 3; // Отклонено
                                otherRequest.request_approval_date = DateTime.Now;
                                otherRequest.request_approved_by_user_ID = _currentUser.ID_user;
                            }
                        }

                        context.SaveChanges();

                        MessageBox.Show("Статус заявки обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Заявка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить эту заявку?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new WIS_databaseEntities())
                {
                    var request = context.WIS_Requests.FirstOrDefault(r => r.ID_request == _request.ID_request);
                    if (request != null)
                    {
                        context.WIS_Requests.Remove(request);
                        context.SaveChanges();

                        MessageBox.Show("Заявка успешно удалена.", "Удалено", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Заявка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
