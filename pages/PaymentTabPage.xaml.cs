using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Payment_Project.Models;

namespace WPF_Payment_Project.Pages
{
    public partial class PaymentTabPage : Page
    {
        private Entities _context;
        private Users _currentUser;

        public PaymentTabPage()
        {
            InitializeComponent();
            _context = Entities.GetContext();
            _currentUser = Manager.CurrentUser;
            LoadData();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            btnAdd.ToolTip = "Добавить новый платеж";
            btnEdit.ToolTip = "Редактировать выбранный платеж";
            btnDelete.ToolTip = "Удалить выбранный платеж";
            btnRefresh.ToolTip = "Обновить список платежей";
            txtSearch.ToolTip = "Введите текст для поиска";
            cmbCategory.ToolTip = "Фильтр по категории";
            dpStartDate.ToolTip = "Начальная дата для фильтрации";
            dpEndDate.ToolTip = "Конечная дата для фильтрации";
        }

        private void LoadData()
        {
            var query = _context.Payment.AsQueryable();

            if (_currentUser != null && _currentUser.Role != "Admin")
            {
                query = query.Where(p => p.UserID == _currentUser.ID);
            }

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchText));
            }

            if (cmbCategory.SelectedItem != null && cmbCategory.SelectedIndex > 0)
            {
                var selectedCategory = cmbCategory.SelectedItem as Category;
                query = query.Where(p => p.CategoryID == selectedCategory.ID);
            }

            if (dpStartDate.SelectedDate.HasValue)
            {
                query = query.Where(p => p.Date >= dpStartDate.SelectedDate.Value);
            }

            if (dpEndDate.SelectedDate.HasValue)
            {
                query = query.Where(p => p.Date <= dpEndDate.SelectedDate.Value);
            }

            var payments = query.ToList();
            dgPayments.ItemsSource = payments;

            decimal totalSum = payments.Sum(p => p.Price * p.Num);
            txtTotalSum.Text = $"Общая сумма: {totalSum:N2} руб.";
            txtTotalCount.Text = $"Количество платежей: {payments.Count}";

            LoadCategories();
        }

        private void LoadCategories()
        {
            if (cmbCategory.Items.Count == 0)
            {
                cmbCategory.Items.Add("Все категории");
                var categories = _context.Category.ToList();
                foreach (var category in categories)
                {
                    cmbCategory.Items.Add(category);
                }
                cmbCategory.SelectedIndex = 0;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddPaymentPage(null));
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgPayments.SelectedItem as Payment;
            if (selected == null)
            {
                MessageBox.Show("Выберите платеж для редактирования", "Внимание", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Manager.MainFrame.Navigate(new AddPaymentPage(selected));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgPayments.SelectedItem as Payment;
            if (selected == null)
            {
                MessageBox.Show("Выберите платеж для удаления", "Внимание", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Вы действительно хотите удалить платеж '{selected.Name}'?", 
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Payment.Remove(selected);
                    _context.SaveChanges();
                    MessageBox.Show("Платеж успешно удален", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbCategory.SelectedIndex = 0;
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            LoadData();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private void dpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        private void dgPayments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEdit_Click(sender, e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
