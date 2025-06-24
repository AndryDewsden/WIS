using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WIS.ApplicationData;
using static WIS.RoleManager;

namespace WIS.Pages
{
    public partial class UserProfilePage : Page
    {
        private WIS_Users current_user;
        private UserRole role;

        public UserProfilePage(WIS_Users current_user)
        {
            InitializeComponent();
            this.current_user = current_user;

            if (!Enum.IsDefined(typeof(UserRole), current_user.user_role_ID))
            {
                MessageBox.Show("Неизвестная роль пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.role = (UserRole)current_user.user_role_ID;
            menuListBox.SelectedIndex = 0; // По умолчанию — Пользователи
        }

        private void menuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (menuListBox.SelectedItem is ListBoxItem item)
            {
                switch (item.Tag.ToString())
                {
                    case "Users":
                        if (RoleManager.HasAccess(role, AccessLevel.ITSpecialist))
                            contentFrame.Navigate(new UserManagementPage(current_user));
                        else
                            ShowAccessDenied();
                        break;

                    case "Assets":
                        if (RoleManager.HasAccess(role, AccessLevel.Manager))
                            contentFrame.Navigate(new AssetManagementPage(current_user));
                        else
                            ShowAccessDenied();
                        break;

                    case "History":
                        if (RoleManager.HasAccess(role, AccessLevel.Accountant))
                            contentFrame.Navigate(new AssetHistoryPage());
                        else
                            ShowAccessDenied();
                        break;

                    case "Reports":
                        if (RoleManager.HasAccess(role, AccessLevel.Accountant))
                            contentFrame.Navigate(new ReportsPage(current_user));
                        else
                            ShowAccessDenied();
                        break;

                    case "StickerGenerator":
                        contentFrame.Navigate(new StickerGeneratorPage(current_user));
                        break;

                    case "ExcelImport":
                        if (RoleManager.HasAccess(role, AccessLevel.Administrator))
                            contentFrame.Navigate(new ExcelImportPage(current_user));
                        else
                            ShowAccessDenied();
                        break;
                }
            }
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show("У вас нет доступа к этой странице.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!RoleManager.HasAccess(role, AccessLevel.ITSpecialist))
                HideMenuItem("Users");

            if (!RoleManager.HasAccess(role, AccessLevel.Manager))
                HideMenuItem("Assets");

            if (!RoleManager.HasAccess(role, AccessLevel.Accountant))
            {
                HideMenuItem("History");
                HideMenuItem("Reports");
            }

            if (!RoleManager.HasAccess(role, AccessLevel.Administrator))
                HideMenuItem("ExcelImport");
        }

        private void HideMenuItem(string tag)
        {
            var item = menuListBox.Items.OfType<ListBoxItem>().FirstOrDefault(i => i.Tag?.ToString() == tag);
            if (item != null)
                item.Visibility = Visibility.Collapsed;
        }

        private void Bt_GoBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new AssetPage(current_user));
        }

        private void Bt_goRequest_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new RequestPage(current_user));
        }
    }
}