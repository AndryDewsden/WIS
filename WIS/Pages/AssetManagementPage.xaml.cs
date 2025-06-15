using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WIS.ApplicationData;

namespace WIS.Pages
{
    public partial class AssetManagementPage : Page
    {
        private WIS_Users currentuser;

        public AssetManagementPage(WIS_Users currentuser)
        {
            InitializeComponent();
            this.currentuser = currentuser;
            LoadAssets();
            LoadComboBoxes();
        }

        private void LoadAssets()
        {
            AssetsDataGrid.ItemsSource = AppConnect.Model.WIS_Assets.ToList();
        }

        private void LoadComboBoxes()
        {
            TypeComboBox.ItemsSource = AppConnect.Model.WIS_Asset_Types.ToList();
            StatusComboBox.ItemsSource = AppConnect.Model.WIS_Asset_Statuses.ToList();
            LocationComboBox.ItemsSource = AppConnect.Model.WIS_Asset_Locations.ToList();
            UserComboBox.ItemsSource = AppConnect.Model.WIS_Users.ToList();
        }

        private void ClearForm()
        {
            NameTextBox.Text = "";
            ModelTextBox.Text = "";
            SerialTextBox.Text = "";
            TypeComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = -1;
            LocationComboBox.SelectedIndex = -1;
            UserComboBox.SelectedIndex = -1;
            PurchaseDatePicker.SelectedDate = null;
            PriceTextBox.Text = "";
            WarrantyDatePicker.SelectedDate = null;
            NoteTextBox.Text = "";
            AssetsDataGrid.SelectedItem = null;
        }

        private void AssetsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var asset = AssetsDataGrid.SelectedItem as WIS_Assets;
            if (asset != null)
            {
                NameTextBox.Text = asset.asset_name;
                ModelTextBox.Text = asset.asset_model;
                SerialTextBox.Text = asset.asset_serial_number;
                TypeComboBox.SelectedValue = asset.asset_type_ID;
                StatusComboBox.SelectedValue = asset.asset_status_ID;
                LocationComboBox.SelectedValue = asset.asset_location_ID;
                UserComboBox.SelectedValue = asset.asset_user_ID;
                PurchaseDatePicker.SelectedDate = asset.asset_purchase_date;
                PriceTextBox.Text = asset.asset_purchase_price?.ToString("F2");
                WarrantyDatePicker.SelectedDate = asset.asset_warranty_expiration_date;
                NoteTextBox.Text = asset.asset_note;
            }
        }

        private void AddAssetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var asset = new WIS_Assets
                {
                    asset_name = NameTextBox.Text,
                    asset_model = ModelTextBox.Text,
                    asset_serial_number = SerialTextBox.Text,
                    asset_type_ID = (int)TypeComboBox.SelectedValue,
                    asset_status_ID = (int)StatusComboBox.SelectedValue,
                    asset_location_ID = (int?)LocationComboBox.SelectedValue,
                    asset_user_ID = (int?)UserComboBox.SelectedValue,
                    asset_purchase_date = PurchaseDatePicker.SelectedDate,
                    asset_purchase_price = decimal.TryParse(PriceTextBox.Text, out var price) ? price : (decimal?)null,
                    asset_warranty_expiration_date = WarrantyDatePicker.SelectedDate,
                    asset_note = NoteTextBox.Text
                };

                AppConnect.Model.WIS_Assets.Add(asset);
                AppConnect.Model.SaveChanges();

                AddHistory(asset.ID_asset, "Добавление", "Актив добавлен");

                MessageBox.Show("Актив добавлен.");
                LoadAssets();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message);
            }
        }

        private void UpdateAssetButton_Click(object sender, RoutedEventArgs e)
        {
            var asset = AssetsDataGrid.SelectedItem as WIS_Assets;
            if (asset == null)
            {
                MessageBox.Show("Выберите актив.");
                return;
            }

            try
            {
                asset.asset_name = NameTextBox.Text;
                asset.asset_model = ModelTextBox.Text;
                asset.asset_serial_number = SerialTextBox.Text;
                asset.asset_type_ID = (int)TypeComboBox.SelectedValue;
                asset.asset_status_ID = (int)StatusComboBox.SelectedValue;
                asset.asset_location_ID = (int?)LocationComboBox.SelectedValue;
                asset.asset_user_ID = (int?)UserComboBox.SelectedValue;
                asset.asset_purchase_date = PurchaseDatePicker.SelectedDate;
                asset.asset_purchase_price = decimal.TryParse(PriceTextBox.Text, out var price) ? price : (decimal?)null;
                asset.asset_warranty_expiration_date = WarrantyDatePicker.SelectedDate;
                asset.asset_note = NoteTextBox.Text;

                // Получаем ID статуса "Списано"
                var disposalStatus = AppConnect.Model.WIS_Asset_Statuses
                    .FirstOrDefault(s => s.status_name.ToLower() == "списано");

                if (disposalStatus != null && asset.asset_status_ID == disposalStatus.ID_asset_status)
                {
                    // Проверяем, есть ли уже запись о списании
                    var existingDisposal = AppConnect.Model.WIS_Asset_Disposals
                        .FirstOrDefault(d => d.disposal_asset_ID == asset.ID_asset);

                    if (existingDisposal != null)
                    {
                        // Обновляем существующую запись
                        existingDisposal.disposal_date = DateTime.Now;
                        existingDisposal.disposal_reason = asset.asset_note; // или отдельное поле, если есть
                        existingDisposal.disposal_user_ID = currentuser.ID_user;

                        // Не добавляем запись в историю
                    }
                    else
                    {
                        // Создаём новую запись о списании
                        var newDisposal = new WIS_Asset_Disposals
                        {
                            disposal_asset_ID = asset.ID_asset,
                            disposal_date = DateTime.Now,
                            disposal_reason = asset.asset_note, // или отдельное поле
                            disposal_user_ID = currentuser.ID_user
                        };

                        AppConnect.Model.WIS_Asset_Disposals.Add(newDisposal);

                        // Добавляем запись в историю только при первом списании
                        AddHistory(asset.ID_asset, "Списание", "Актив списан");
                    }

                    AppConnect.Model.SaveChanges();
                }

                AddHistory(asset.ID_asset, "Обновление", "Актив обновлён");

                MessageBox.Show("Актив обновлён.");
                LoadAssets();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления: " + ex.Message);
            }
        }

        private void DeleteAssetButton_Click(object sender, RoutedEventArgs e)
        {
            var asset = AssetsDataGrid.SelectedItem as WIS_Assets;
            if (asset == null)
            {
                MessageBox.Show("Выберите актив.");
                return;
            }

            var result = MessageBox.Show("Удалить актив и связанные записи?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var histories = AppConnect.Model.WIS_Asset_Histories.Where(h => h.history_asset_ID == asset.ID_asset).ToList();
                AppConnect.Model.WIS_Asset_Histories.RemoveRange(histories);

                var requests = AppConnect.Model.WIS_Requests.Where(r => r.request_asset_ID == asset.ID_asset).ToList();
                AppConnect.Model.WIS_Requests.RemoveRange(requests);

                var disposals = AppConnect.Model.WIS_Asset_Disposals.Where(d => d.disposal_asset_ID == asset.ID_asset).ToList();
                AppConnect.Model.WIS_Asset_Disposals.RemoveRange(disposals);

                AppConnect.Model.WIS_Assets.Remove(asset);
                AppConnect.Model.SaveChanges();

                MessageBox.Show("Актив и связанные записи удалены.");
                LoadAssets();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void AddHistory(int assetId, string eventType, string description)
        {
            var history = new WIS_Asset_Histories
            {
                history_asset_ID = assetId,
                history_event_date = DateTime.Now,
                history_event_type = eventType,
                history_description = description,
                history_user_ID = currentuser.ID_user
            };

            AppConnect.Model.WIS_Asset_Histories.Add(history);
            AppConnect.Model.SaveChanges();
        }
    }
}
