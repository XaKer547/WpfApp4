using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WpfApp4.DataAccess.Data;

namespace WpfApp4
{
    public partial class OrderManagement : Window
    {
        private string _adminName;
        private ObservableCollection<OrderDTO> _orderList;
        private ObservableCollection<OrderDTO> _filteredOrderList;
        private OrderDTO _selectedOrder;
        private readonly WpfApp4DbContext _context = new();
        public OrderManagement(string adminName)
        {
            LoadOrderList();
            InitializeComponent();
            OrderListView.ItemsSource = _filteredOrderList;

            _adminName = adminName;
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void LoadOrderList()
        {
            // TODO: Загрузка списка заказов из базы данных +
            var orders = _context.Orders.Include(o => o.Items)
                .Select(o => new OrderDTO
                {
                    Id = o.Id,
                    Status = o.Status,
                    CustomerName = o.User.Name,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Items = o.Items.Select(i => new OrderItemDTO
                    {
                        Name = i.Pizza.Name,
                        Price = i.Pizza.Price,
                        Quantity = i.Quantity,
                    }).ToList()
                }).ToList();

            _orderList = new ObservableCollection<OrderDTO>(orders);

            _filteredOrderList = new ObservableCollection<OrderDTO>(_orderList);
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
        }

        private void FilterOrders()
        {
            var selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var selectedDate = DateFilterPicker.SelectedDate;

            IEnumerable<OrderDTO> filteredOrders = _orderList;

            // Фильтр по статусу
            if (selectedStatus != "Все заказы")
            {
                filteredOrders = filteredOrders.Where(o => o.Status == selectedStatus);
            }

            // Фильтр по дате
            if (selectedDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o =>
                    o.OrderDate.Date == selectedDate.Value.Date);
            }

            _filteredOrderList.Clear();
            foreach (var order in filteredOrders)
            {
                _filteredOrderList.Add(order);
            }
        }

        private void OrderListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedOrder = OrderListView.SelectedItem as OrderDTO;
            if (_selectedOrder != null)
            {
                // Заполнение деталей заказа
                OrderNumberBlock.Text = _selectedOrder.Id.ToString();
                CustomerBlock.Text = _selectedOrder.CustomerName;
                OrderDateBlock.Text = _selectedOrder.OrderDate.ToString("dd.MM.yyyy");
                TotalAmountBlock.Text = $"{_selectedOrder.TotalPrice} руб.";
                OrderItemsListView.ItemsSource = _selectedOrder.Items;

                // Установка текущего статуса
                OrderStatusComboBox.SelectedItem = OrderStatusComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == _selectedOrder.Status);
            }
        }

        private void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для обновления статуса",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newStatus = (OrderStatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (newStatus == _selectedOrder.Status)
            {
                MessageBox.Show("Статус заказа не изменился",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Проверка логики изменения статуса
            if (!IsValidStatusTransition(_selectedOrder.Status, newStatus))
            {
                MessageBox.Show("Недопустимое изменение статуса заказа",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                OrderStatusComboBox.SelectedItem = OrderStatusComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == _selectedOrder.Status);
                return;
            }

            _selectedOrder.Status = newStatus;

            // TODO: Обновление статуса в базе данных +
            UpdateOrderStatus();

            OrderListView.Items.Refresh();
            MessageBox.Show($"Статус заказа №{_selectedOrder.Id} обновлен на \"{newStatus}\"",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            // Правила перехода статусов
            switch (currentStatus)
            {
                case "Новый":
                    return newStatus == "В обработке" || newStatus == "Отменен";
                case "В обработке":
                    return newStatus == "Готов" || newStatus == "Отменен";
                case "Готов":
                    return newStatus == "Доставлен" || newStatus == "Отменен";
                case "Доставлен":
                case "Отменен":
                    return false; // Финальные статусы
                default:
                    return false;
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для отмены",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedOrder.Status == "Доставлен" || _selectedOrder.Status == "Отменен")
            {
                MessageBox.Show("Невозможно отменить заказ в текущем статусе",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Вы уверены, что хотите отменить заказ №{_selectedOrder.Id}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _selectedOrder.Status = "Отменен";

                // TODO: Обновление статуса в базе данных +
                UpdateOrderStatus();

                OrderListView.Items.Refresh();
                MessageBox.Show($"Заказ №{_selectedOrder.Id} отменен",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateOrderStatus()
        {
            var order = _context.Orders.Single(o => o.Id == _selectedOrder.Id);

            order.Status = _selectedOrder.Status;

            _context.Orders.Update(order);

            _context.SaveChanges();
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AdminMainMenu mainMenu = new AdminMainMenu(_adminName);
            mainMenu.Show();
            this.Close();
        }
    }

    public class OrderDTO
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public float TotalPrice { get; set; }
        public string Status { get; set; }
        public IReadOnlyCollection<OrderItemDTO> Items { get; set; }
    }

    public class OrderItemDTO
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
    }
}
