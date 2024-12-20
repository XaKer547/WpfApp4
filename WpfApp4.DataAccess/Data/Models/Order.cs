using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp4.DataAccess.Data.Models
{
    public partial class Order
    {
        public int Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime OrderDate { get; set; }
        public string OrderDetails { get; set; }
        public float TotalPrice { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }
        public string Status { get; set; } = "Новый";
        public virtual User User { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();
    }
}