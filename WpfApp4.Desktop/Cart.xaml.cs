using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WpfApp4.DataAccess.Data;
using WpfApp4.DataAccess.Data.Models;
using WpfApp4.Desktop.Helpers;

namespace WpfApp4
{
    public partial class Cart : Window
    {
        private string _userName;
        private ObservableCollection<CartItemDTO> _cartItems;
        private readonly WpfApp4DbContext _context = new();
        public Cart(string userName)
        {
            InitializeComponent();
            _userName = userName;
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            // TODO: Загрузка товаров из базы данных +
            var cart = _context.Users.Select(c => new
            {
                UserId = c.Id,
                CartItems = c.Cart.Items.Select(i => new CartItemDTO
                {
                    Id = i.Id,
                    Description = i.Pizza.Description,
                    ImagePath = i.Pizza.ImagePath,
                    Quantity = i.Quantity,
                    PizzaId = i.Pizza.Id,
                    Name = i.Pizza.Name,
                    Price = i.Pizza.Price,
                }).ToList()
            }).Single(c => c.UserId == SessionHandler.UserId);

            _cartItems = new ObservableCollection<CartItemDTO>(cart.CartItems);

            /*_cartItems = new ObservableCollection<CartItemDTO>
            //{
            //    new CartItemDTO
            //    {
            //        Id = 1,
            //        Name = "Пепперони",
            //        Description = "Пепперони (50г), Моцарелла (120г), Томатный соус (80г)",
            //        Price = 599,
            //        ImagePath = "/Images/pepperoni.jpg",
            //        Quantity = 2
            //    },
            //    new CartItemDTO
            //    {
            //        Id = 2,
            //        Name = "4 сыра",
            //        Description = "Моцарелла (80г), Горгонзола (40г), Пармезан (40г), Чеддер (40г)",
            //        Price = 699,
            //        ImagePath = "/Images/four_cheese.jpg",
            //        Quantity = 1
            //    }
            };*/

            CartItemsControl.ItemsSource = _cartItems;

            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            var total = _cartItems.Sum(item => item.Sum);

            TotalPriceBlock.Text = $"{total} руб.";
        }

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int itemId = (int)button.Tag;
            var item = _cartItems.First(i => i.Id == itemId);
            item.Quantity++;
            UpdateTotalPrice();
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int itemId = (int)button.Tag;
            var item = _cartItems.First(i => i.Id == itemId);
            if (item.Quantity > 1)
            {
                item.Quantity--;
                UpdateTotalPrice();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int itemId = (int)button.Tag;
            var item = _cartItems.First(i => i.Id == itemId);

            if (MessageBox.Show($"Удалить {item.Name} из корзины?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _cartItems.Remove(item);

                var cartItem = _context.CartItems.First(i => i.Id == item.Id);

                _context.CartItems.Remove(cartItem);

                //_context.SaveChanges(); сохранение в БД?

                UpdateTotalPrice();
            }
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_cartItems.Any())
            {
                MessageBox.Show("Корзина пуста", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Определяем выбранный способ оплаты
            string paymentMethod = CashRadioButton.IsChecked == true ? "Наличные" :
                                 CardRadioButton.IsChecked == true ? "Банковская карта" :
                                 "Криптовалюта";

            var orderDetails = string.Join("\n", _cartItems.Select(item =>
                    $"{item.Name} x{item.Quantity} - {item.Price * item.Quantity} руб."));

            var items = new List<OrderItem>();

            foreach (var item in _cartItems)
            {
                var cartItem = _context.CartItems.Include(c => c.Pizza).Single(c => c.Id == item.Id);

                items.Add(new OrderItem()
                {
                    Pizza = cartItem.Pizza,
                    Quantity = cartItem.Quantity,
                });
            }

            // TODO: Сохранение заказа в базу данных +
            var order = new Order
            {
                OrderDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                Items = items,
                TotalPrice = _cartItems.Sum(item => item.Sum),
                OrderDetails = orderDetails,
            };

            var user = _context.Users.Include(u => u.Cart)
                .Single(u => u.Id == SessionHandler.UserId);

            user.Orders.Add(order);

            user.Cart.Items.Clear();

            _context.Users.Update(user);

            _context.SaveChanges();

            MessageBox.Show($"Заказ успешно создан!\n\n" +
                          $"Способ оплаты: {paymentMethod}\n" +
                          $"Детали заказа:\n{orderDetails}\n\n" +
                          $"Итого: {order.TotalPrice} руб.",
                          "Заказ создан", MessageBoxButton.OK, MessageBoxImage.Information);

            // Возвращаемся в главное меню
            GuestMainMenu mainMenu = new GuestMainMenu(_userName);
            mainMenu.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GuestMainMenu mainMenu = new GuestMainMenu(_userName);
            mainMenu.Show();
            this.Close();
        }
    }

    public class CartItemDTO : System.ComponentModel.INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PizzaId { get; set; }
        public float Price { get; set; }
        public string ImagePath { get; set; }
        public float Sum => Price * Quantity;

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
