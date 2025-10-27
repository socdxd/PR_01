using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Payment_Project.Models;

namespace WPF_Payment_Project.Pages
{
    public partial class AddCategoryPage : Page
    {
        private Entities _context;
        private Category _currentCategory;

        public AddCategoryPage(Category category)
        {
            InitializeComponent();
            _context = Entities.GetContext();
            _currentCategory = category;
            SetupToolTips();

            if (_currentCategory != null)
            {
                Title = "Редактирование категории";
                btnSave.Content = "Сохранить изменения";
                txtName.Text = _currentCategory.Name;
                txtDescription.Text = _currentCategory.Description;
            }
            else
            {
                Title = "Добавление категории";
                btnSave.Content = "Добавить категорию";
                _currentCategory = new Category();
            }
        }

        private void SetupToolTips()
        {
            txtName.ToolTip = "Введите название категории";
            txtDescription.ToolTip = "Введите описание категории (необязательно)";
            btnSave.ToolTip = "Сохранить категорию";
            btnCancel.ToolTip = "Отменить и вернуться назад";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string validationError = ValidateInput();
            if (!string.IsNullOrEmpty(validationError))
            {
                MessageBox.Show(validationError, "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _currentCategory.Name = txtName.Text.Trim();
                _currentCategory.Description = txtDescription.Text.Trim();

                if (_currentCategory.ID == 0)
                {
                    _context.Category.Add(_currentCategory);
                }

                _context.SaveChanges();
                MessageBox.Show("Категория успешно сохранена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
                return "Введите название категории";

            if (txtName.Text.Length < 2)
                return "Название категории должно содержать минимум 2 символа";

            if (txtName.Text.Length > 50)
                return "Название категории не должно превышать 50 символов";

            var existingCategory = _context.Category
                .FirstOrDefault(c => c.Name.ToLower() == txtName.Text.Trim().ToLower() &&
                                     c.ID != _currentCategory.ID);

            if (existingCategory != null)
                return "Категория с таким названием уже существует";

            if (txtDescription.Text.Length > 200)
                return "Описание не должно превышать 200 символов";

            return null;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblNameHint.Visibility = string.IsNullOrEmpty(txtName.Text) ?
                Visibility.Visible : Visibility.Collapsed;

            if (txtName.Text.Length > 0)
            {
                txtCharCount.Text = $"{txtName.Text.Length}/50";
            }
            else
            {
                txtCharCount.Text = "0/50";
            }
        }

        private void txtDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblDescriptionHint.Visibility = string.IsNullOrEmpty(txtDescription.Text) ?
                Visibility.Visible : Visibility.Collapsed;

            if (txtDescription.Text.Length > 0)
            {
                txtDescCharCount.Text = $"{txtDescription.Text.Length}/200";
            }
            else
            {
                txtDescCharCount.Text = "0/200";
            }
        }

        private void txtName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (txtName.Text.Length >= 50)
            {
                e.Handled = true;
            }
        }
    }
}