using System.Windows;
using System.Windows.Input;

namespace WpfApp4
{
    public partial class AdminMainMenu : Window
    {
        private string _adminName;
        public AdminMainMenu(string adminName)
        {
            InitializeComponent();

            _adminName = adminName;
            
            AdminNameBlock.Text = _adminName;
        }

        private void ProfileSection_Click(object sender, MouseButtonEventArgs e)
        {
            AdminProfile profileWindow = new AdminProfile(_adminName);
            profileWindow.Show();
            this.Close();
        }

        private void PizzaManagementSection_Click(object sender, MouseButtonEventArgs e)
        {
            PizzaManagement pizzaManagementWindow = new PizzaManagement(_adminName);
            pizzaManagementWindow.Show();
            this.Close();
        }

        private void UserManagementSection_Click(object sender, MouseButtonEventArgs e)
        {
            UserManagement userManagementWindow = new UserManagement(_adminName);
            userManagementWindow.Show();
            this.Close();
        }

        private void OrderManagementSection_Click(object sender, MouseButtonEventArgs e)
        {
            OrderManagement orderManagementWindow = new OrderManagement(_adminName);
            orderManagementWindow.Show();
            this.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти из системы?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }
    }
}
