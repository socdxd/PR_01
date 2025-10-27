using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WPF_Payment_Project.Models;

namespace WPF_Payment_Project.Pages
{
    public partial class UserPage : Page
    {
        private Entities _context;
        private Users _currentUser;

        public UserPage()
        {
            InitializeComponent();
            _context = Entities.GetContext();
            _currentUser = Manager.CurrentUser;
            LoadUserInfo();
            LoadStatistics();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            btnMyPayments.ToolTip = "Просмотр и управление вашими платежами";
            btnAddPayment.ToolTip = "Добавить новый платеж";
            btnViewDiagrams.ToolTip = "Просмотр статистики ваших платежей";
            btnChangePassword.ToolTip = "Изменить пароль учетной записи";
            btnLogout.ToolTip = "Выход из системы";
        }

        private void LoadUserInfo()
        {
            if (_currentUser != null)
            {
                txtWelcome.Text = $"Добро пожаловать, {_currentUser.FIO}!";
                txtLogin.Text = $"Логин: {_currentUser.Login}";
                txtRole.Text = $"Роль: Пользователь";
            }
        }

        private void LoadStatistics()
        {
            try
            {
                if (_currentUser == null) return;

                var userPayments = _context.Payment
                    .Where(p => p.UserID == _currentUser.ID)
                    .ToList();

                txtTotalPayments.Text = userPayments.Count.ToString();
                
                decimal totalSum = userPayments.Sum(p => p.Price * p.Num);
                txtTotalSum.Text = $"{totalSum:N2} руб.";

                if (userPayments.Any())
                {
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;
                    
                    var monthPayments = userPayments
                        .Where(p => p.Date.Month == currentMonth && p.Date.Year == currentYear)
                        .ToList();
                    
                    decimal monthSum = monthPayments.Sum(p => p.Price * p.Num);
                    txtMonthSum.Text = $"{monthSum:N2} руб.";
                    txtMonthCount.Text = monthPayments.Count.ToString();

                    var topCategory = userPayments
                        .GroupBy(p => p.Category.Name)
                        .OrderByDescending(g => g.Sum(p => p.Price * p.Num))
                        .FirstOrDefault();

                    if (topCategory != null)
                    {
                        txtTopCategory.Text = topCategory.Key;
                        decimal categorySum = topCategory.Sum(p => p.Price * p.Num);
                        txtTopCategorySum.Text = $"{categorySum:N2} руб.";
                    }

                    var lastPayment = userPayments
                        .OrderByDescending(p => p.Date)
                        .FirstOrDefault();
                    
                    if (lastPayment != null)
                    {
                        txtLastPayment.Text = $"{lastPayment.Date:dd.MM.yyyy} - {lastPayment.Name} ({lastPayment.Price * lastPayment.Num:N2} руб.)";
                    }
                }
                else
                {
                    txtMonthSum.Text = "0.00 руб.";
                    txtMonthCount.Text = "0";
                    txtTopCategory.Text = "Нет данных";
                    txtTopCategorySum.Text = "0.00 руб.";
                    txtLastPayment.Text = "Нет платежей";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnMyPayments_Click(object sender, RoutedEventArgs e)
        {
            frameContent.Navigate(new PaymentTabPage());
        }

        private void btnAddPayment_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddPaymentPage(null));
        }

        private void btnViewDiagrams_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new DiagrammPage());
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChangePassPage());
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

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            if (frameContent.Content is PaymentTabPage paymentPage)
            {
                frameContent.Navigate(new PaymentTabPage());
            }
            MessageBox.Show("Данные обновлены", "Информация", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }
    }
}
