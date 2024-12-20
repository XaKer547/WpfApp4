using System.Windows;

namespace WpfApp4
{
    public partial class GuestMainMenu : Window
    {
        private string _userName;

        public GuestMainMenu(string userName)
        {
            InitializeComponent();
            _userName = userName;
            UserNameBlock.Text = userName;
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно личного кабинета
            GuestProfile profileWindow = new GuestProfile(_userName);
            profileWindow.Show();
            this.Close();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно меню пиццерии
            PizzaMenu menuWindow = new PizzaMenu(_userName);
            menuWindow.Show();
            this.Close();
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно корзины
            Cart cartWindow = new Cart(_userName);
            cartWindow.Show();
            this.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся к окну авторизации
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }
    }
}
