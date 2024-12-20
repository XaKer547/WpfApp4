using System.Text.RegularExpressions;
using System.Windows;
using WpfApp4.DataAccess.Data;
using WpfApp4.DataAccess.Data.Models;

namespace WpfApp4
{
    public partial class RegistrationWindow : Window
    {
        private readonly WpfApp4DbContext _context = new();
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные из полей
            string name = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            bool isGuest = GuestRadioButton.IsChecked ?? true;

            if (!IsValidName(name))
            {
                MessageBox.Show("Имя должно содержать только буквы и быть не длиннее 16 символов",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Email должен содержать символ @, не содержать русских букв и быть не длиннее 30 символов",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация пароля
            if (!IsValidPassword(password))
            {
                MessageBox.Show("Пароль должен содержать минимум 1 заглавную букву, 1 цифру и 3 специальных символа",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Добавить сохранение данных в базу +
            var user = new User
            {
                Name = name,
                Email = email,  // Используем email вместо Name
                Password = password,
                Role = isGuest ? "Гость" : "Администратор"  // Преобразуем булево значение в строку роли
            };

            // Добавление пользователя в базу данных и сохранение изменений

            _context.Users.Add(user);

            _context.SaveChanges();

            MessageBox.Show("Регистрация будет добавлена после подключения базы данных",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

            // Возвращаемся к окну входа
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (name.Length > 16) return false;

            return Regex.IsMatch(name, @"^[a-zA-Zа-яА-Я]+$");
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (email.Length > 30) return false;

            // Проверка на наличие русских букв
            if (Regex.IsMatch(email, @"[а-яА-Я]"))
                return false;

            // Проверка формата email
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            // Проверка на наличие заглавной буквы
            bool hasUpperCase = password.Any(char.IsUpper);

            // Проверка на наличие цифры
            bool hasDigit = password.Any(char.IsDigit);

            // Проверка на наличие специальных символов
            int specialCharCount = password.Count(c => !char.IsLetterOrDigit(c));

            return hasUpperCase && hasDigit && specialCharCount >= 3;
        }
    }
}
