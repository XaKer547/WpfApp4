using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WpfApp4.DataAccess.Data;

namespace WpfApp4
{
    public partial class UserManagement : Window
    {
        private string _adminName;
        private ObservableCollection<UserDTO> _userList;
        private ObservableCollection<UserDTO> _filteredUserList;
        private UserDTO _selectedUser;
        private readonly WpfApp4DbContext _context = new();
        public UserManagement(string adminName)
        {
            LoadUserList();
            InitializeComponent();
            _adminName = adminName;
            UserListView.ItemsSource = _filteredUserList;
        }
        public class UserDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }

        private void LoadUserList()
        {
            // TODO: Загрузка списка пользователей из базы данных +
            var users = _context.Users.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
            }).ToList();

            _userList = new ObservableCollection<UserDTO>(users);

            _filteredUserList = new ObservableCollection<UserDTO>(_userList);

        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Поиск по имени или email")
                SearchTextBox.Text = "";
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                SearchTextBox.Text = "Поиск по имени или email";
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterUsers();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterUsers();
        }

        private void FilterUsers()
        {
            string searchText = SearchTextBox.Text.ToLower();
            if (searchText == "поиск по имени или email") searchText = "";

            _filteredUserList.Clear();
            foreach (var user in _userList.Where(u =>
                u.Name.ToLower().Contains(searchText) ||
                u.Email.ToLower().Contains(searchText)))
            {
                _filteredUserList.Add(user);
            }
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = UserListView.SelectedItem as UserDTO;
            if (_selectedUser != null)
            {
                NameTextBox.Text = _selectedUser.Name;
                EmailTextBox.Text = _selectedUser.Email;
                RoleComboBox.SelectedItem = RoleComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == _selectedUser.Role);
                PasswordBox.Password = string.Empty;
            }
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация имени
            string name = NameTextBox.Text.Trim();
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
            string email = EmailTextBox.Text.Trim();
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка дублирования email
            if (_userList.Any(u => u.Email == email && u.Id != _selectedUser.Id))
            {
                MessageBox.Show("Пользователь с таким email уже существует",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Обновление данных пользователя
            _selectedUser.Name = name;
            _selectedUser.Email = email;
            _selectedUser.Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var user = _context.Users.Single(u => u.Id == _selectedUser.Id);

            user.Name = _selectedUser.Name;
            user.Email = _selectedUser.Email;
            user.Role = _selectedUser.Role;

            // Обновление пароля если введен
            if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                if (!IsValidPassword(PasswordBox.Password))
                {
                    MessageBox.Show("Новый пароль должен содержать минимум:\n" +
                        "- 1 заглавную букву\n" +
                        "- 1 цифру\n" +
                        "- 3 специальных символа",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // TODO: Обновление пароля в базе данных +
                user.Password = PasswordBox.Password;
            }

            // TODO: Сохранение в базу данных +
            _context.Users.Update(user);

            _context.SaveChanges();


            UserListView.Items.Refresh();
            MessageBox.Show("Данные пользователя успешно обновлены",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm_Click(null, null);
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var userId = (int)button.Tag;
            var selectedUser = _userList.First(u => u.Id == userId);

            if (selectedUser.Role == "Администратор")
            {
                MessageBox.Show("Невозможно удалить администратора",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Вы уверены, что хотите удалить пользователя {selectedUser.Name}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _userList.Remove(selectedUser);
                _filteredUserList.Remove(selectedUser);
                // TODO: Удаление из базы данных +
                var user = _context.Users.Single(u => u.Id == selectedUser.Id);

                _context.Users.Remove(user);

                _context.SaveChanges();

                MessageBox.Show("Пользователь успешно удален",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm_Click(null, null);
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            NameTextBox.Clear();
            EmailTextBox.Clear();
            PasswordBox.Clear();
            RoleComboBox.SelectedIndex = -1;
            _selectedUser = null;
            UserListView.SelectedItem = null;
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
