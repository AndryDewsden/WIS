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
using WIS;
using static WIS.RoleManager;

namespace WIS.Pages
{
    public partial class UserManagementPage : Page
    {
        private WIS_Users CurrentUser;
        public UserManagementPage(WIS_Users currentUser)
        {
            InitializeComponent();

            if (currentUser == null)
            {
                MessageBox.Show("Ошибка: текущий пользователь не задан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.IsEnabled = false;
                return;
            }

            CurrentUser = currentUser;

            if (!HasAccessToPage())
            {
                MessageBox.Show("У вас нет доступа к этой странице.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.IsEnabled = false;
                return;
            }
            LoadUsers();
            UpdateUIAccess();
            LoadRoles();
        }

        private void LoadRoles()
        {
            using (var context = new WIS_databaseEntities())
            {
                var roles = context.WIS_User_Roles.ToList();
                RoleComboBox.ItemsSource = roles;
            }
        }

        private bool HasAccessToPage()
        {
            if (CurrentUser == null)
            {
                return false;
            }

            return RoleManager.HasAccess((UserRole)CurrentUser.user_role_ID, AccessLevel.Manager);
        }

        private void UpdateUIAccess()
        {
            // По умолчанию блокируем все кнопки и поля редактирования
            AddUserButton.IsEnabled = false;
            UpdateUserButton.IsEnabled = false;
            DeleteUserButton.IsEnabled = false;

            FirstNameTextBox.IsEnabled = false;
            LastNameTextBox.IsEnabled = false;
            LoginTextBox.IsEnabled = false;
            EmailTextBox.IsEnabled = false;
            PasswordBox.IsEnabled = false;
            RoleComboBox.IsEnabled = false;
            DepartmentTextBox.IsEnabled = false;

            var role = (UserRole)CurrentUser.user_role_ID;

            // IT-Специалист (5) — только просмотр, ничего не включаем
            if (role == UserRole.ITSpecialist)
            {
                return;
            }

            // Администратор (1) и Менеджер (2) могут добавлять пользователей с ролью ниже своей
            if (role == UserRole.Administrator || role == UserRole.Manager)
            {
                AddUserButton.IsEnabled = true;

                // Для редактирования и удаления — зависит от выбранного пользователя
                UpdateUserButton.IsEnabled = false;
                DeleteUserButton.IsEnabled = false;
            }

            // Пользователь (3) — не может ничего делать (только просмотр)
            if (CurrentUser.user_role_ID == 3)
            {
                // Только просмотр, ничего включаем
                return;
            }
        }

        private void LoadUsers()
        {
            try
            {
                // Загрузка пользователей из базы
                var usersList = AppConnect.Model.WIS_Users.ToList();
                UsersDataGrid.ItemsSource = usersList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            LoginTextBox.Text = "";
            EmailTextBox.Text = "";
            PasswordBox.Password = "";
            RoleComboBox.SelectedIndex = 0;
            DepartmentTextBox.Text = "";
            UsersDataGrid.SelectedItem = null;
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as WIS_Users;

            if (selectedUser != null)
            {
                FirstNameTextBox.Text = selectedUser.user_firstname;
                LastNameTextBox.Text = selectedUser.user_lastname;
                LoginTextBox.Text = selectedUser.user_login;
                EmailTextBox.Text = selectedUser.user_email;
                PasswordBox.Password = "";
                RoleComboBox.SelectedIndex = selectedUser.user_role_ID;
                DepartmentTextBox.Text = selectedUser.user_department;

                // Проверяем права на редактирование выбранного пользователя
                bool canEdit = CanEditUser(selectedUser);
                UpdateUserButton.IsEnabled = canEdit;
                DeleteUserButton.IsEnabled = canEdit;

                FirstNameTextBox.IsEnabled = canEdit;
                LastNameTextBox.IsEnabled = canEdit;
                LoginTextBox.IsEnabled = canEdit;
                EmailTextBox.IsEnabled = canEdit;
                PasswordBox.IsEnabled = canEdit;
                RoleComboBox.IsEnabled = canEdit;
                DepartmentTextBox.IsEnabled = canEdit;
            }
            else
            {
                ClearForm();
                UpdateUIAccess();
            }
        }

        private bool CanEditUser(WIS_Users user)
        {
            // Редактировать можно только пользователей с ролью ниже текущего пользователя (число роли больше)
            // И только если текущий пользователь имеет право на редактирование (т.е. не IT-специалист и не бухгалтер)
            var currentRole = (UserRole)CurrentUser.user_role_ID;
            var targetRole = (UserRole)user.user_role_ID;

            if (currentRole == UserRole.ITSpecialist || currentRole == UserRole.Accountant)
                return false;

            return RoleManager.GetRoleLevel(targetRole) < RoleManager.GetRoleLevel(currentRole);
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
                {
                    MessageBox.Show("Введите логин пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existingUser = AppConnect.Model.WIS_Users.FirstOrDefault(u => u.user_login == LoginTextBox.Text);
                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(RoleComboBox.Text, out int roleId))
                {
                    MessageBox.Show("Введите корректный ID роли.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var currentRole = (UserRole)CurrentUser.user_role_ID;
                var newUserRole = (UserRole)roleId;

                if (RoleManager.GetRoleLevel(newUserRole) >= RoleManager.GetRoleLevel(currentRole))
                {
                    MessageBox.Show("Вы не можете добавить пользователя с ролью выше или равной вашей.");
                    return;
                }

                if (string.IsNullOrEmpty(PasswordBox.Password))
                {
                    MessageBox.Show("Введите пароль пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newUser = new WIS_Users()
                {
                    user_firstname = FirstNameTextBox.Text,
                    user_lastname = LastNameTextBox.Text,
                    user_login = LoginTextBox.Text,
                    user_email = EmailTextBox.Text,
                    user_password_hash = HashHelper.ComputeSha256HashBytes(PasswordBox.Password),
                    user_role_ID = roleId,
                    user_department = DepartmentTextBox.Text
                };

                AppConnect.Model.WIS_Users.Add(newUser);
                AppConnect.Model.SaveChanges();

                MessageBox.Show("Пользователь добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as WIS_Users;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var currentRole = (UserRole)CurrentUser.user_role_ID;
                var targetRole = (UserRole)selectedUser.user_role_ID;

                if (RoleManager.GetRoleLevel(targetRole) >= RoleManager.GetRoleLevel(currentRole))
                {
                    MessageBox.Show("Вы не можете обновлять пользователя с ролью выше или равной вашей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(RoleComboBox.Text, out int roleId))
                {
                    MessageBox.Show("Введите корректный ID роли.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrEmpty(PasswordBox.Password))
                {
                    selectedUser.user_password_hash = HashHelper.ComputeSha256HashBytes(PasswordBox.Password);
                }

                selectedUser.user_firstname = FirstNameTextBox.Text;
                selectedUser.user_lastname = LastNameTextBox.Text;
                selectedUser.user_login = LoginTextBox.Text;
                selectedUser.user_email = EmailTextBox.Text;
                selectedUser.user_role_ID = roleId;
                selectedUser.user_department = DepartmentTextBox.Text;

                AppConnect.Model.SaveChanges();

                MessageBox.Show("Пользователь обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as WIS_Users;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentRole = (UserRole)CurrentUser.user_role_ID;
            var targetRole = (UserRole)selectedUser.user_role_ID;

            if (RoleManager.GetRoleLevel(targetRole) >= RoleManager.GetRoleLevel(currentRole))
            {
                MessageBox.Show("Вы не можете удалять пользователя с ролью выше или равной вашей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя {selectedUser.user_login}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var db = AppConnect.Model;

                // 1. Обновляем активы, связанные с пользователем
                var assets = db.WIS_Assets.Where(a => a.asset_user_ID == selectedUser.ID_user).ToList();

                // Получаем ID статуса "Эксплуатируется" и "Свободен"
                var statusExploited = db.WIS_Asset_Statuses.FirstOrDefault(s => s.status_name.ToLower() == "Эксплуатируется");
                var statusFree = db.WIS_Asset_Statuses.FirstOrDefault(s => s.status_name.ToLower() == "Свободно");

                foreach (var asset in assets)
                {
                    asset.asset_user_ID = null;

                    // Если статус "Эксплуатируется", меняем на "Свободен"
                    if (statusExploited != null && asset.asset_status_ID == statusExploited.ID_asset_status)
                    {
                        if (statusFree != null)
                            asset.asset_status_ID = statusFree.ID_asset_status;
                    }
                }

                // 2. Удаляем связанные записи в других таблицах (кроме активов)
                // Пример: заявки (WIS_Requests), списания (WIS_Asset_Disposals), истории (WIS_Asset_Histories), отчёты (WIS_Reports)

                var requests = db.WIS_Requests.Where(r => r.request_user_ID == selectedUser.ID_user || r.request_approved_by_user_ID == selectedUser.ID_user).ToList();
                db.WIS_Requests.RemoveRange(requests);

                var disposals = db.WIS_Asset_Disposals.Where(d => d.disposal_user_ID == selectedUser.ID_user).ToList();
                db.WIS_Asset_Disposals.RemoveRange(disposals);

                var histories = db.WIS_Asset_Histories.Where(h => h.history_user_ID == selectedUser.ID_user).ToList();
                db.WIS_Asset_Histories.RemoveRange(histories);

                var reports = db.WIS_Reports.Where(r => r.report_user_ID == selectedUser.ID_user).ToList();
                db.WIS_Reports.RemoveRange(reports);

                // 3. Удаляем самого пользователя
                db.WIS_Users.Remove(selectedUser);

                db.SaveChanges();

                MessageBox.Show("Пользователь удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
