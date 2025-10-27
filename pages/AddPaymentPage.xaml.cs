using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_Payment_Project.Models;

namespace WPF_Payment_Project.Pages
{
    public partial class AddPaymentPage : Page
    {
        private Entities _context;
        private Payment _currentPayment;

        public AddPaymentPage(Payment payment)
        {
            InitializeComponent();
            _context = Entities.GetContext();
            _currentPayment = payment;
            LoadData();
            SetupToolTips();

            if (_currentPayment != null)
            {
                Title = "Редактирование платежа";
                btnSave.Content = "Сохранить изменения";
                FillData();
            }
            else
            {
                Title = "Добавление платежа";
                btnSave.Content = "Добавить платеж";
                _currentPayment = new Payment();
                dpDate.SelectedDate = DateTime.Now;
                txtNum.Text = "1";
            }
        }

        private void SetupToolTips()
        {
            txtName.ToolTip = "Введите название платежа";
            cmbCategory.ToolTip = "Выберите категорию платежа";
            cmbUser.ToolTip = "Выберите пользователя";
            dpDate.ToolTip = "Выберите дату платежа";
            txtNum.ToolTip = "Введите количество (от 1 до 999)";
            txtPrice.ToolTip = "Введите стоимость за единицу";
            btnSave.ToolTip = "Сохранить платеж";
            btnCancel.ToolTip = "Отменить и вернуться назад";
        }

        private void LoadData()
        {
            cmbCategory.ItemsSource = _context.Category.ToList();
            cmbCategory.DisplayMemberPath = "Name";

            if (Manager.CurrentUser?.Role == "Admin")
            {
                cmbUser.ItemsSource = _context.Users.ToList();
                cmbUser.DisplayMemberPath = "FIO";
                cmbUser.Visibility = Visibility.Visible;
                lblUser.Visibility = Visibility.Visible;
            }
            else
            {
                cmbUser.Visibility = Visibility.Collapsed;
                lblUser.Visibility = Visibility.Collapsed;
            }
        }

        private void FillData()
        {
            if (_currentPayment != null)
            {
                txtName.Text = _currentPayment.Name;
                cmbCategory.SelectedValue = _currentPayment.CategoryID;
                dpDate.SelectedDate = _currentPayment.Date;
                txtNum.Text = _currentPayment.Num.ToString();
                txtPrice.Text = _currentPayment.Price.ToString();

                if (Manager.CurrentUser?.Role == "Admin")
                {
                    cmbUser.SelectedValue = _currentPayment.UserID;
                }

                UpdateTotalSum();
            }
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
                _currentPayment.Name = txtName.Text.Trim();
                _currentPayment.CategoryID = (cmbCategory.SelectedItem as Category).ID;
                _currentPayment.Date = dpDate.SelectedDate.Value;
                _currentPayment.Num = int.Parse(txtNum.Text);
                _currentPayment.Price = decimal.Parse(txtPrice.Text.Replace(",", "."));

                if (Manager.CurrentUser?.Role == "Admin" && cmbUser.SelectedItem != null)
                {
                    _currentPayment.UserID = (cmbUser.SelectedItem as Users).ID;
                }
                else if (_currentPayment.ID == 0)
                {
                    _currentPayment.UserID = Manager.CurrentUser.ID;
                }

                if (_currentPayment.ID == 0)
                {
                    _context.Payment.Add(_currentPayment);
                }

                _context.SaveChanges();
                MessageBox.Show("Платеж успешно сохранен", "Успех",
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
                return "Введите название платежа";

            if (txtName.Text.Length < 2)
                return "Название платежа должно содержать минимум 2 символа";

            if (cmbCategory.SelectedItem == null)
                return "Выберите категорию";

            if (Manager.CurrentUser?.Role == "Admin" && cmbUser.SelectedItem == null)
                return "Выберите пользователя";

            if (!dpDate.SelectedDate.HasValue)
                return "Выберите дату";

            if (dpDate.SelectedDate.Value > DateTime.Now)
                return "Дата не может быть в будущем";

            if (!int.TryParse(txtNum.Text, out int num) || num < 1 || num > 999)
                return "Количество должно быть числом от 1 до 999";

            if (!decimal.TryParse(txtPrice.Text.Replace(",", "."), out decimal price) || price <= 0)
                return "Цена должна быть положительным числом";

            if (price > 999999)
                return "Цена не может превышать 999999";

            return null;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void txtNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void txtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != "," && e.Text != ".")
            {
                e.Handled = true;
            }
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotalSum();
        }

        private void txtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotalSum();
        }

        private void UpdateTotalSum()
        {
            if (int.TryParse(txtNum.Text, out int num) &&
                decimal.TryParse(txtPrice.Text.Replace(",", "."), out decimal price))
            {
                decimal total = num * price;
                txtTotalSum.Text = $"Итого: {total:N2} руб.";
            }
            else
            {
                txtTotalSum.Text = "Итого: 0.00 руб.";
            }
        }

        private void txtNum_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNum.Text) || txtNum.Text == "0")
            {
                txtNum.Text = "1";
            }
        }

        private void txtPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtPrice.Text.Replace(",", "."), out decimal price))
            {
                txtPrice.Text = price.ToString("F2");
            }
        }

        private void txtName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
        }
    }
}