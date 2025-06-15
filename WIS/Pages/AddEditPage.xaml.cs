using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WIS.ApplicationData;

namespace WIS.Pages
{
    public partial class AddEditPage : Page
    {
        private readonly WIS_Users _currentUser;
        private WIS_Assets _currentAsset;
        private bool _isEditMode;

        // Для сравнения статуса при сохранении
        private int? _oldStatusId;

        public IQueryable<WIS_Asset_Types> AssetTypes { get; private set; }
        public IQueryable<WIS_Asset_Statuses> AssetStatuses { get; private set; }
        public IQueryable<WIS_Asset_Locations> Locations { get; private set; }
        public IQueryable<WIS_Users> Users { get; private set; }

        public AddEditPage(WIS_Users currentUser, WIS_Assets asset = null)
        {
            InitializeComponent();

            _currentUser = currentUser;
            _currentAsset = asset ?? new WIS_Assets();
            _isEditMode = asset != null;
            Title += _isEditMode ? " (Редактирование)" : " (Добавление)";

            if (!RoleManager.HasAccess((RoleManager.UserRole)_currentUser.user_role_ID, RoleManager.AccessLevel.Manager))
            {
                MessageBox.Show("У вас нет прав для добавления или редактирования оборудования.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }

            LoadData();

            DataContext = this;

            // Привязка объектов к контролам
            tbxAssetName.DataContext = _currentAsset;
            tbxAssetModel.DataContext = _currentAsset;
            tbxSerialNumber.DataContext = _currentAsset;
            cbxAssetType.DataContext = _currentAsset;
            cbxAssetStatus.DataContext = _currentAsset;
            cbxLocation.DataContext = _currentAsset;
            tbxNote.DataContext = _currentAsset;
            dpDisposalDate.DataContext = _currentAsset;
            tbxDisposalReason.DataContext = _currentAsset;
            cbxResponsibleUser.DataContext = _currentAsset;
            cbxUser.DataContext = _currentAsset;

            _oldStatusId = _currentAsset.asset_status_ID;

            UpdateUI();
        }

        private void LoadData()
        {
            try
            {
                var db = AppConnect.Model;

                AssetTypes = db.WIS_Asset_Types;
                AssetStatuses = db.WIS_Asset_Statuses;
                Locations = db.WIS_Asset_Locations;
                Users = db.WIS_Users;

                cbxAssetType.ItemsSource = AssetTypes.ToList();
                cbxAssetStatus.ItemsSource = AssetStatuses.ToList();
                cbxLocation.ItemsSource = Locations.ToList();
                cbxUser.ItemsSource = Users.ToList();
                cbxResponsibleUser.ItemsSource = Users.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUI()
        {
            var selectedStatus = AssetStatuses.FirstOrDefault(s => s.ID_asset_status == _currentAsset.asset_status_ID);

            if (selectedStatus != null && selectedStatus.ID_asset_status == 1) // Списано
            {
                DisposalPanel.Visibility = Visibility.Visible;

                if (!_currentAsset.asset_disposal_date.HasValue)
                    _currentAsset.asset_disposal_date = DateTime.Now;
            }
            else
            {
                DisposalPanel.Visibility = Visibility.Collapsed;

                // Очистка временных полей
                _currentAsset.asset_disposal_date = null;
                _currentAsset.asset_disposal_reason = null;
            }
        }

        private void cbxAssetStatus_DropDownClosed(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            if (AppConnect.Model.WIS_Assets.Any(a => a.asset_serial_number == _currentAsset.asset_serial_number && a.ID_asset != _currentAsset.ID_asset))
            {
                errors.AppendLine("Оборудование с таким серийным номером уже существует.");
            }

            if (string.IsNullOrWhiteSpace(_currentAsset.asset_name))
                errors.AppendLine("Укажите наименование оборудования");
            if (string.IsNullOrWhiteSpace(_currentAsset.asset_model))
                errors.AppendLine("Укажите модель оборудования");
            if (cbxAssetType.SelectedItem == null)
                errors.AppendLine("Выберите тип оборудования");
            if (cbxAssetStatus.SelectedItem == null)
                errors.AppendLine("Выберите статус оборудования");
            if (cbxLocation.SelectedItem == null)
                errors.AppendLine("Выберите местоположение");

            var selectedStatus = AssetStatuses.FirstOrDefault(s => s.ID_asset_status == _currentAsset.asset_status_ID);

            if (selectedStatus?.ID_asset_status == 1)
            {
                if (!_currentAsset.asset_disposal_date.HasValue)
                    errors.AppendLine("Укажите дату списания.");
            }

            if (selectedStatus != null && selectedStatus.ID_asset_status == 2) // Эксплуатируется
            {
                if (cbxUser.SelectedItem == null)
                    errors.AppendLine("Укажите пользователя для статуса 'Эксплуатируется'");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var db = AppConnect.Model;

                bool isNew = !_isEditMode;

                if (isNew)
                {
                    _currentAsset.asset_purchase_date = DateTime.Now;
                    db.WIS_Assets.Add(_currentAsset);
                    db.SaveChanges(); // Получаем ID
                }
                else
                {
                    db.WIS_Assets.Attach(_currentAsset);
                    db.Entry(_currentAsset).State = System.Data.Entity.EntityState.Modified;
                }

                // Обработка списания
                if (selectedStatus != null && selectedStatus.ID_asset_status == 1) // Списано
                {
                    var disposalDate = _currentAsset.asset_disposal_date ?? DateTime.Now;

                    // Проверяем, существует ли уже запись о списании
                    var existingDisposal = db.WIS_Asset_Disposals
                        .FirstOrDefault(d => d.disposal_asset_ID == _currentAsset.ID_asset);

                    if (existingDisposal != null)
                    {
                        // Обновляем существующую запись
                        existingDisposal.disposal_date = disposalDate;
                        existingDisposal.disposal_reason = _currentAsset.asset_disposal_reason;
                        existingDisposal.disposal_user_ID = _currentUser.ID_user;
                        // db.Entry(existingDisposal).State = EntityState.Modified; // Необязательно, если используется отслеживание изменений
                    }
                    else
                    {
                        // Создаём новую запись
                        var disposal = new WIS_Asset_Disposals
                        {
                            disposal_asset_ID = _currentAsset.ID_asset,
                            disposal_user_ID = _currentUser.ID_user,
                            disposal_date = disposalDate,
                            disposal_reason = _currentAsset.asset_disposal_reason
                        };
                        db.WIS_Asset_Disposals.Add(disposal);

                        // Добавляем запись в историю: Списание
                        var disposalHistory = new WIS_Asset_Histories
                        {
                            history_asset_ID = _currentAsset.ID_asset,
                            history_event_date = disposalDate,
                            history_event_type = "Списание",
                            history_description = _currentAsset.asset_disposal_reason,
                            history_user_ID = _currentUser.ID_user
                        };
                        db.WIS_Asset_Histories.Add(disposalHistory);
                    }
                }

                // Добавление истории изменения статуса
                var history = new WIS_Asset_Histories
                {
                    history_asset_ID = _currentAsset.ID_asset,
                    history_event_date = DateTime.Now,
                    history_event_type = $"Статус изменён на '{selectedStatus?.status_name}'",
                    history_description = $"Пользователь {_currentUser.user_firstname} {_currentUser.user_lastname} изменил статус оборудования.",
                    history_user_ID = _currentUser.ID_user
                };
                db.WIS_Asset_Histories.Add(history);

                // Обновляем ID локации и пользователя
                _currentAsset.asset_location_ID = (int)cbxLocation.SelectedValue;
                _currentAsset.asset_user_ID = (int?)cbxUser.SelectedValue;

                db.SaveChanges();

                MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.Navigate(new AssetPage(_currentUser));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Отменить изменения?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                NavigationService.GoBack();
            }
        }

        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AssetPage(_currentUser));
        }
    }
}