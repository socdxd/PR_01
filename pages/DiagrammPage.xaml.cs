using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using WPF_Payment_Project.Models;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace WPF_Payment_Project.Pages
{
    public partial class DiagrammPage : Page
    {
        private Entities _context;

        public DiagrammPage()
        {
            InitializeComponent();
            _context = Entities.GetContext();
            LoadData();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            CmbUser.ToolTip = "Выберите пользователя для отображения статистики";
            CmbDiagram.ToolTip = "Выберите тип диаграммы";
            BtnExportWord.ToolTip = "Экспорт отчета в Microsoft Word";
            BtnExportExcel.ToolTip = "Экспорт отчета в Microsoft Excel";
        }

        private void LoadData()
        {
            CmbUser.ItemsSource = _context.Users.ToList();
            CmbUser.DisplayMemberPath = "FIO";

            CmbDiagram.Items.Add(SeriesChartType.Column);
            CmbDiagram.Items.Add(SeriesChartType.Pie);
            CmbDiagram.Items.Add(SeriesChartType.Line);
            CmbDiagram.Items.Add(SeriesChartType.Bar);
            CmbDiagram.Items.Add(SeriesChartType.Area);
            CmbDiagram.Items.Add(SeriesChartType.Doughnut);
            CmbDiagram.SelectedIndex = 0;

            if (CmbUser.Items.Count > 0)
            {
                CmbUser.SelectedIndex = 0;
            }
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem == null || CmbDiagram.SelectedItem == null)
                return;

            var selectedUser = CmbUser.SelectedItem as Users;
            var chartType = (SeriesChartType)CmbDiagram.SelectedItem;

            var payments = _context.Payment
                .Where(p => p.UserID == selectedUser.ID)
                .GroupBy(p => p.Category.Name)
                .Select(g => new {
                    Category = g.Key,
                    Total = g.Sum(p => p.Price * p.Num)
                })
                .ToList();

            ChartPayments.Series.Clear();
            ChartPayments.ChartAreas.Clear();
            ChartPayments.Legends.Clear();

            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "Категории";
            chartArea.AxisY.Title = "Сумма (руб.)";
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.LabelStyle.Angle = -45;
            ChartPayments.ChartAreas.Add(chartArea);

            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Top;
            ChartPayments.Legends.Add(legend);

            Series series = new Series
            {
                Name = "Платежи по категориям",
                IsVisibleInLegend = true,
                ChartType = chartType,
                ChartArea = "MainArea",
                Legend = "MainLegend"
            };

            foreach (var payment in payments)
            {
                var point = series.Points.AddXY(payment.Category, payment.Total);
                series.Points[point].Label = $"{payment.Total:N2} руб.";
                series.Points[point].ToolTip = $"{payment.Category}: {payment.Total:N2} руб.";
            }

            ChartPayments.Series.Add(series);
            ChartPayments.Titles.Clear();
            ChartPayments.Titles.Add($"Статистика платежей: {selectedUser.FIO}");
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.Users.ToList();
                var allCategories = _context.Category.ToList();

                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (var user in allUsers)
                {
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userParagraph.set_Style("Заголовок");
                    userRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();

                    document.Paragraphs.Add();

                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table paymentsTable = document.Tables.Add(tableRange,
                        allCategories.Count() + 1, 2);

                    paymentsTable.Borders.InsideLineStyle =
                        paymentsTable.Borders.OutsideLineStyle =
                        Word.WdLineStyle.wdLineStyleSingle;

                    paymentsTable.Range.Cells.VerticalAlignment =
                        Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    Word.Range cellRange;
                    cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.Bold = 1;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];
                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        cellRange = paymentsTable.Cell(i + 2, 2).Range;
                        decimal sum = user.Payment.ToList()
                            .Where(u => u.Category == currentCategory)
                            .Sum(u => u.Num * u.Price);
                        cellRange.Text = sum.ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }

                    document.Paragraphs.Add();

                    if (user.Payment.Any())
                    {
                        var maxPayment = user.Payment
                            .OrderByDescending(u => u.Price * u.Num)
                            .FirstOrDefault();

                        if (maxPayment != null)
                        {
                            Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                            Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                            maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} " +
                                $"за {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб. " +
                                $"от {maxPayment.Date.ToString("dd.MM.yyyy")}";
                            maxPaymentParagraph.set_Style("Подзаголовок");
                            maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                            maxPaymentRange.InsertParagraphAfter();
                        }

                        var minPayment = user.Payment
                            .OrderBy(u => u.Price * u.Num)
                            .FirstOrDefault();

                        if (minPayment != null)
                        {
                            Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                            Word.Range minPaymentRange = minPaymentParagraph.Range;
                            minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} " +
                                $"за {(minPayment.Price * minPayment.Num).ToString("N2")} руб. " +
                                $"от {minPayment.Date.ToString("dd.MM.yyyy")}";
                            minPaymentParagraph.set_Style("Подзаголовок");
                            minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                            minPaymentRange.InsertParagraphAfter();
                        }
                    }

                    if (user != allUsers.LastOrDefault())
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }

                application.Visible = true;
                MessageBox.Show("Экспорт в Word выполнен успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.Users.ToList();
                var allCategories = _context.Category.ToList();

                var excelApp = new Excel.Application();
                excelApp.Workbooks.Add();
                Excel.Worksheet worksheet = excelApp.ActiveSheet;
                worksheet.Name = "Отчет по платежам";

                int currentRow = 1;

                worksheet.Cells[currentRow, 1] = "Сводный отчет по платежам всех пользователей";
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 5]].Merge();
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 1]].Font.Size = 16;
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 1]].Font.Bold = true;
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 1]].HorizontalAlignment =
                    Excel.XlHAlign.xlHAlignCenter;

                currentRow += 2;

                foreach (var user in allUsers)
                {
                    worksheet.Cells[currentRow, 1] = $"Пользователь: {user.FIO}";
                    worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 5]].Merge();
                    worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 1]].Font.Size = 14;
                    worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 1]].Font.Bold = true;
                    worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 1]].Interior.Color =
                        0xFFE6D8;
                    currentRow += 2;

                    worksheet.Cells[currentRow, 1] = "Категория";
                    worksheet.Cells[currentRow, 2] = "Название платежа";
                    worksheet.Cells[currentRow, 3] = "Дата";
                    worksheet.Cells[currentRow, 4] = "Количество";
                    worksheet.Cells[currentRow, 5] = "Сумма";

                    Excel.Range headerRange = worksheet.Range[
                        worksheet.Cells[currentRow, 1],
                        worksheet.Cells[currentRow, 5]];
                    headerRange.Font.Bold = true;
                    headerRange.Interior.Color = 0xD3D3D3;
                    headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    currentRow++;

                    var userPayments = _context.Payment
                        .Where(p => p.UserID == user.ID)
                        .OrderBy(p => p.Category.Name)
                        .ThenBy(p => p.Date)
                        .ToList();

                    decimal totalSum = 0;
                    foreach (var payment in userPayments)
                    {
                        worksheet.Cells[currentRow, 1] = payment.Category.Name;
                        worksheet.Cells[currentRow, 2] = payment.Name;
                        worksheet.Cells[currentRow, 3] = payment.Date.ToString("dd.MM.yyyy");
                        worksheet.Cells[currentRow, 4] = payment.Num;
                        worksheet.Cells[currentRow, 5] = payment.Price * payment.Num;

                        Excel.Range dataRange = worksheet.Range[
                            worksheet.Cells[currentRow, 1],
                            worksheet.Cells[currentRow, 5]];
                        dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                        totalSum += payment.Price * payment.Num;
                        currentRow++;
                    }

                    worksheet.Cells[currentRow, 4] = "Итого:";
                    worksheet.Cells[currentRow, 5] = totalSum;
                    Excel.Range totalRange = worksheet.Range[
                        worksheet.Cells[currentRow, 4],
                        worksheet.Cells[currentRow, 5]];
                    totalRange.Font.Bold = true;
                    totalRange.Interior.Color = 0x00FFFF;

                    currentRow += 3;
                }

                worksheet.Columns.AutoFit();

                Excel.Range allDataRange = worksheet.UsedRange;
                allDataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                excelApp.Visible = true;
                MessageBox.Show("Экспорт в Excel выполнен успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}