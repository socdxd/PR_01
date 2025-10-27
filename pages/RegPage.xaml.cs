using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPF_Payment_Project.Models;
using WPF_Payment_Project.Helpers;
using Microsoft.Win32;
using System.IO;

namespace WPF_Payment_Project.Pages
{
    public partial class RegPage : Page
    {
        private string selectedPhotoPath;

        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0;
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            txtbxLog.ToolTip = "Логин должен содержать только латинские буквы и цифры";
            passBxFrst.ToolTip = "Пароль должен содержать минимум 6 символов, включая заглавные и строчные буквы, цифры";
            passBxScnd.ToolTip = "Повторите пароль для подтверждения";
            txtbxFIO.ToolTip = "Введите Фамилию Имя Отчество";
            comboBxRole.ToolTip = "Выберите роль пользователя";
            btnSelectPhoto.ToolTip = "Выберите фотографию профиля (необязательно)";
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            string validationError = ValidateInput();
            if (!string.IsNullOrEmpty(validationError))
            {
                MessageBox.Show(validationError, "Ошибка валидации", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingUser = Entities.GetContext().Users
                .FirstOrDefault(x => x.Login == txtbxLog.Text);
            
            if (existingUser != null)
            {
                MessageBox.Show("Пользователь с таким логином уже существует", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Users newUser = new Users
                {
                    Login = txtbxLog.Text,
                    Password = PasswordHelper.HashPassword(passBxFrst.Password),
                    Role = (comboBxRole.SelectedItem as ComboBoxItem).Content.ToString(),
                    FIO = txtbxFIO.Text,
                    Photo = selectedPhotoPath
                };

                Entities.GetContext().Users.Add(newUser);
                Entities.GetContext().SaveChanges();

                MessageBox.Show("Регистрация успешна! Теперь вы можете войти в систему", 
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.Navigate(new AuthPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtbxLog.Text))
                return "Введите логин";

            if (!Regex.IsMatch(txtbxLog.Text, @"^[a-zA-Z0-9]+$"))
                return "Логин должен содержать только латинские буквы и цифры";

            if (txtbxLog.Text.Length < 3)
                return "Логин должен содержать минимум 3 символа";

            if (string.IsNullOrWhiteSpace(passBxFrst.Password))
                return "Введите пароль";

            if (!ValidationHelper.IsStrongPassword(passBxFrst.Password))
                return "Пароль должен содержать минимум 6 символов, включая заглавные и строчные буквы, и цифры";

            if (passBxFrst.Password != passBxScnd.Password)
                return "Пароли не совпадают";

            if (string.IsNullOrWhiteSpace(txtbxFIO.Text))
                return "Введите ФИО";

            string[] fioWords = txtbxFIO.Text.Trim().Split(' ');
            if (fioWords.Length < 2)
                return "Введите полное ФИО (минимум Фамилия и Имя)";

            foreach (string word in fioWords)
            {
                if (!Regex.IsMatch(word, @"^[а-яА-ЯёЁa-zA-Z]+$"))
                    return "ФИО должно содержать только буквы";
            }

            return null;
        }

        private void btnSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog.Title = "Выберите фотографию профиля";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedPhotoPath = openFileDialog.FileName;
                lblPhotoPath.Text = Path.GetFileName(selectedPhotoPath);
                lblPhotoPath.Visibility = Visibility.Visible;
            }
        }

        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = string.IsNullOrEmpty(txtbxLog.Text) ? 
                Visibility.Visible : Visibility.Collapsed;
            
            if (!string.IsNullOrEmpty(txtbxLog.Text))
            {
                if (!Regex.IsMatch(txtbxLog.Text, @"^[a-zA-Z0-9]+$"))
                {
                    txtbxLog.BorderBrush = Brushes.Red;
                }
                else
                {
                    txtbxLog.BorderBrush = SystemColors.ControlDarkBrush;
                }
            }
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = string.IsNullOrEmpty(passBxFrst.Password) ? 
                Visibility.Visible : Visibility.Collapsed;
            
            if (!string.IsNullOrEmpty(passBxFrst.Password))
            {
                if (!ValidationHelper.IsStrongPassword(passBxFrst.Password))
                {
                    passBxFrst.BorderBrush = Brushes.Red;
                    passwordStrength.Text = "Слабый пароль";
                    passwordStrength.Foreground = Brushes.Red;
                }
                else
                {
                    passBxFrst.BorderBrush = SystemColors.ControlDarkBrush;
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

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = string.IsNullOrEmpty(passBxScnd.Password) ? 
                Visibility.Visible : Visibility.Collapsed;
            CheckPasswordsMatch();
        }

        private void CheckPasswordsMatch()
        {
            if (!string.IsNullOrEmpty(passBxFrst.Password) && 
                !string.IsNullOrEmpty(passBxScnd.Password))
            {
                if (passBxFrst.Password != passBxScnd.Password)
                {
                    passBxScnd.BorderBrush = Brushes.Red;
                    passwordMatch.Text = "Пароли не совпадают";
                    passwordMatch.Foreground = Brushes.Red;
                }
                else
                {
                    passBxScnd.BorderBrush = SystemColors.ControlDarkBrush;
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

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = string.IsNullOrEmpty(txtbxFIO.Text) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        private void txtbxLog_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$"))
            {
                e.Handled = true;
            }
        }

        private void txtbxFIO_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[а-яА-ЯёЁa-zA-Z\s]+$"))
            {
                e.Handled = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AuthPage());
        }
    }
}
