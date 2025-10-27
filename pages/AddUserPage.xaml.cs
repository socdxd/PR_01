using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WPF_Payment_Project.Models;
using WPF_Payment_Project.Helpers;

namespace WPF_Payment_Project.Pages
{
    public partial class AddUserPage : Page
    {
        private Entities _context;
        private Users _currentUser;
        private string _selectedPhotoPath;

        public AddUserPage(Users user)
        {
            InitializeComponent();
            _context = Entities.GetContext();
            _currentUser = user;
            SetupToolTips();
            LoadRoles();

            if (_currentUser != null)
            {
                Title = "Редактирование пользователя";
                btnSave.Content = "Сохранить изменения";
                FillData();
                passwordPanel.Visibility = Visibility.Collapsed;
                btnResetPassword.Visibility = Visibility.Visible;
            }
            else
            {
                Title = "Добавление пользователя";
                btnSave.Content = "Добавить пользователя";
                _currentUser = new Users();
                btnResetPassword.Visibility = Visibility.Collapsed;
            }
        }

        private void SetupToolTips()
        {
            txtLogin.ToolTip = "Логин должен содержать только латинские буквы и цифры";
            txtPassword.ToolTip = "Пароль должен содержать минимум 6 символов";
            txtConfirmPassword.ToolTip = "Повторите пароль для подтверждения";
            txtFIO.ToolTip = "Введите Фамилию Имя Отчество";
            cmbRole.ToolTip = "Выберите роль пользователя";
            btnSelectPhoto.ToolTip = "Выберите фотографию профиля";
            btnRemovePhoto.ToolTip = "Удалить фотографию";
            btnSave.ToolTip = "Сохранить пользователя";
            btnCancel.ToolTip = "Отменить и вернуться назад";
        }

        private void LoadRoles()
        {
            cmbRole.Items.Add("Admin");
            cmbRole.Items.Add("User");
            cmbRole.SelectedIndex = 1;
        }

        private void FillData()
        {
            if (_currentUser != null)
            {
                txtLogin.Text = _currentUser.Login;
                txtFIO.Text = _currentUser.FIO;
                cmbRole.SelectedItem = _currentUser.Role;
                
                if (!string.IsNullOrEmpty(_currentUser.Photo) && File.Exists(_currentUser.Photo))
                {
                    _selectedPhotoPath = _currentUser.Photo;
                    LoadPhoto(_selectedPhotoPath);
                }
            }
        }

        private void LoadPhoto(string path)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgPhoto.Source = bitmap;
                btnRemovePhoto.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фото: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
                _currentUser.Login = txtLogin.Text.Trim();
                _currentUser.FIO = txtFIO.Text.Trim();
                _currentUser.Role = cmbRole.SelectedItem.ToString();
                _currentUser.Photo = _selectedPhotoPath;

                if (_currentUser.ID == 0)
                {
                    _currentUser.Password = PasswordHelper.HashPassword(txtPassword.Password);
                    _context.Users.Add(_currentUser);
                }

                _context.SaveChanges();
                MessageBox.Show("Пользователь успешно сохранен", "Успех", 
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
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
                return "Введите логин";

            if (!Regex.IsMatch(txtLogin.Text, @"^[a-zA-Z0-9]+$"))
                return "Логин должен содержать только латинские буквы и цифры";

            if (txtLogin.Text.Length < 3)
                return "Логин должен содержать минимум 3 символа";

            var existingUser = _context.Users
                .FirstOrDefault(u => u.Login == txtLogin.Text && u.ID != _currentUser.ID);
            
            if (existingUser != null)
                return "Пользователь с таким логином уже существует";

            if (_currentUser.ID == 0)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                    return "Введите пароль";

                if (!ValidationHelper.IsStrongPassword(txtPassword.Password))
                    return "Пароль должен содержать минимум 6 символов, заглавные и строчные буквы, и цифры";

                if (txtPassword.Password != txtConfirmPassword.Password)
                    return "Пароли не совпадают";
            }

            if (string.IsNullOrWhiteSpace(txtFIO.Text))
                return "Введите ФИО";

            string[] fioWords = txtFIO.Text.Trim().Split(' ');
            if (fioWords.Length < 2)
                return "Введите полное ФИО (минимум Фамилия и Имя)";

            if (cmbRole.SelectedItem == null)
                return "Выберите роль";

            return null;
        }

        private void btnSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog.Title = "Выберите фотографию профиля";

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedPhotoPath = openFileDialog.FileName;
                LoadPhoto(_selectedPhotoPath);
            }
        }

        private void btnRemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            _selectedPhotoPath = null;
            imgPhoto.Source = null;
            btnRemovePhoto.Visibility = Visibility.Collapsed;
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Сбросить пароль на 'Password123'?", 
                "Подтверждение", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _currentUser.Password = PasswordHelper.HashPassword("Password123");
                MessageBox.Show("Пароль будет сброшен при сохранении", "Информация", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLoginHint.Visibility = string.IsNullOrEmpty(txtLogin.Text) ? 
                Visibility.Visible : Visibility.Collapsed;
            
            if (!string.IsNullOrEmpty(txtLogin.Text))
            {
                if (!Regex.IsMatch(txtLogin.Text, @"^[a-zA-Z0-9]+$"))
                {
                    txtLogin.BorderBrush = Brushes.Red;
                }
                else
                {
                    txtLogin.BorderBrush = SystemColors.ControlDarkBrush;
                }
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPasswordHint.Visibility = string.IsNullOrEmpty(txtPassword.Password) ? 
                Visibility.Visible : Visibility.Collapsed;
            
            if (!string.IsNullOrEmpty(txtPassword.Password))
            {
                if (!ValidationHelper.IsStrongPassword(txtPassword.Password))
                {
                    txtPassword.BorderBrush = Brushes.Red;
                    txtPasswordStrength.Text = "Слабый пароль";
                    txtPasswordStrength.Foreground = Brushes.Red;
                }
                else
                {
                    txtPassword.BorderBrush = SystemColors.ControlDarkBrush;
                    txtPasswordStrength.Text = "Сильный пароль";
                    txtPasswordStrength.Foreground = Brushes.Green;
                }
                txtPasswordStrength.Visibility = Visibility.Visible;
            }
            else
            {
                txtPasswordStrength.Visibility = Visibility.Collapsed;
            }

            CheckPasswordsMatch();
        }

        private void txtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblConfirmPasswordHint.Visibility = string.IsNullOrEmpty(txtConfirmPassword.Password) ? 
                Visibility.Visible : Visibility.Collapsed;
            CheckPasswordsMatch();
        }

        private void CheckPasswordsMatch()
        {
            if (!string.IsNullOrEmpty(txtPassword.Password) && 
                !string.IsNullOrEmpty(txtConfirmPassword.Password))
            {
                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    txtConfirmPassword.BorderBrush = Brushes.Red;
                }
                else
                {
                    txtConfirmPassword.BorderBrush = SystemColors.ControlDarkBrush;
                }
            }
        }

        private void txtFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFIOHint.Visibility = string.IsNullOrEmpty(txtFIO.Text) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        private void txtLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$"))
            {
                e.Handled = true;
            }
        }

        private void txtFIO_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[а-яА-ЯёЁa-zA-Z\s]+$"))
            {
                e.Handled = true;
            }
        }
    }
}
