using PdfSharp.Pdf;
using PdfSharp.Fonts;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ZXing.Common;
using ZXing;
using WIS.ApplicationData;

namespace WIS.Pages
{
    public partial class StickerGeneratorPage : Page
    {
        private readonly string LogoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemImages", "icon_black.png");
        private const double MmToPt = 2.83465;
        private const double LabelWidthMm = 60;
        private const double LabelHeightMm = 40;
        private const double LabelWidthPt = LabelWidthMm * MmToPt;
        private const double LabelHeightPt = LabelHeightMm * MmToPt;
        public StickerGeneratorPage(WIS_Users currentUser)
        {
            InitializeComponent();
            GlobalFontSettings.FontResolver = new FontResolver();

            if (!File.Exists(LogoPath))
            {
                MessageBox.Show($"Файл логотипа не найден: {LogoPath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Проверка доступа
            var role = (RoleManager.UserRole)currentUser.user_role_ID;
            if (!RoleManager.HasAccess(role, RoleManager.AccessLevel.Manager))
            {
                MessageBox.Show("У вас нет прав для доступа к генератору наклеек.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Возврат на предыдущую страницу или главную
                AppFrame.frameMain.GoBack();
                return;
            }
        }
        private UIElement CreateStickerPreview(string number)
        {
            double width = 170;
            double height = 110;

            var border = new Border
            {
                Width = width,
                Height = height,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(3),
                Background = Brushes.White
            };

            var canvas = new Canvas
            {
                Width = width,
                Height = height
            };

            double padding = 5;

            // Номер
            var numberText = new TextBlock
            {
                Text = number,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                FontFamily = new FontFamily("Times New Roman")
            };
            Canvas.SetLeft(numberText, padding);
            Canvas.SetTop(numberText, padding);
            canvas.Children.Add(numberText);

            // Поля
            double fieldY = padding + 18 + 4;
            double fieldHeight = 13;

            AddField(canvas, "Модель:", 45, padding, fieldY, width);
            AddField(canvas, "Дата получения:", 85, padding, fieldY + fieldHeight, width);
            AddField(canvas, "Цена:", 35, padding, fieldY + fieldHeight * 2, width);

            // Штрих-код
            var barcodeImage = GenerateBarcodeImage(number);
            if (barcodeImage != null)
            {
                var barcode = new Image
                {
                    Source = barcodeImage,
                    Stretch = Stretch.Uniform,
                    Height = 30
                };
                Canvas.SetLeft(barcode, padding);
                Canvas.SetBottom(barcode, padding);
                canvas.Children.Add(barcode);
            }

            // Логотип
            if (File.Exists(LogoPath))
            {
                try
                {
                    var logoImage = new BitmapImage(new Uri(LogoPath, UriKind.Absolute));
                    var logo = new Image
                    {
                        Source = logoImage,
                        MaxHeight = 30,
                        MaxWidth = 50,
                        Stretch = Stretch.Uniform
                    };
                    Canvas.SetRight(logo, padding);
                    Canvas.SetBottom(logo, padding);
                    canvas.Children.Add(logo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке логотипа: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            border.Child = canvas;

            return new Viewbox
            {
                Child = border,
                Stretch = Stretch.Uniform
            };
        }

        private void AddField(Canvas canvas, string label, double lineOffset, double x, double y, double totalWidth)
        {
            var text = new TextBlock
            {
                Text = label,
                FontSize = 9,
                FontFamily = new FontFamily("Times New Roman")
            };
            Canvas.SetLeft(text, x);
            Canvas.SetTop(text, y);
            canvas.Children.Add(text);

            var line = new Rectangle
            {
                Height = 1,
                Width = totalWidth - x - lineOffset - 5,
                Fill = Brushes.Black
            };
            Canvas.SetLeft(line, x + lineOffset);
            Canvas.SetTop(line, y + 11); // немного ниже текста
            canvas.Children.Add(line);
        }


        private UIElement CreateField(string label, double lineOffset)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 9,
                FontFamily = new FontFamily("Times New Roman")
            });

            var line = new Rectangle
            {
                Height = 1,
                Width = 100,
                Fill = Brushes.Black,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 0, 0, 0)
            };
            panel.Children.Add(line);

            return panel;
        }
        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "";
            
            try
            {
                var input = RangeTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(input))
                {
                    MessageBox.Show("Введите диапазон номеров.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var format = FormatTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(format))
                {
                    MessageBox.Show("Введите формат номера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var numbers = ParseRanges(input);
                if (numbers.Count == 0)
                {
                    MessageBox.Show("Не удалось распознать номера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var formattedNumbers = numbers.Select(n => FormatNumber(n, format)).ToList();

                PreviewPanel.Items.Clear();
                foreach (var number in formattedNumbers.Take(5))
                {
                    var sticker = CreateStickerPreview(number);
                    PreviewPanel.Items.Add(sticker);
                }

                var pdfPath = await Task.Run(() => GeneratePdf(formattedNumbers));

                if (pdfPath == null)
                {
                    return;
                }

                StatusTextBlock.Text = $"Файл сохранён: {pdfPath}";
                MessageBox.Show($"Наклейки сгенерированы и сохранены в:\n{pdfPath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "";
                MessageBox.Show("Ошибка при генерации: " + ex.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ImageSource GenerateBarcodeImage(string number)
        {
            try
            {
                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 40,
                        Width = 120,
                        Margin = 1
                    }
                };

                var pixelData = writer.Write(number);

                using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bmpData.Scan0, pixelData.Pixels.Length);
                    bitmap.UnlockBits(bmpData);

                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Position = 0;

                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        return image;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при генерации штрих-кода: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // Парсинг диапазонов и одиночных значений
        private List<string> ParseRanges(string input)
        {
            var result = new List<string>();
            var parts = input.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var partRaw in parts)
            {
                var part = partRaw.Trim();

                if (part.Contains("-"))
                {
                    var bounds = part.Split('-');
                    if (string.IsNullOrWhiteSpace(bounds[0]) || string.IsNullOrWhiteSpace(bounds[1]))
                    {
                        continue;
                    }

                    var start = bounds[0].Trim();
                    var end = bounds[1].Trim();

                    if (IsPureNumber(start) && IsPureNumber(end))
                    {
                        int s = int.Parse(start);
                        int e = int.Parse(end);
                        if (s > e) (s, e) = (e, s);
                        for (int i = s; i <= e; i++)
                            result.Add(i.ToString());
                    }
                    else if (IsSingleLetter(start) && IsSingleLetter(end))
                    {
                        char s = char.ToUpper(start[0]);
                        char e = char.ToUpper(end[0]);
                        if (s > e) (s, e) = (e, s);
                        for (char c = s; c <= e; c++)
                            result.Add(c.ToString());
                    }
                    else if (TryParseMixedRange(start, end, out var mixedList))
                    {
                        result.AddRange(mixedList);
                    }
                    else
                    {
                        result.Add(part); // нераспознанный диапазон - добавим как есть
                    }
                }
                else
                {
                    result.Add(part);
                }
            }

            return result;
        }

        private bool IsPureNumber(string s) => int.TryParse(s, out _);
        private bool IsSingleLetter(string s) => s.Length == 1 && char.IsLetter(s[0]);

        private bool TryParseMixedRange(string start, string end, out List<string> result)
        {
            result = new List<string>();
            var regex = new Regex(@"^([A-Za-z]*)(\d+)$");
            var m1 = regex.Match(start);
            var m2 = regex.Match(end);

            if (!m1.Success || !m2.Success)
                return false;

            var prefix1 = m1.Groups[1].Value;
            var prefix2 = m2.Groups[1].Value;
            if (!string.Equals(prefix1, prefix2, StringComparison.OrdinalIgnoreCase))
                return false;

            int num1 = int.Parse(m1.Groups[2].Value);
            int num2 = int.Parse(m2.Groups[2].Value);
            if (num1 > num2) (num1, num2) = (num2, num1);

            int digitsCount = m1.Groups[2].Value.Length;
            for (int i = num1; i <= num2; i++)
            {
                var numberPart = i.ToString(new string('0', digitsCount));
                result.Add(prefix1 + numberPart);
            }
            return true;
        }

        // Форматирование номера по маске с '№' - заменяем на цифры номера с ведущими нулями
        private string FormatNumber(string number, string format)
        {
            var digitsOnly = new string(number.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digitsOnly))
            {
                digitsOnly = "0";
            }
            int count = format.Count(c => c == '№');
            if (count == 0)
            {
                return format; // или вернуть number
            }
            var padded = digitsOnly.PadLeft(count, '0');

            var sb = new StringBuilder();
            int index = 0;

            foreach (var ch in format)
            {
                sb.Append(ch == '№' ? (index < padded.Length ? padded[index++] : '0') : ch);
            }

            return sb.ToString();
        }

        // Генерация PDF с наклейками
        private string GeneratePdf(List<string> numbers)
        {
            var pdf = new PdfDocument();
            pdf.Info.Title = "Наклейки инвентаризации";

            const double pageWidth = 595;  // A4 pt
            const double pageHeight = 842;

            // Уменьшенный размер наклейки: 60мм x 35мм
            double labelWidth = 60 * 2.83465;
            double labelHeight = 40 * 2.83465;

            double marginLeft = 20;
            double marginTop = 20;
            double horizontalSpacing = 8;
            double verticalSpacing = 8;

            int labelsPerRow = (int)((pageWidth - 2 * marginLeft + horizontalSpacing) / (labelWidth + horizontalSpacing));
            int labelsPerColumn = (int)((pageHeight - 2 * marginTop + verticalSpacing) / (labelHeight + verticalSpacing));
            int labelsPerPage = labelsPerRow * labelsPerColumn;

            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 30,  // уменьшена высота штрих-кода
                    Width = 130,
                    Margin = 1
                }
            };

            int pageCount = (numbers.Count + labelsPerPage - 1) / labelsPerPage;
            int numberIndex = 0;

            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                var page = pdf.AddPage();
                page.Width = XUnit.FromPoint(pageWidth);
                page.Height = XUnit.FromPoint(pageHeight);
                var gfx = XGraphics.FromPdfPage(page);

                for (int i = 0; i < labelsPerPage && numberIndex < numbers.Count; i++, numberIndex++)
                {
                    int row = i / labelsPerRow;
                    int col = i % labelsPerRow;

                    double x = marginLeft + col * (labelWidth + horizontalSpacing);
                    double y = marginTop + row * (labelHeight + verticalSpacing);

                    DrawLabel(gfx, x, y, labelWidth, labelHeight, numbers[numberIndex], barcodeWriter);
                }
            }

            var downloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            var filename = $"InventoryStickers_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var path = System.IO.Path.Combine(downloads, filename);

            try
            {
                pdf.Save(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении PDF: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return path;
        }


        private void DrawLabel(XGraphics gfx, double x, double y, double width, double height, string number, BarcodeWriterPixelData barcodeWriter)
        {
            var rect = new XRect(x, y, width, height);
            gfx.DrawRectangle(XPens.Black, rect);

            double padding = 5;

            // Шрифт для номера
            var fontNumber = new XFont("Times New Roman", 12);
            gfx.DrawString(number, fontNumber, XBrushes.Black, new XRect(x + padding, y + padding, width - 2 * padding, 18), XStringFormats.TopLeft);

            // Генерация штрих-кода
            var pixelData = barcodeWriter.Write(number);

            using (var ms = new MemoryStream())
            {
                using (var bmp = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                {
                    var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bmpData.Scan0, pixelData.Pixels.Length);
                    bmp.UnlockBits(bmpData);

                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;

                    // Размеры штрих-кода
                    double barcodeHeight = 30;
                    double barcodeY = y + height - padding - barcodeHeight;

                    // Левая часть — штрих-код
                    double barcodeWidth = (width - 2 * padding) * 0.7;
                    double barcodeX = x + padding;

                    using (var barcodeImage = XImage.FromStream(ms))
                    {
                        gfx.DrawImage(barcodeImage, barcodeX, barcodeY, barcodeWidth, barcodeHeight);
                    }

                    // Правая часть — логотип
                    if (LogoExists())
                    {
                        try
                        {
                            using (var logoStream = File.OpenRead(LogoPath))
                            using (var logoImage = XImage.FromStream(logoStream))
                            {
                                double maxLogoWidth = (width - 2 * padding) * 0.3;
                                double maxLogoHeight = barcodeHeight;

                                double originalWidth = logoImage.PixelWidth;
                                double originalHeight = logoImage.PixelHeight;

                                double scale = Math.Min(maxLogoWidth / originalWidth, maxLogoHeight / originalHeight);

                                double logoWidth = originalWidth * scale;
                                double logoHeight = originalHeight * scale;

                                double logoX = x + width - padding - logoWidth;
                                double logoY = barcodeY + (barcodeHeight - logoHeight); // выравнивание по нижнему краю


                                gfx.DrawImage(logoImage, logoX, logoY, logoWidth, logoHeight);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при загрузке логотипа в PDF: " + ex.Message);
                        }
                    }

                }
            }

            var fontFields = new XFont("Times New Roman", 9);
            double fieldsY = y + 22;
            double fieldHeight = 13;

            // Поля "Модель", "Дата получения", "Цена" с линиями
            gfx.DrawString("Модель:", fontFields, XBrushes.Black, new XRect(x + padding, fieldsY, width - 2 * padding, fieldHeight), XStringFormats.TopLeft);
            gfx.DrawLine(XPens.Black, x + padding + 45, fieldsY + fieldHeight - 3, x + width - padding, fieldsY + fieldHeight - 3);

            gfx.DrawString("Дата получения:", fontFields, XBrushes.Black, new XRect(x + padding, fieldsY + fieldHeight, width - 2 * padding, fieldHeight), XStringFormats.TopLeft);
            gfx.DrawLine(XPens.Black, x + padding + 85, fieldsY + fieldHeight * 2 - 3, x + width - padding, fieldsY + fieldHeight * 2 - 3);

            gfx.DrawString("Цена:", fontFields, XBrushes.Black, new XRect(x + padding, fieldsY + fieldHeight * 2, width - 2 * padding, fieldHeight), XStringFormats.TopLeft);
            gfx.DrawLine(XPens.Black, x + padding + 35, fieldsY + fieldHeight * 3 - 3, x + width - padding, fieldsY + fieldHeight * 3 - 3);
        }

        private bool LogoExists() => !string.IsNullOrEmpty(LogoPath) && File.Exists(LogoPath);

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var input = RangeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Введите диапазон номеров.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var format = FormatTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(format))
            {
                MessageBox.Show("Введите формат номера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var numbers = ParseRanges(input);
            if (numbers.Count == 0)
            {
                MessageBox.Show("Не удалось распознать номера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var formattedNumbers = numbers.Select(n => FormatNumber(n, format)).ToList();

            PreviewPanel.Items.Clear();
            foreach (var number in formattedNumbers.Take(5))
            {
                var sticker = CreateStickerPreview(number);
                PreviewPanel.Items.Add(sticker);
            }
        }
    }
}