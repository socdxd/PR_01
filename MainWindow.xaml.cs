using System;
using System.Windows;
using System.Windows.Threading;
using WPF_Payment_Project.Pages;

namespace WPF_Payment_Project
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            Manager.MainFrame = MainFrame;
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            ButtonBack.Visibility = Visibility.Hidden;
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            ButtonBack.ToolTip = "Вернуться на предыдущую страницу";
            CmbTheme.ToolTip = "Выберите тему оформления";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTimeNow.Text = DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AuthPage());
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из приложения?", 
                "Подтверждение выхода", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                timer?.Stop();
            }
        }

        private void CmbTheme_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CmbTheme.SelectedIndex == 0)
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary { Source = new Uri("Dictionary.xaml", UriKind.Relative) });
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary { Source = new Uri("DictionaryDark.xaml", UriKind.Relative) });
            }
        }
    }
}
