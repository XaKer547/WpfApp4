using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WpfApp4.DataAccess.Data;
using WpfApp4.Desktop.Helpers;

namespace WpfApp4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WpfApp4DbContext _context = new();
        public MainWindow()
        {
            InitializeComponent();
            EmailTextBox.Text = "Введите email";
            EmailTextBox.GotFocus += RemoveText;
            EmailTextBox.LostFocus += AddText;
        }

        private void RemoveText(object sender, EventArgs e)
        {
            if (EmailTextBox.Text == "Введите email")
                EmailTextBox.Text = "";
        }

        private void AddText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                EmailTextBox.Text = "Введите email";
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            // Валидация email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Пожалуйста, введите корректный email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация пароля
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Проверка учетных данных в базе данных +
            var user = _context.Users.SingleOrDefault(u => u.Email == email && u.Password == password);

            if (user is null)
            {
                MessageBox.Show("Неверный email или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            SessionHandler.SetSession(user.Id, user.Role);

            if (user.Role == "Администратор")
            {
                AdminMainMenu adminMenu = new AdminMainMenu(user.Name);
                adminMenu.Show();
                this.Close();
            }

            else if (user.Role == "Гость")
            {
                GuestMainMenu guestMenu = new GuestMainMenu(user.Name);
                guestMenu.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неизвестная роль пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); 
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (email == "Введите email") return false;

            try
            {
                // Проверка на наличие русских букв
                if (Regex.IsMatch(email, @"[а-яА-Я]"))
                    return false;

                // Базовая проверка формата email
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email) && email.Length <= 30;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
