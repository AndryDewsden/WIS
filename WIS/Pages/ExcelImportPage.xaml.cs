using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using ClosedXML.Excel;

namespace WIS.Pages
{
    public partial class ExcelImportPage : Page
    {
        private readonly Dictionary<int, int> roleHierarchy = new Dictionary<int, int>
{
    { 1, 5 }, // Администратор
    { 5, 4 }, // IT-Специалист
    { 2, 3 }, // Менеджер
    { 4, 2 }, // Бухгалтер
    { 3, 1 }  // Пользователь
};

        WIS_Users currentUser;
        private DataTable excelTable;

        public ExcelImportPage(WIS_Users currentUser)
        {
            InitializeComponent();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            this.currentUser = currentUser;
        }

        private bool CanCreateUserWithRole(int targetRoleId)
        {
            int currentRoleId = currentUser.user_role_ID;

            if (!roleHierarchy.ContainsKey(currentRoleId) || !roleHierarchy.ContainsKey(targetRoleId))
                return false;

            int currentLevel = roleHierarchy[currentRoleId];
            int targetLevel = roleHierarchy[targetRoleId];

            // Администратор может всё, но добавление другого администратора — только с подтверждением
            if (currentRoleId == 1 && targetRoleId == 1)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите добавить другого администратора?\nЭто действие требует подтверждения.",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                return result == MessageBoxResult.Yes;
            }

            // Пользователь не может создать равного или выше
            return currentLevel > targetLevel;
        }

        private readonly Dictionary<string, List<string>> expectedColumns = new Dictionary<string, List<string>>()
{
    { "Пользователи", new List<string> { "user_firstname", "user_lastname", "user_login", "user_password_hash", "user_email", "user_role_ID" } },
    { "Активы", new List<string> { "asset_name", "asset_model", "asset_serial_number", "asset_type_ID", "asset_purchase_date", "asset_purchase_price", "asset_warranty_expiration_date", "asset_note", "asset_status_ID", "asset_location_ID", "asset_user_ID" } },
    { "Заявки", new List<string> { "request_asset_ID", "request_user_ID", "request_date", "request_status_ID", "request_approved_by_user_ID", "request_approval_date", "request_note" } }
};

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            string selectedTable = (comboTargetTable.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedTable) || !expectedColumns.ContainsKey(selectedTable))
            {
                MessageBox.Show("Выберите таблицу для импорта.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var columns = expectedColumns[selectedTable];
            string message = $"Ожидаемые столбцы для таблицы \"{selectedTable}\":\n\n" + string.Join("\n", columns);
            MessageBox.Show(message, "Справка по формату Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BtnLoadExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                using (var stream = File.Open(dlg.FileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        excelTable = result.Tables[0];
                        dataGridExcel.ItemsSource = excelTable.DefaultView;
                    }
                }
            }
        }

        private void BtnSaveToDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (excelTable == null)
            {
                MessageBox.Show("Сначала загрузите Excel файл.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedTable = (comboTargetTable.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedTable))
            {
                MessageBox.Show("Выберите таблицу для импорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!expectedColumns.ContainsKey(selectedTable))
            {
                MessageBox.Show("Неизвестная таблица.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var requiredColumns = expectedColumns[selectedTable];
            var missingColumns = requiredColumns.Where(col => !excelTable.Columns.Contains(col)).ToList();
            if (missingColumns.Any())
            {
                MessageBox.Show("В Excel-файле отсутствуют следующие столбцы:\n" + string.Join("\n", missingColumns), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                switch (selectedTable)
                {
                    case "Пользователи":
                        ImportUsers();
                        break;
                    case "Активы":
                        ImportAssets();
                        break;
                    case "Заявки":
                        ImportRequests();
                        break;
                }

                AppConnect.Model.SaveChanges();
                MessageBox.Show("Данные успешно сохранены в базе.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportUsers()
        {
            foreach (DataRow row in excelTable.Rows)
            {
                string login = row["user_login"].ToString();

                int newRoleId = int.Parse(row["user_role_ID"].ToString());

                if (!CanCreateUserWithRole(newRoleId))
                {
                    MessageBox.Show($"Недостаточно прав для создания пользователя с ролью ID = {newRoleId}.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                var existingUser = AppConnect.Model.WIS_Users.FirstOrDefault(u => u.user_login == login);

                if (existingUser != null)
                {
                    existingUser.user_firstname = row["user_firstname"].ToString();
                    existingUser.user_lastname = row["user_lastname"].ToString();
                    existingUser.user_password_hash = HashHelper.HexStringToByteArray(row["user_password_hash"].ToString());
                    existingUser.user_email = row["user_email"].ToString();
                    existingUser.user_role_ID = newRoleId;
                }
                else
                {
                    var newUser = new WIS_Users
                    {
                        user_firstname = row["user_firstname"].ToString(),
                        user_lastname = row["user_lastname"].ToString(),
                        user_login = login,
                        user_password_hash = HashHelper.HexStringToByteArray(row["user_password_hash"].ToString()),
                        user_email = row["user_email"].ToString(),
                        user_role_ID = newRoleId
                    };
                    AppConnect.Model.WIS_Users.Add(newUser);
                }
            }
        }

        private void ImportAssets()
        {
            foreach (DataRow row in excelTable.Rows)
            {
                var asset = new WIS_Assets
                {
                    asset_name = row["asset_name"].ToString(),
                    asset_model = row["asset_model"].ToString(),
                    asset_serial_number = row["asset_serial_number"].ToString(),
                    asset_type_ID = int.Parse(row["asset_type_ID"].ToString()),
                    asset_purchase_date = DateTime.TryParse(row["asset_purchase_date"].ToString(), out var pd) ? pd : (DateTime?)null,
                    asset_purchase_price = decimal.TryParse(row["asset_purchase_price"].ToString(), out var price) ? price : (decimal?)null,
                    asset_warranty_expiration_date = DateTime.TryParse(row["asset_warranty_expiration_date"].ToString(), out var wd) ? wd : (DateTime?)null,
                    asset_note = row["asset_note"].ToString(),
                    asset_status_ID = int.Parse(row["asset_status_ID"].ToString()),
                    asset_location_ID = int.TryParse(row["asset_location_ID"].ToString(), out var lid) ? lid : (int?)null,
                    asset_user_ID = int.TryParse(row["asset_user_ID"].ToString(), out var uid) ? uid : (int?)null
                };
                AppConnect.Model.WIS_Assets.Add(asset);
            }
        }

        private void ImportRequests()
        {
            foreach (DataRow row in excelTable.Rows)
            {
                var request = new WIS_Requests
                {
                    request_asset_ID = int.Parse(row["request_asset_ID"].ToString()),
                    request_user_ID = int.Parse(row["request_user_ID"].ToString()),
                    request_date = DateTime.Parse(row["request_date"].ToString()),
                    request_status_ID = int.Parse(row["request_status_ID"].ToString()),
                    request_approved_by_user_ID = int.TryParse(row["request_approved_by_user_ID"].ToString(), out var aid) ? aid : (int?)null,
                    request_approval_date = DateTime.TryParse(row["request_approval_date"].ToString(), out var ad) ? ad : (DateTime?)null,
                    request_note = row["request_note"].ToString()
                };
                AppConnect.Model.WIS_Requests.Add(request);
            }
        }

        private void BtnGenerateTemplate_Click(object sender, RoutedEventArgs e)
        {
            string selectedTable = (comboTargetTable.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedTable) || !expectedColumns.ContainsKey(selectedTable))
            {
                MessageBox.Show("Выберите таблицу для генерации шаблона.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var columns = expectedColumns[selectedTable];

                // Создаём Excel-файл
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Шаблон");

                    // Записываем заголовки
                    for (int i = 0; i < columns.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = columns[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                        worksheet.Column(i + 1).AdjustToContents();
                    }

                    // Путь к папке загрузок
                    string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    string fileName = $"Шаблон_{selectedTable}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    string fullPath = System.IO.Path.Combine(downloadsPath, fileName);

                    workbook.SaveAs(fullPath);

                    MessageBox.Show($"Шаблон успешно сохранён:\n{fullPath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации шаблона:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}