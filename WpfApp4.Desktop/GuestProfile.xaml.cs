using System.Windows;
using WpfApp4.DataAccess.Data;
using WpfApp4.Desktop.Helpers;

namespace WpfApp4
{
    public partial class GuestProfile : Window
    {
        private string _userName;
        private string _userEmail;
        private readonly WpfApp4DbContext _context = new WpfApp4DbContext();
        public GuestProfile(string _userName)
        {
            InitializeComponent();
            _userName = Name;
            LoadUserData();
            LoadOrderHistory();
        }

        private void LoadUserData()
        {
            // TODO: Загрузка данных пользователя из базы данных +
            var user = _context.Users.Single(u => u.Id == SessionHandler.UserId);

            if (user is null) // это никогда не сработает просто так
            {
                MessageBox.Show("Ошибка загрузки данных пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NameTextBlock.Text = user.Name;
            EmailTextBlock.Text = user.Email;
        }

        private void LoadOrderHistory()
        {
            // TODO: Загрузка истории заказов из базы данных +
            try
            {
                var orders = _context.Users.Select(u => new
                {
                    UserId = u.Id,
                    Orders = u.Orders.Select(o => new OrderDTO
                    {
                        Id = o.Id,
                        Date = o.OrderDate,
                        TotalPrice = $"{o.TotalPrice} руб.",
                        Items = string.Join(",", o.Items.Select(i => $"{i.Pizza.Name} - {i.Quantity}"))
                    }).ToList()
                }).Single(o => o.UserId == SessionHandler.UserId);

                OrdersListView.ItemsSource = orders.Orders;
            }
            catch
            {
                MessageBox.Show("Ошибка загрузки данных пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        class OrderDTO
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string OrderDate => Date.ToString("dd.MM.yyyy HH:mm");
            public string Items { get; set; }
            public string TotalPrice { get; set; }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Пожалуйста, заполните оба поля пароля",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidPassword(newPassword))
            {
                MessageBox.Show("Новый пароль должен содержать минимум 1 заглавную букву, 1 цифру и 3 специальных символа",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Найти пользователя по email
            var user = _context.Users.FirstOrDefault(u => u.Email == _userEmail);

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверить старый пароль
            if (user.Password != oldPassword)
            {
                MessageBox.Show("Неверный текущий пароль",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновить пароль
            user.Password = newPassword;

            try
            {
                // Сохранить изменения в базе данных
                _context.SaveChanges();

                MessageBox.Show("Пароль успешно изменен!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистить поля ввода
                OldPasswordBox.Clear();
                NewPasswordBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пароля: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GuestMainMenu mainMenu = new GuestMainMenu(_userName);
            mainMenu.Show();
            this.Close();
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
