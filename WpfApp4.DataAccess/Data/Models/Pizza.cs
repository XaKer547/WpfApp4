using System.ComponentModel.DataAnnotations;
namespace WpfApp4.DataAccess.Data.Models
{
    public  class Pizza
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string ImagePath { get; set; }
    }
}