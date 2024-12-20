using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp4.DataAccess.Data;
using WpfApp4.DataAccess.Data.Models;

namespace WpfApp4
{
    public partial class PizzaManagement : Window
    {
        private string _adminName;
        private ObservableCollection<Pizza> _pizzaList;
        private Pizza _selectedPizza;
        private readonly WpfApp4DbContext _context = new();
        public PizzaManagement(string adminName)
        {
            InitializeComponent();
            _adminName = adminName;
            LoadPizzaList();
        }

        private void LoadPizzaList()
        {
            // TODO: Загрузка списка пицц из базы данных +
            _pizzaList = new ObservableCollection<Pizza>([.. _context.Pizzas]);

            PizzaListView.ItemsSource = _pizzaList;
        }

        private void PizzaListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPizza = PizzaListView.SelectedItem as Pizza;
            if (_selectedPizza != null)
            {
                NameTextBox.Text = _selectedPizza.Name;
                DescriptionTextBox.Text = _selectedPizza.Description;
                PriceTextBox.Text = _selectedPizza.Price.ToString();
                ImagePathTextBox.Text = _selectedPizza.ImagePath;
                LoadPreviewImage(_selectedPizza.ImagePath);
            }
        }

        private void SavePizza_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название пиццы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Введите описание пиццы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!float.TryParse(PriceTextBox.Text, out float price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ImagePathTextBox.Text))
            {
                MessageBox.Show("Выберите изображение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создание или обновление пиццы
            if (_selectedPizza == null)
            {
                // Создание новой пиццы
                var newPizza = new Pizza
                {
                    //Id = _pizzaList.Count + 1, // TODO: Получение ID из базы данных
                    //EF сам генерирует Id

                    Name = NameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Price = price,
                    ImagePath = ImagePathTextBox.Text
                };

                _pizzaList.Add(newPizza);

                _context.Pizzas.Add(newPizza);
            }
            else
            {
                // Обновление существующей пиццы
                _selectedPizza.Name = NameTextBox.Text;
                _selectedPizza.Description = DescriptionTextBox.Text;
                _selectedPizza.Price = price;
                _selectedPizza.ImagePath = ImagePathTextBox.Text;
                PizzaListView.Items.Refresh();
            }

            // TODO: Сохранение в базу данных +

            _context.SaveChanges();

            MessageBox.Show("Пицца успешно сохранена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm_Click(null, null);
        }

        private void DeletePizza_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var pizzaId = (int)button.Tag;
            var pizza = _pizzaList[pizzaId - 1];

            if (MessageBox.Show($"Вы уверены, что хотите удалить пиццу \"{pizza.Name}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _pizzaList.Remove(pizza);

                // TODO: Удаление из базы данных +

                _context.Pizzas.Remove(pizza);

                MessageBox.Show("Пицца успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm_Click(null, null);
            }
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png|Все файлы|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedPath = openFileDialog.FileName;
                ImagePathTextBox.Text = selectedPath;
                LoadPreviewImage(selectedPath);
            }
        }

        private void LoadPreviewImage(string imagePath)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                image.EndInit();
                PreviewImage.Source = image;
            }
            catch (Exception)
            {
                PreviewImage.Source = null;
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            NameTextBox.Clear();
            DescriptionTextBox.Clear();
            PriceTextBox.Clear();
            ImagePathTextBox.Clear();
            PreviewImage.Source = null;
            _selectedPizza = null;
            PizzaListView.SelectedItem = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AdminMainMenu mainMenu = new AdminMainMenu(_adminName);
            mainMenu.Show();
            this.Close();
        }
    }
}
