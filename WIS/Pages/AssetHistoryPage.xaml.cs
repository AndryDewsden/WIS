using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WIS.ApplicationData;

namespace WIS.Pages
{
    public partial class AssetHistoryPage : Page
    {
        private List<WIS_Asset_Histories> _allHistory;
        private List<WIS_Assets> _allAssets;

        public AssetHistoryPage()
        {
            InitializeComponent();
            LoadAssetsForFilter();
            LoadHistory();
        }

        private void LoadAssetsForFilter()
        {
            try
            {
                _allAssets = AppConnect.Model.WIS_Assets
                    .OrderBy(a => a.asset_name)
                    .ToList();

                AssetFilterComboBox.ItemsSource = _allAssets;
                AssetFilterComboBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки активов: " + ex.Message);
            }
        }

        private void LoadHistory()
        {
            try
            {
                _allHistory = AppConnect.Model.WIS_Asset_Histories
                    .ToList();

                var historyWithDetails = _allHistory
                    .Select(h => new
                    {
                        h.history_event_date,
                        h.history_event_type,
                        h.history_description,
                        AssetName = h.WIS_Assets?.asset_name ?? "Неизвестно",
                        UserName = h.WIS_Users != null
                            ? $"{h.WIS_Users.user_firstname} {h.WIS_Users.user_lastname}"
                            : "Неизвестно"
                    })
                    .OrderByDescending(h => h.history_event_date)
                    .ToList();

                HistoryDataGrid.ItemsSource = historyWithDetails;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории: " + ex.Message);
            }
        }

        private void AssetFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssetFilterComboBox.SelectedItem is WIS_Assets selectedAsset)
            {
                var filtered = _allHistory
                    .Where(h => h.history_asset_ID == selectedAsset.ID_asset)
                    .Select(h => new
                    {
                        h.history_event_date,
                        h.history_event_type,
                        h.history_description,
                        AssetName = h.WIS_Assets?.asset_name ?? "Неизвестно",
                        UserName = h.WIS_Users != null
                            ? $"{h.WIS_Users.user_firstname} {h.WIS_Users.user_lastname}"
                            : "Неизвестно"
                    })
                    .OrderByDescending(h => h.history_event_date)
                    .ToList();

                HistoryDataGrid.ItemsSource = filtered;
            }
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            AssetFilterComboBox.SelectedIndex = -1;
            AssetSearchTextBox.Text = string.Empty;
            LoadHistory();
        }

        private void AssetSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = AssetSearchTextBox.Text.ToLower();

            var filteredAssets = _allAssets
                .Where(a => a.asset_name.ToLower().Contains(searchText))
                .ToList();

            AssetFilterComboBox.ItemsSource = filteredAssets;

            if (filteredAssets.Count == 1)
            {
                AssetFilterComboBox.SelectedItem = filteredAssets.First();
            }
        }
    }
}
