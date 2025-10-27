using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Payment_Project.Models;

namespace WPF_Payment_Project.Pages
{
    public partial class CategoryTabPage : Page
    {
        private Entities _context;

        public CategoryTabPage()
        {
            InitializeComponent();
            _context = Entities.GetContext();
            LoadData();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            btnAdd.ToolTip = "Добавить новую категорию";
            btnEdit.ToolTip = "Редактировать выбранную категорию";
            btnDelete.ToolTip = "Удалить выбранную категорию";
            btnRefresh.ToolTip = "Обновить список категорий";
            txtSearch.ToolTip = "Введите текст для поиска";
        }

        private void LoadData()
        {
            var query = _context.Category.AsQueryable();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(searchText));
            }

            var categories = query.ToList();
            dgCategories.ItemsSource = categories;

            foreach (var category in categories)
            {
                var paymentCount = _context.Payment.Count(p => p.CategoryID == category.ID);
                var totalSum = _context.Payment
                    .Where(p => p.CategoryID == category.ID)
                    .Sum(p => (decimal?)(p.Price * p.Num)) ?? 0;

                category.PaymentCount = paymentCount;
                category.TotalSum = totalSum;
            }

            txtTotalCount.Text = $"Всего категорий: {categories.Count}";
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddCategoryPage(null));
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgCategories.SelectedItem as Category;
            if (selected == null)
            {
                MessageBox.Show("Выберите категорию для редактирования", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Manager.MainFrame.Navigate(new AddCategoryPage(selected));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgCategories.SelectedItem as Category;
            if (selected == null)
            {
                MessageBox.Show("Выберите категорию для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var paymentCount = _context.Payment.Count(p => p.CategoryID == selected.ID);
            if (paymentCount > 0)
            {
                MessageBox.Show($"Невозможно удалить категорию '{selected.Name}', так как она используется в {paymentCount} платежах. " +
                    "Сначала удалите или измените связанные платежи.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы действительно хотите удалить категорию '{selected.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Category.Remove(selected);
                    _context.SaveChanges();
                    MessageBox.Show("Категория успешно удалена", "Успех",
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
            LoadData();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void dgCategories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEdit_Click(sender, e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}