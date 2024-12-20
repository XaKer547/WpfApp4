namespace WpfApp4.DataAccess.Data.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public Pizza Pizza { get; set; }
        public int Quantity { get; set; }
        public Cart Cart { get; set; }
    }
}