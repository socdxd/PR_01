using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Payment_Project.Models;
using WPF_Payment_Project.Helpers;

namespace WPF_Payment_Project.Pages
{
    public partial class UsersTabPage : Page
    {
        private Entities _context;

        public UsersTabPage()
        {
            InitializeComponent();
            _context = Entities.GetContext();
            LoadData();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            btnAdd.ToolTip = "Добавить нового пользователя";
            btnEdit.ToolTip = "Редактировать выбранного пользователя";
            btnDelete.ToolTip = "Удалить выбранного пользователя";
            btnRefresh.ToolTip = "Обновить список пользователей";
            btnResetPassword.ToolTip = "Сбросить пароль пользователя";
            txtSearch.ToolTip = "Введите текст для поиска";
            cmbRole.ToolTip = "Фильтр по роли";
        }

        private void LoadData()
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                query = query.Where(u => u.FIO.ToLower().Contains(searchText) ||
                                        u.Login.ToLower().Contains(searchText));
            }

            if (cmbRole.SelectedItem != null && cmbRole.SelectedIndex > 0)
            {
                string selectedRole = (cmbRole.SelectedItem as ComboBoxItem).Content.ToString();
                query = query.Where(u => u.Role == selectedRole);
            }

            var users = query.ToList();

            foreach (var user in users)
            {
                var paymentCount = _context.Payment.Count(p => p.UserID == user.ID);
                var totalSum = _context.Payment
                    .Where(p => p.UserID == user.ID)
                    .Sum(p => (decimal?)(p.Price * p.Num)) ?? 0;

                user.PaymentCount = paymentCount;
                user.TotalPaymentSum = totalSum;
            }

            dgUsers.ItemsSource = users;
            txtTotalCount.Text = $"Всего пользователей: {users.Count}";
        }

        private void LoadRoles()
        {
            if (cmbRole.Items.Count == 0)
            {
                cmbRole.Items.Add(new ComboBoxItem { Content = "Все роли" });
                cmbRole.Items.Add(new ComboBoxItem { Content = "Admin" });
                cmbRole.Items.Add(new ComboBoxItem { Content = "User" });
                cmbRole.SelectedIndex = 0;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddUserPage(null));
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgUsers.SelectedItem as Users;
            if (selected == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Manager.MainFrame.Navigate(new AddUserPage(selected));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgUsers.SelectedItem as Users;
            if (selected == null)
            {
                MessageBox.Show("Выберите пользователя для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selected.ID == Manager.CurrentUser.ID)
            {
                MessageBox.Show("Вы не можете удалить свою учетную запись", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var paymentCount = _context.Payment.Count(p => p.UserID == selected.ID);
            if (paymentCount > 0)
            {
                var result = MessageBox.Show($"У пользователя '{selected.FIO}' есть {paymentCount} платежей. " +
                    "При удалении пользователя все его платежи также будут удалены. Продолжить?",
                    "Предупреждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;
            }
            else
            {
                var result = MessageBox.Show($"Вы действительно хотите удалить пользователя '{selected.FIO}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            try
            {
                var userPayments = _context.Payment.Where(p => p.UserID == selected.ID).ToList();
                _context.Payment.RemoveRange(userPayments);
                _context.Users.Remove(selected);
                _context.SaveChanges();

                MessageBox.Show("Пользователь успешно удален", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgUsers.SelectedItem as Users;
            if (selected == null)
            {
                MessageBox.Show("Выберите пользователя для сброса пароля", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Сбросить пароль для пользователя '{selected.FIO}'? " +
                "Новый пароль будет: 'Password123'",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    selected.Password = PasswordHelper.HashPassword("Password123");
                    _context.SaveChanges();

                    MessageBox.Show($"Пароль успешно сброшен. Новый пароль: Password123",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сбросе пароля: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbRole.SelectedIndex = 0;
            LoadData();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void cmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers != null)
                LoadData();
        }

        private void dgUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEdit_Click(sender, e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRoles();
            LoadData();
        }
    }
}

namespace WPF_Payment_Project.Models
{
    public partial class Users
    {
        public int PaymentCount { get; set; }
        public decimal TotalPaymentSum { get; set; }
    }
}