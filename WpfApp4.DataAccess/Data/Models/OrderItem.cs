namespace WpfApp4.DataAccess.Data.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public Pizza Pizza { get; set; }
        public int Quantity { get; set; }
    }
}
