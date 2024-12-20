using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WpfApp4.DataAccess.Data;
using WpfApp4.DataAccess.Data.Models;
using WpfApp4.Desktop.Helpers;

namespace WpfApp4
{
    public partial class PizzaMenu : Window
    {
        private string _userName;
        private ObservableCollection<PizzaItem> _pizzaItems;
        private readonly WpfApp4DbContext _context = new();
        public PizzaMenu(string userName)
        {
            InitializeComponent();
            _userName = userName;
            LoadPizzaMenu();
        }

        private void LoadPizzaMenu()
        {
            try
            {
                // Загрузка пицц из базы данных
                _pizzaItems = new ObservableCollection<PizzaItem>(
                    [.. _context.Pizzas.Select(p => new PizzaItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        ImagePath = p.ImagePath, // string в базу данных
                        Quantity = 0 // Начальное количество всегда 0
                    })]
                );

                // Если в базе нет пицц, используем тестовые данные
                if (_pizzaItems.Count == 0)
                {
                    _pizzaItems = new ObservableCollection<PizzaItem>
                    {
                        new PizzaItem
                        {
                            Id = 1,
                            Name = "Пепперони",
                            Description = "Классическая пицца с пепперони, моцареллой и томатным соусом\nСостав: Пепперони (50г), Моцарелла (120г), Томатный соус (80г)",
                            Price = 599,
                            ImagePath = "/Images/pepperoni.jpg",
                            Quantity = 0
                        },
                        // ... остальные тестовые пиццы
                    };
                }

                PizzaListView.ItemsSource = _pizzaItems;
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке
                MessageBox.Show($"Ошибка загрузки меню: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Используем тестовые данные в случае ошибки
                _pizzaItems = new ObservableCollection<PizzaItem>
            {
                new PizzaItem
                {
                    Id = 1,
                    Name = "Пепперони",
                    Description = "Классическая пицца с пепперони, моцареллой и томатным соусом\nСостав: Пепперони (50г), Моцарелла (120г), Томатный соус (80г)",
                    Price = 599,
                    ImagePath = "/Images/pepperoni.jpg",
                    Quantity = 0
                },
                // ... остальные тестовые пиццы
            };

                PizzaListView.ItemsSource = _pizzaItems;
            }
        }

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int pizzaId = (int)button.Tag;
            var pizza = _pizzaItems.First(p => p.Id == pizzaId);
            pizza.Quantity++;
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int pizzaId = (int)button.Tag;
            var pizza = _pizzaItems.First(p => p.Id == pizzaId);
            if (pizza.Quantity > 0)
                pizza.Quantity--;
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int pizzaId = (int)button.Tag;
            var selectedPizza = _pizzaItems.First(p => p.Id == pizzaId);

            if (selectedPizza.Quantity == 0)
            {
                MessageBox.Show("Пожалуйста, укажите количество пиццы",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Добавление в корзину (сохранение в базу данных) +

            var pizza = _context.Pizzas.Single(p => p.Id == selectedPizza.Id);

            var data = _context.Users.Include(u => u.Cart)
                .Select(u => new
                {
                    u.Id,
                    u.Cart,
                    CartItem = u.Cart.Items.SingleOrDefault(i => i.Pizza == pizza)
                }).Single(u => u.Id == SessionHandler.UserId);

            var cartItem = data.CartItem;

            if (cartItem is null) //а что если, пользователь уже добавлял эту пиццу в корзину?
            {
                cartItem = new CartItem()
                {
                    Pizza = pizza,
                    Quantity = selectedPizza.Quantity
                };
                var cart = data.Cart;

                cart.Items.Add(cartItem);

                _context.Carts.Update(cart);
            }
            else
            {
                cartItem.Quantity += selectedPizza.Quantity;

                _context.CartItems.Update(cartItem);
            }

            _context.SaveChanges();

            MessageBox.Show($"Добавлено в корзину: {selectedPizza.Name} - {selectedPizza.Quantity} шт.",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            selectedPizza.Quantity = 0; // Сбрасываем количество после добавления
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GuestMainMenu mainMenu = new GuestMainMenu(_userName);
            mainMenu.Show();
            this.Close();
        }

        private void GoToCartButton_Click(object sender, RoutedEventArgs e)
        {
            Cart cartWindow = new Cart(_userName);
            cartWindow.Show();
            this.Close();
        }
    }

    public class PizzaItem : System.ComponentModel.INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string ImagePath { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
