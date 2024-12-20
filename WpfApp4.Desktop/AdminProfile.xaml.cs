using System.Text.RegularExpressions;
using System.Windows;
using WpfApp4.DataAccess.Data;
using WpfApp4.DataAccess.Data.Models;
using WpfApp4.Desktop.Helpers;

namespace WpfApp4
{
    public partial class AdminProfile : Window
    {
        private readonly WpfApp4DbContext _context = new();
        private string _adminName;
        private User _admin;
        public AdminProfile(string adminName) //эти данные спокойно берутся из бд, параметр не нужен
        {
            InitializeComponent();
            _adminName = adminName;
            LoadAdminData();
        }

        private void LoadAdminData()
        {
            _admin = _context.Users.Single(u => u.Id == SessionHandler.UserId);

            // TODO: Загрузка данных из базы данных +
            NameTextBox.Text = _admin.Name;
            EmailTextBox.Text = _admin.Email;
        }

        private void SaveDataButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();

            // Валидация имени
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(name, @"^[a-zA-Zа-яА-Я]+$") || name.Length > 16)
            {
                MessageBox.Show("Имя должно содержать только буквы и быть не длиннее 16 символов",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            // TODO: Сохранение данных в базу данных +
            _admin.Name = name;
            _admin.Email = email;

            _context.Users.Update(_admin);

            _context.SaveChanges();

            MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            _adminName = name;
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Проверка текущего пароля
            if (currentPassword != _admin.Password) // TODO: Проверка в базе данных +
            {
                MessageBox.Show("Неверный текущий пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка нового пароля
            if (!IsValidPassword(newPassword))
            {
                MessageBox.Show("Новый пароль должен содержать минимум:\n" +
                    "- 1 заглавную букву\n" +
                    "- 1 цифру\n" +
                    "- 3 специальных символа",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка подтверждения пароля
            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // TODO: Сохранение нового пароля в базу данных +
            _admin.Password = newPassword;

            _context.Users.Update(_admin);

            _context.SaveChanges();

            MessageBox.Show("Пароль успешно изменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // Очистка полей
            CurrentPasswordBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AdminMainMenu mainMenu = new AdminMainMenu(_adminName);
            mainMenu.Show();
            this.Close();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

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

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            var hasUpperCase = new Regex(@"[A-Z]");
            var hasNumber = new Regex(@"[0-9]");
            var hasSpecialChar = new Regex(@"[!@#$%^&*(),.?""':{}|<>]");

            return hasUpperCase.Matches(password).Count >= 1 &&
                   hasNumber.Matches(password).Count >= 1 &&
                   hasSpecialChar.Matches(password).Count >= 3;
        }
    }
}
