using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPF_Payment_Project.Models;
using WPF_Payment_Project.Helpers;

namespace WPF_Payment_Project.Pages
{
    public partial class ChangePassPage : Page
    {
        private Entities _context;

        public ChangePassPage()
        {
            InitializeComponent();
            _context = Entities.GetContext();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            txtLogin.ToolTip = "Введите ваш логин";
            txtOldPassword.ToolTip = "Введите текущий пароль";
            txtNewPassword.ToolTip = "Введите новый пароль (минимум 6 символов, заглавные и строчные буквы, цифры)";
            txtConfirmPassword.ToolTip = "Повторите новый пароль";
            btnChangePassword.ToolTip = "Нажмите для смены пароля";
            btnCancel.ToolTip = "Вернуться к авторизации";
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string validationError = ValidateInput();
            if (!string.IsNullOrEmpty(validationError))
            {
                MessageBox.Show(validationError, "Ошибка валидации", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string oldPasswordHash = PasswordHelper.HashPassword(txtOldPassword.Password);
            var user = _context.Users
                .FirstOrDefault(x => x.Login == txtLogin.Text && x.Password == oldPasswordHash);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или старый пароль", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string newPasswordHash = PasswordHelper.HashPassword(txtNewPassword.Password);
            
            if (newPasswordHash == oldPasswordHash)
            {
                MessageBox.Show("Новый пароль не должен совпадать со старым", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы действительно хотите сменить пароль?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    user.Password = newPasswordHash;
                    _context.SaveChanges();

                    MessageBox.Show("Пароль успешно изменен! Теперь используйте новый пароль для входа", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Manager.MainFrame.Navigate(new AuthPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при смене пароля: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
                return "Введите логин";

            if (string.IsNullOrWhiteSpace(txtOldPassword.Password))
                return "Введите старый пароль";

            if (string.IsNullOrWhiteSpace(txtNewPassword.Password))
                return "Введите новый пароль";

            if (!ValidationHelper.IsStrongPassword(txtNewPassword.Password))
                return "Новый пароль должен содержать минимум 6 символов, включая заглавные и строчные буквы, и цифры";

            if (txtNewPassword.Password != txtConfirmPassword.Password)
                return "Новый пароль и подтверждение не совпадают";

            return null;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AuthPage());
        }

        private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLoginHint.Visibility = string.IsNullOrEmpty(txtLogin.Text) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        private void txtOldPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblOldPassHint.Visibility = string.IsNullOrEmpty(txtOldPassword.Password) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        private void txtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblNewPassHint.Visibility = string.IsNullOrEmpty(txtNewPassword.Password) ? 
                Visibility.Visible : Visibility.Collapsed;
            
            if (!string.IsNullOrEmpty(txtNewPassword.Password))
            {
                if (!ValidationHelper.IsStrongPassword(txtNewPassword.Password))
                {
                    txtNewPassword.BorderBrush = Brushes.Red;
                    passwordStrength.Text = "Слабый пароль";
                    passwordStrength.Foreground = Brushes.Red;
                }
                else
                {
                    txtNewPassword.BorderBrush = SystemColors.ControlDarkBrush;
                    passwordStrength.Text = "Сильный пароль";
                    passwordStrength.Foreground = Brushes.Green;
                }
                passwordStrength.Visibility = Visibility.Visible;
            }
            else
            {
                passwordStrength.Visibility = Visibility.Collapsed;
            }

            CheckPasswordsMatch();
        }

        private void txtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblConfirmPassHint.Visibility = string.IsNullOrEmpty(txtConfirmPassword.Password) ? 
                Visibility.Visible : Visibility.Collapsed;
            CheckPasswordsMatch();
        }

        private void CheckPasswordsMatch()
        {
            if (!string.IsNullOrEmpty(txtNewPassword.Password) && 
                !string.IsNullOrEmpty(txtConfirmPassword.Password))
            {
                if (txtNewPassword.Password != txtConfirmPassword.Password)
                {
                    txtConfirmPassword.BorderBrush = Brushes.Red;
                    passwordMatch.Text = "Пароли не совпадают";
                    passwordMatch.Foreground = Brushes.Red;
                }
                else
                {
                    txtConfirmPassword.BorderBrush = SystemColors.ControlDarkBrush;
                    passwordMatch.Text = "Пароли совпадают";
                    passwordMatch.Foreground = Brushes.Green;
                }
                passwordMatch.Visibility = Visibility.Visible;
            }
            else
            {
                passwordMatch.Visibility = Visibility.Collapsed;
            }
        }

        private void txtLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$"))
            {
                e.Handled = true;
            }
        }
    }
}
