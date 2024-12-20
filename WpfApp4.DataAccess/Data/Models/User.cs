using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp4.DataAccess.Data.Models
{


    public class User
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [EmailAddress]
        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Password { get; set; }

        [StringLength(50)]
        public string Role { get; set; }

        public Cart Cart { get; set; } = new Cart();
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}