using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WIS.ApplicationData;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using PdfSharp.Fonts;
using ClosedXML.Excel;

namespace WIS.Pages
{
    public partial class ReportPage : Page
    {
        private FontResolver _fontResolver = new FontResolver();
        private WIS_Users _currentUser;
        private DateTime? _selectedStartDate;
        private DateTime? _selectedEndDate;

        public ReportPage(WIS_Users user)
        {
            InitializeComponent();
            _fontResolver = new FontResolver();
            GlobalFontSettings.FontResolver = _fontResolver;
            _currentUser = user;
            InitializeUI();
            LoadReportTypes();
            SetDefaultDates();
        }

        private void InitializeUI()
        {
            Title = "Создание отчётов";
            StartDatePicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
            EndDatePicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
        }

        private void LoadReportTypes()
        {
            var reportTypes = AppConnect.Model.WIS_Report_Types.ToList();
            ReportTypeComboBox.ItemsSource = reportTypes;
            ReportTypeComboBox.DisplayMemberPath = "report_type_name";
            ReportTypeComboBox.SelectedValuePath = "ID_report_type";
            ReportTypeComboBox.SelectedIndex = 0;
        }

        private void SetDefaultDates()
        {
            _selectedEndDate = DateTime.Today;
            _selectedStartDate = DateTime.Today.AddMonths(-1);
            StartDatePicker.SelectedDate = _selectedStartDate;
            EndDatePicker.SelectedDate = _selectedEndDate;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStartDate = StartDatePicker.SelectedDate;
            _selectedEndDate = EndDatePicker.SelectedDate;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReportNameTextBox.Text))
            {
                MessageBox.Show("Введите название отчёта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(ReportTypeComboBox.SelectedItem is WIS_Report_Types selectedType))
            {
                MessageBox.Show("Выберите тип отчёта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataTable reportData = null;

            switch (selectedType.ID_report_type)
            {
                case 1:
                    reportData = GenerateAssetsReport();
                    break;
                case 2:
                    reportData = GenerateRequestsReport();
                    break;
                case 3:
                    reportData = GenerateDisposalsReport();
                    break;
                default:
                    MessageBox.Show("Неизвестный тип отчёта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            if (reportData != null)
            {
                PreviewDataGrid.ItemsSource = reportData.DefaultView;
                //SaveReportToDatabase(ReportNameTextBox.Text, selectedType.ID_report_type);
                //MessageBox.Show("Отчёт сформирован", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(ReportNameTextBox.Text) && PreviewDataGrid.ItemsSource != null;
            }
        }

        private void SaveReportToDatabase(string name, int typeId)
        {
            try
            {
                var report = new WIS_Reports
                {
                    report_name = name,
                    report_type_ID = typeId,
                    report_creation_date = DateTime.Now,
                    report_user_ID = _currentUser.ID_user
                };

                AppConnect.Model.WIS_Reports.Add(report);
                AppConnect.Model.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении отчёта в базу данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataTable GenerateAssetsReport()
        {
            var data = new DataTable();

            var query = AppConnect.Model.WIS_Assets
                .Join(AppConnect.Model.WIS_Asset_Types,
                    a => a.asset_type_ID,
                    t => t.ID_asset_type,
                    (a, t) => new { a, t })
                .Join(AppConnect.Model.WIS_Asset_Statuses,
                    at => at.a.asset_status_ID,
                    s => s.ID_asset_status,
                    (at, s) => new { at.a, at.t, s })
                .AsQueryable();

            if (_selectedStartDate.HasValue)
            {
                query = query.Where(x => x.a.asset_purchase_date >= _selectedStartDate);
            }
            if (_selectedEndDate.HasValue)
            {
                query = query.Where(x => x.a.asset_purchase_date <= _selectedEndDate);
            }

            if (StatusFilterComboBox.SelectedValue is int statusId && statusId != -1)
            {
                query = query.Where(x => x.a.asset_status_ID == statusId);
            }
            if (TypeFilterComboBox.SelectedValue is int typeId && typeId != -1)
            {
                query = query.Where(x => x.a.asset_type_ID == typeId);
            }

            var result = query.Select(x => new
            {
                x.a.ID_asset,
                x.a.asset_name,
                x.t.asset_type_name,
                x.s.status_name,
                x.a.asset_purchase_date,
                x.a.asset_purchase_price
            }).ToList();

            data.Columns.Add("ID", typeof(int));
            data.Columns.Add("Название", typeof(string));
            data.Columns.Add("Тип", typeof(string));
            data.Columns.Add("Статус", typeof(string));
            data.Columns.Add("Дата покупки", typeof(DateTime));
            data.Columns.Add("Цена", typeof(decimal));

            foreach (var item in result)
            {
                data.Rows.Add(item.ID_asset, item.asset_name, item.asset_type_name,
                    item.status_name, item.asset_purchase_date, item.asset_purchase_price);
            }

            return data;
        }

        private DataTable GenerateRequestsReport()
        {
            var data = new DataTable();

            var query = AppConnect.Model.WIS_Requests
                .Join(AppConnect.Model.WIS_Assets,
                    r => r.request_asset_ID,
                    a => a.ID_asset,
                    (r, a) => new { r, a })
                .Join(AppConnect.Model.WIS_Users,
                    ra => ra.r.request_user_ID,
                    u => u.ID_user,
                    (ra, u) => new { ra.r, ra.a, u })
                .Join(AppConnect.Model.WIS_Request_Statuses,
                    rau => rau.r.request_status_ID,
                    s => s.ID_request_status,
                    (rau, s) => new { rau.r, rau.a, rau.u, s })
                .AsQueryable();

            if (_selectedStartDate.HasValue)
                query = query.Where(x => x.r.request_date >= _selectedStartDate);
            if (_selectedEndDate.HasValue)
                query = query.Where(x => x.r.request_date <= _selectedEndDate);

            if (StatusFilterComboBox.SelectedValue is int statusId && statusId != -1)
                query = query.Where(x => x.a.asset_status_ID == statusId);
            if (TypeFilterComboBox.SelectedValue is int typeId && typeId != -1)
                query = query.Where(x => x.a.asset_type_ID == typeId);

            var result = query.Select(x => new
            {
                x.r.ID_request,
                x.a.asset_name,
                UserName = x.u.user_firstname + " " + x.u.user_lastname,
                x.r.request_date,
                x.s.request_status_name
            }).ToList();

            data.Columns.Add("ID", typeof(int));
            data.Columns.Add("Оборудование", typeof(string));
            data.Columns.Add("Пользователь", typeof(string));
            data.Columns.Add("Дата заявки", typeof(DateTime));
            data.Columns.Add("Статус", typeof(string));

            foreach (var item in result)
            {
                data.Rows.Add(item.ID_request, item.asset_name, item.UserName,
                    item.request_date, item.request_status_name);
            }

            return data;
        }

        private DataTable GenerateDisposalsReport()
        {
            var data = new DataTable();

            var query = AppConnect.Model.WIS_Asset_Disposals
                .Join(AppConnect.Model.WIS_Assets,
                    d => d.disposal_asset_ID,
                    a => a.ID_asset,
                    (d, a) => new { d, a })
                .Join(AppConnect.Model.WIS_Asset_Types,
                    da => da.a.asset_type_ID,
                    t => t.ID_asset_type,
                    (da, t) => new { da.d, da.a, t })
                .Join(AppConnect.Model.WIS_Users,
                    dat => dat.d.disposal_user_ID,
                    u => u.ID_user,
                    (dat, u) => new { dat.d, dat.a, dat.t, u })
                .AsQueryable();

            if (_selectedStartDate.HasValue)
                query = query.Where(x => x.d.disposal_date >= _selectedStartDate);
            if (_selectedEndDate.HasValue)
                query = query.Where(x => x.d.disposal_date <= _selectedEndDate);
            if (StatusFilterComboBox.SelectedValue is int statusId && statusId != -1)
                query = query.Where(x => x.a.asset_status_ID == statusId);
            if (TypeFilterComboBox.SelectedValue is int typeId && typeId != -1)
                query = query.Where(x => x.a.asset_type_ID == 1);
            TypeFilterComboBox.IsEnabled = false;

            var result = query.Select(x => new
            {
                x.a.ID_asset,
                x.a.asset_name,
                x.t.asset_type_name,
                x.d.disposal_date,
                x.d.disposal_reason,
                UserName = x.u.user_firstname + " " + x.u.user_lastname
            }).ToList();

            data.Columns.Add("ID", typeof(int));
            data.Columns.Add("Название", typeof(string));
            data.Columns.Add("Тип", typeof(string));
            data.Columns.Add("Дата списания", typeof(DateTime));
            data.Columns.Add("Причина", typeof(string));
            data.Columns.Add("Списал", typeof(string));

            foreach (var item in result)
            {
                data.Rows.Add(item.ID_asset, item.asset_name, item.asset_type_name,
                    item.disposal_date, item.disposal_reason, item.UserName);
            }

            return data;
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewDataGrid.ItemsSource == null)
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string fileName = ReportNameTextBox.Text + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
            string filePath = Path.Combine(downloadsPath, fileName);

            try
            {
                var dataView = (DataView)PreviewDataGrid.ItemsSource;
                var dataTable = dataView.ToTable();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Отчёт");

                    // Вставка таблицы с заголовками
                    worksheet.Cell(1, 1).InsertTable(dataTable, "ReportTable", true);

                    // Автоподбор ширины столбцов
                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(filePath);
                }

                MessageBox.Show("Отчёт сохранён в Excel: " + filePath, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте в Excel: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewDataGrid.ItemsSource == null)
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string fileName = ReportNameTextBox.Text + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
            string filePath = Path.Combine(downloadsPath, fileName);

            var document = new Document(PageSize.A4, 40, 40, 60, 40);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                var writer = PdfWriter.GetInstance(document, stream);
                writer.PageEvent = new PdfPageEvents();

                document.Open();

                byte[] fontData = _fontResolver.GetFont("Times New Roman");

                BaseFont baseFont;
                using (var ms = new MemoryStream(fontData))
                {
                    baseFont = BaseFont.CreateFont("Times New Roman.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, true, fontData, null);
                }

                Font titleFont = new Font(baseFont, 20, Font.BOLD);
                Font normalFont = new Font(baseFont, 12, Font.NORMAL);
                Font headerFont = new Font(baseFont, 10, Font.BOLD);
                Font cellFont = new Font(baseFont, 10, Font.NORMAL);

                // --- Титульная страница ---

                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemImages", "icon_black.png");
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(120f, 120f);
                    logo.Alignment = Element.ALIGN_CENTER;
                    logo.SpacingAfter = 30f;
                    document.Add(logo);
                }

                var title = new Paragraph("Отчёт: " + ReportNameTextBox.Text, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                };
                document.Add(title);

                var userInfo = new Paragraph($"Сформировано пользователем: {_currentUser.user_firstname} {_currentUser.user_lastname}", normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(userInfo);

                var dateInfo = new Paragraph("Дата формирования: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(dateInfo);

                string periodText = $"Период отчёта: " +
                    (_selectedStartDate.HasValue ? _selectedStartDate.Value.ToString("dd.MM.yyyy") : "не указан") +
                    " - " +
                    (_selectedEndDate.HasValue ? _selectedEndDate.Value.ToString("dd.MM.yyyy") : "не указан");

                var periodParagraph = new Paragraph(periodText, normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 50
                };
                document.Add(periodParagraph);

                // Отступы для подписи и печати
                var signatureTable = new PdfPTable(2)
                {
                    WidthPercentage = 80,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    SpacingBefore = 50
                };
                signatureTable.SetWidths(new float[] { 1, 1 });

                var signatureCell = new PdfPCell(new Phrase("Подпись: ______________________", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingTop = 20
                };

                var stampCell = new PdfPCell(new Phrase("Печать: ______________________", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingTop = 20
                };

                signatureTable.AddCell(signatureCell);
                signatureTable.AddCell(stampCell);

                document.Add(signatureTable);

                // Новая страница для таблицы с данными
                document.NewPage();

                var table = new PdfPTable(PreviewDataGrid.Columns.Count)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10
                };

                // Заголовки столбцов
                foreach (var column in PreviewDataGrid.Columns)
                {
                    string headerText = column.Header?.ToString() ?? "";
                    var cell = new PdfPCell(new Phrase(headerText, headerFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Данные из DataView
                var dataView = (DataView)PreviewDataGrid.ItemsSource;
                foreach (DataRowView row in dataView)
                {
                    foreach (var item in row.Row.ItemArray)
                    {
                        string text = item?.ToString() ?? "";
                        var cell = new PdfPCell(new Phrase(text, cellFont))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Padding = 4
                        };
                        table.AddCell(cell);
                    }
                }

                document.Add(table);

                // Новая страница с выводом (подпись и печать)
                document.NewPage();

                var footerTable = new PdfPTable(2)
                {
                    WidthPercentage = 80,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    SpacingBefore = 200
                };
                footerTable.SetWidths(new float[] { 1, 1 });

                var footerSignatureCell = new PdfPCell(new Phrase("Подпись: ______________________", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingTop = 20
                };

                var footerStampCell = new PdfPCell(new Phrase("Печать: ______________________", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingTop = 20
                };

                footerTable.AddCell(footerSignatureCell);
                footerTable.AddCell(footerStampCell);

                document.Add(footerTable);

                document.Close();
            }

            MessageBox.Show("Отчёт сохранён в PDF: " + filePath, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ReportNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(ReportNameTextBox.Text) && PreviewDataGrid.ItemsSource != null;
        }

        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ReportTypeComboBox.SelectedItem is WIS_Report_Types selectedType))
            {
                return;
            }

            if (selectedType.ID_report_type == 3)
            {
                TypeFilterComboBox.IsEnabled = false;
                TypeFilterComboBox.SelectedIndex = 0;
            }
            else
            {
                TypeFilterComboBox.IsEnabled = true;
            }

            UpdateFilters();
        }

        private void UpdateFilters()
        {
            if (!(ReportTypeComboBox.SelectedItem is WIS_Report_Types selectedType))
                return;

            // Статусы
            if (selectedType.ID_report_type == 1)
            {
                var statuses = AppConnect.Model.WIS_Asset_Statuses.ToList();
                statuses.Insert(0, new WIS_Asset_Statuses { ID_asset_status = -1, status_name = "Любой статус" });
                StatusFilterComboBox.ItemsSource = statuses;
                StatusFilterComboBox.DisplayMemberPath = "status_name";
                StatusFilterComboBox.SelectedValuePath = "ID_asset_status";
                StatusFilterComboBox.SelectedIndex = 0;
            }
            else if (selectedType.ID_report_type == 2)
            {
                var statuses = AppConnect.Model.WIS_Request_Statuses.ToList();
                statuses.Insert(0, new WIS_Request_Statuses { ID_request_status = -1, request_status_name = "Любой статус" });
                StatusFilterComboBox.ItemsSource = statuses;
                StatusFilterComboBox.DisplayMemberPath = "request_status_name";
                StatusFilterComboBox.SelectedValuePath = "ID_request_status";
                StatusFilterComboBox.SelectedIndex = 0;
            }
            else
            {
                StatusFilterComboBox.ItemsSource = null;
            }

            // Типы оборудования
            var types = AppConnect.Model.WIS_Asset_Types.ToList();
            types.Insert(0, new WIS_Asset_Types { ID_asset_type = -1, asset_type_name = "Все типы" });
            TypeFilterComboBox.ItemsSource = types;
            TypeFilterComboBox.DisplayMemberPath = "asset_type_name";
            TypeFilterComboBox.SelectedValuePath = "ID_asset_type";
            TypeFilterComboBox.SelectedIndex = 0;
        }

        private void DateRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(DateRangeComboBox.SelectedItem is ComboBoxItem selectedItem))
            {
                return;
            }

            if (StartDatePicker == null || EndDatePicker == null)
            {
                return;
            }

            var today = DateTime.Today;

            switch (selectedItem.Content.ToString())
            {
                case "Сегодня":
                    StartDatePicker.SelectedDate = today;
                    EndDatePicker.SelectedDate = today;
                    break;
                case "Неделя":
                    StartDatePicker.SelectedDate = today.AddDays(-7);
                    EndDatePicker.SelectedDate = today;
                    break;
                case "Месяц":
                    StartDatePicker.SelectedDate = today.AddMonths(-1);
                    EndDatePicker.SelectedDate = today;
                    break;
                case "Квартал":
                    StartDatePicker.SelectedDate = today.AddMonths(-3);
                    EndDatePicker.SelectedDate = today;
                    break;
                case "Год":
                    StartDatePicker.SelectedDate = today.AddYears(-1);
                    EndDatePicker.SelectedDate = today;
                    break;
                default:
                    break;
            }
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            ReportNameTextBox.Text = string.Empty;
            ReportTypeComboBox.SelectedIndex = 0;
            DateRangeComboBox.SelectedIndex = 5; // "Произвольный"
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            StatusFilterComboBox.SelectedIndex = -1;
            TypeFilterComboBox.SelectedIndex = -1;
            PreviewDataGrid.ItemsSource = null;
            ResultsInfoTextBlock.Text = string.Empty;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Здесь будет справка по созданию и экспорту отчётов.\n\n" +
                            "1. Укажите название отчёта.\n" +
                            "2. Выберите тип отчёта.\n" +
                            "3. Установите диапазон дат и фильтры.\n" +
                            "4. Нажмите 'Сформировать'.\n" +
                            "5. Используйте кнопки 'PDF' или 'Excel' для экспорта.\n" +
                            "6. Кнопка 'Сохранить отчёт' сохраняет информацию в базу данных.",
                            "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReportNameTextBox.Text))
            {
                MessageBox.Show("Введите название отчёта перед сохранением.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PreviewDataGrid.ItemsSource == null)
            {
                MessageBox.Show("Нет данных для сохранения. Сначала сформируйте отчёт.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var report = new WIS_Reports
                {
                    report_name = ReportNameTextBox.Text,
                    report_type_ID = (int)(ReportTypeComboBox.SelectedValue ?? 0),
                    report_creation_date = DateTime.Now,
                    report_user_ID = _currentUser.ID_user
                };

                AppConnect.Model.WIS_Reports.Add(report);
                AppConnect.Model.SaveChanges();

                MessageBox.Show("Параметры отчёта успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении отчёта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}