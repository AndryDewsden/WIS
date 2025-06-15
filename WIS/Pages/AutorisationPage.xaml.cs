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

namespace WIS.Pages
{
    public partial class AutorisationPage : Page
    {
        public AutorisationPage()
        {
            InitializeComponent();
            InitializePlaceholders();
            EnterEnable();
        }

        private void InitializePlaceholders()
        {
            txbLogin.Visibility = Visibility.Collapsed;
            txbPassword.Visibility = Visibility.Collapsed;
        }

        private void EnterEnable()
        {
            Enter.IsEnabled = !string.IsNullOrEmpty(txbLogin.Text) && !string.IsNullOrEmpty(txbPassword.Password);
        }

        private void HandlePlaceholderFocus(UIElement realControl, UIElement placeholder, bool isGettingFocus)
        {
            realControl.Visibility = isGettingFocus ? Visibility.Visible : Visibility.Collapsed;
            placeholder.Visibility = isGettingFocus ? Visibility.Collapsed : Visibility.Visible;

            if (isGettingFocus)
            {
                realControl.Focus();
            }
        }

        private void txbLogin_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txbLogin.Text))
            {
                HandlePlaceholderFocus(txbLogin, txbLoginPlaceHolder, false);
            }
        }

        private void txbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnterEnable();
        }

        private void txbLoginPlaceHolder_GotFocus(object sender, RoutedEventArgs e)
        {
            HandlePlaceholderFocus(txbLogin, txbLoginPlaceHolder, true);
        }

        private void txbPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txbPassword.Password))
            {
                HandlePlaceholderFocus(txbPassword, txbPasswordPlaceHolder, false);
            }
        }

        private void txbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            EnterEnable();
        }

        private void txbPasswordPlaceHolder_GotFocus(object sender, RoutedEventArgs e)
        {
            HandlePlaceholderFocus(txbPassword, txbPasswordPlaceHolder, true);
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txbLogin.Text.Trim();
                string password = txbPassword.Password.Trim();

                // Хешируем введённый пароль
                string passwordHash = HashHelper.ComputeSha256Hash(password);

                var userObj = AppConnect.Model.WIS_Users.FirstOrDefault(x => x.user_login.Trim().ToLower() == login.ToLower() && BitConverter.ToString(x.user_password_hash).Replace("-", "").ToLower() == passwordHash);

                if (userObj == null)
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Преобразуем ID роли в enum
                var role = (RoleManager.UserRole)userObj.user_role_ID;

                // Приветствие
                string roleName = userObj.RoleName ?? "пользователь";
                string greeting = $"Здравствуйте, {roleName.ToLower()} {userObj.user_login}!";

                // Определение целевой страницы
                Page targetPage;

                switch (role)
                {
                    case RoleManager.UserRole.Administrator:
                        // Администратор — доступ ко всем функциям
                        targetPage = new UserProfilePage(userObj); // пример
                        break;

                    case RoleManager.UserRole.ITSpecialist:
                        // IT-специалист — доступ к управлению оборудованием
                        targetPage = new UserProfilePage(userObj);
                        break;

                    case RoleManager.UserRole.Manager:
                        // Менеджер — доступ к управлению оборудованием
                        targetPage = new AssetPage(userObj);
                        break;

                    case RoleManager.UserRole.Accountant:
                        // Бухгалтер — доступ к отчётам
                        targetPage = new UserProfilePage(userObj);
                        break;

                    case RoleManager.UserRole.User:
                        // Обычный пользователь — доступ к заявкам
                        targetPage = new RequestPage(userObj);
                        break;

                    default:
                        throw new Exception("Неизвестная роль пользователя");
                }

                MessageBox.Show(greeting, "Успешный вход",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                AppFrame.frameMain.Navigate(targetPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации:\n{ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void ExitB_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите закрыть приложение?",
                                       "Подтверждение выхода",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
