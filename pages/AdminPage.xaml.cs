using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WPF_Payment_Project.Models;

namespace WPF_Payment_Project.Pages
{
    public partial class AdminPage : Page
    {
        private Entities _context;

        public AdminPage()
        {
            InitializeComponent();
            _context = Entities.GetContext();
            LoadStatistics();
            SetupToolTips();

            if (Manager.CurrentUser != null)
            {
                txtWelcome.Text = $"Добро пожаловать, {Manager.CurrentUser.FIO}!";
                txtRole.Text = $"Ваша роль: Администратор";
            }
        }

        private void SetupToolTips()
        {
            btnManageUsers.ToolTip = "Управление пользователями системы";
            btnManagePayments.ToolTip = "Управление всеми платежами";
            btnManageCategories.ToolTip = "Управление категориями платежей";
            btnViewDiagrams.ToolTip = "Просмотр диаграмм и статистики";
            btnExportReports.ToolTip = "Экспорт отчетов в Word и Excel";
            btnLogout.ToolTip = "Выход из системы";
        }

        private void LoadStatistics()
        {
            try
            {
                var totalUsers = _context.Users.Count();
                var totalPayments = _context.Payment.Count();
                var totalCategories = _context.Category.Count();
                var totalSum = _context.Payment.Sum(p => (decimal?)(p.Price * p.Num)) ?? 0;

                txtTotalUsers.Text = totalUsers.ToString();
                txtTotalPayments.Text = totalPayments.ToString();
                txtTotalCategories.Text = totalCategories.ToString();
                txtTotalSum.Text = $"{totalSum:N2} руб.";

                var lastPayment = _context.Payment
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefault();

                if (lastPayment != null)
                {
                    txtLastActivity.Text = $"Последний платеж: {lastPayment.Date:dd.MM.yyyy} - {lastPayment.Name}";
                }
                else
                {
                    txtLastActivity.Text = "Нет платежей";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnManageUsers_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
            frameContent.Navigate(new UsersTabPage());
        }

        private void btnManagePayments_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
            frameContent.Navigate(new PaymentTabPage());
        }

        private void btnManageCategories_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 2;
            frameContent.Navigate(new CategoryTabPage());
        }

        private void btnViewDiagrams_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new DiagrammPage());
        }

        private void btnExportReports_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new DiagrammPage());
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Manager.CurrentUser = null;
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ButtonBack.Visibility = Visibility.Hidden;
                }
                Manager.MainFrame.Navigate(new AuthPage());
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (frameContent == null) return;

            var tabControl = sender as TabControl;
            if (tabControl != null)
            {
                switch (tabControl.SelectedIndex)
                {
                    case 0:
                        frameContent.Navigate(new UsersTabPage());
                        break;
                    case 1:
                        frameContent.Navigate(new PaymentTabPage());
                        break;
                    case 2:
                        frameContent.Navigate(new CategoryTabPage());
                        break;
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }
    }
}