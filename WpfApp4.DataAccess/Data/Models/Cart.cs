namespace WpfApp4.DataAccess.Data.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public ICollection<CartItem> Items { get; set; } = new HashSet<CartItem>();
    }
}
