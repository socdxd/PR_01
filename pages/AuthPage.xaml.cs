using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPF_Payment_Project.Models;
using WPF_Payment_Project.Helpers;

namespace WPF_Payment_Project.Pages
{
    public partial class AuthPage : Page
    {
        private int attemptCount = 0;
        private const int maxAttempts = 3;
        private string generatedCaptcha;
        private DispatcherTimer blockTimer;
        private DateTime blockEndTime;

        public AuthPage()
        {
            InitializeComponent();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            TextBoxLogin.ToolTip = "Введите ваш логин";
            PasswordBox.ToolTip = "Введите ваш пароль";
            ButtonEnter.ToolTip = "Нажмите для входа в систему";
            ButtonReg.ToolTip = "Нажмите для перехода к регистрации";
            ButtonChangePassword.ToolTip = "Нажмите для смены пароля";
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (blockTimer != null && blockTimer.IsEnabled)
            {
                TimeSpan remaining = blockEndTime - DateTime.Now;
                MessageBox.Show($"Вход заблокирован. Осталось {remaining.Seconds} секунд", 
                    "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) || 
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (captchaPanel.Visibility == Visibility.Visible && 
                captchaInput.Text != generatedCaptcha)
            {
                MessageBox.Show("Неверная капча", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                GenerateCaptcha();
                captchaInput.Clear();
                return;
            }

            string hashedPassword = PasswordHelper.HashPassword(PasswordBox.Password);
            var user = Entities.GetContext().Users
                .FirstOrDefault(x => x.Login == TextBoxLogin.Text && x.Password == hashedPassword);

            if (user != null)
            {
                attemptCount = 0;
                captchaPanel.Visibility = Visibility.Collapsed;
                
                Manager.CurrentUser = user;
                
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ButtonBack.Visibility = Visibility.Visible;
                }

                if (user.Role == "Admin")
                {
                    Manager.MainFrame.Navigate(new AdminPage());
                }
                else
                {
                    Manager.MainFrame.Navigate(new UserPage());
                }
            }
            else
            {
                attemptCount++;
                MessageBox.Show($"Неверный логин или пароль. Попытка {attemptCount} из {maxAttempts}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                if (attemptCount >= maxAttempts)
                {
                    ShowCaptcha();
                    if (attemptCount > maxAttempts + 2)
                    {
                        BlockLogin(10);
                    }
                }
            }
        }

        private void ShowCaptcha()
        {
            captchaPanel.Visibility = Visibility.Visible;
            GenerateCaptcha();
            captchaInput.Clear();
        }

        private void GenerateCaptcha()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            generatedCaptcha = "";
            
            for (int i = 0; i < 6; i++)
            {
                generatedCaptcha += chars[random.Next(chars.Length)];
            }
            
            captchaText.Text = "";
            foreach (char c in generatedCaptcha)
            {
                captchaText.Text += c + " ";
            }
            captchaText.Text = captchaText.Text.Trim();
            
            RotateTransform rotate = new RotateTransform(random.Next(-15, 15));
            captchaText.RenderTransform = rotate;
        }

        private void BlockLogin(int seconds)
        {
            blockEndTime = DateTime.Now.AddSeconds(seconds);
            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += (s, args) =>
            {
                if (DateTime.Now >= blockEndTime)
                {
                    blockTimer.Stop();
                    attemptCount = 0;
                    captchaPanel.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Блокировка снята. Можете попробовать войти снова", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };
            blockTimer.Start();
            
            MessageBox.Show($"Превышено количество попыток. Вход заблокирован на {seconds} секунд", 
                "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void refreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
            captchaInput.Clear();
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ChangePassPage());
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtHintLogin.Visibility = string.IsNullOrEmpty(TextBoxLogin.Text) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintPass.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || 
                e.Command == ApplicationCommands.Cut || 
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void TextBoxLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$"))
            {
                e.Handled = true;
            }
        }
    }
}
