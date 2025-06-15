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
using System.Windows.Shapes;
using WIS.ApplicationData;
using System.Data.Entity;

namespace WIS.Pages
{

    public partial class RequestCreateWindow : Window
    {
        private readonly WIS_Users _currentUser;

        public RequestCreateWindow(WIS_Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadAssetTypes();
        }

        private void LoadAssetTypes()
        {
            try
            {
                using (var context = new WIS_databaseEntities())
                {
                    var types = context.WIS_Asset_Types.ToList();
                    cbAssetTypes.ItemsSource = types;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов оборудования: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAssets(int? typeId = null)
        {
            try
            {
                using (var context = new WIS_databaseEntities())
                {
                    var requestedAssetIds = context.WIS_Requests
                        .Where(r => r.request_status_ID == 1) // Активные заявки
                        .Select(r => r.request_asset_ID)
                        .Distinct()
                        .ToList();

                    var query = context.WIS_Assets
                        .Include(a => a.WIS_Asset_Locations)
                        .Where(a => a.asset_status_ID == 3); // Только доступные по статусу

                    if (typeId.HasValue)
                        query = query.Where(a => a.asset_type_ID == typeId.Value);

                    var assets = query.ToList();

                    var assetViewModels = assets.Select(a => new AssetViewModel
                    {
                        Asset = a,
                        IsAvailable = !requestedAssetIds.Contains(a.ID_asset)
                    }).ToList();

                    cbAssets.ItemsSource = assetViewModels;
                    cbAssets.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оборудования: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (cbAssets.SelectedItem == null || string.IsNullOrWhiteSpace(txtNote.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedAssetVM = cbAssets.SelectedItem as AssetViewModel;
            if (selectedAssetVM == null || !selectedAssetVM.IsAvailable)
            {
                MessageBox.Show("Выберите доступное оборудование и заполните комментарий.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedAsset = selectedAssetVM.Asset;

            try
            {
                using (var context = new WIS_databaseEntities())
                {
                    // Проверка на дубликат
                    bool duplicateExists = context.WIS_Requests.Any(r =>
                        r.request_asset_ID == selectedAsset.ID_asset &&
                        r.request_user_ID == _currentUser.ID_user &&
                        r.request_status_ID == 1); // Статус "новая"

                    if (duplicateExists)
                    {
                        MessageBox.Show("Вы уже отправили заявку на это оборудование. Дождитесь обработки.", "Предупреждение",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Проверка: есть ли активная заявка на это оборудование от любого пользователя
                    bool assetAlreadyRequested = context.WIS_Requests.Any(r =>
                        r.request_asset_ID == selectedAsset.ID_asset &&
                        r.request_status_ID == 1); // Статус "Ожидает"

                    if (assetAlreadyRequested)
                    {
                        MessageBox.Show("На это оборудование уже подана заявка другим пользователем. Выберите другое.", "Предупреждение",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Создание заявки
                    var request = new WIS_Requests
                    {
                        request_asset_ID = selectedAsset.ID_asset,
                        request_user_ID = _currentUser.ID_user,
                        request_date = DateTime.Now,
                        request_note = txtNote.Text,
                        request_status_ID = 1
                    };

                    context.WIS_Requests.Add(request);
                    context.SaveChanges();

                    MessageBox.Show("Заявка успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заявки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Закрыть окно без сохранения
        }

        private void cbAssetTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAssetTypes.SelectedItem is WIS_Asset_Types selectedType)
            {
                LoadAssets(selectedType.ID_asset_type);

                using (var context = new WIS_databaseEntities())
                {
                    int count = context.WIS_Assets
                        .Count(a => a.asset_type_ID == selectedType.ID_asset_type && a.asset_status_ID == 3);

                    txtAvailableCount.Text = $"Доступно устройств: {count}";
                }
            }
            else
            {
                LoadAssets();
                txtAvailableCount.Text = string.Empty;
            }
        }

        private void cbAssets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAssets.SelectedItem is AssetViewModel selectedAssetVM && selectedAssetVM.IsAvailable)
            {
                var selectedAsset = selectedAssetVM.Asset;

                string location = selectedAsset.WIS_Asset_Locations?.location_name ?? "не указано";

                txtAssetInfo.Text = $"Название: {selectedAsset.asset_name}\n" +
                                    $"Модель: {selectedAsset.asset_model}\n" +
                                    $"Серийный номер: {selectedAsset.asset_serial_number}\n" +
                                    $"Срок гарантии до: {selectedAsset.asset_warranty_expiration_date?.ToShortDateString() ?? "не указано"}\n" +
                                    $"Местоположение: {location}\n";
            }
            else
            {
                txtAssetInfo.Text = string.Empty;
            }
        }
    }
}