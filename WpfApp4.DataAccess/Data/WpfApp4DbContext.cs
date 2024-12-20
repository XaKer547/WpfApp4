using Microsoft.EntityFrameworkCore;
using WpfApp4.DataAccess.Data.Models;

namespace WpfApp4.DataAccess.Data
{
    public class WpfApp4DbContext : DbContext
    {
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItem> OrderItem { get; set; }
        public virtual DbSet<Pizza> Pizzas { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var serverName = "b2-225-002\\SQLEXPRESS"; //Пиши тот, который в SQL Server выводится как 'Имя сервера'

            optionsBuilder.UseSqlServer($"Server={serverName};Database=WpfApp4;TrustServerCertificate=true;Trusted_connection=true");

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pizza>()
                .HasData(new Pizza
                {
                    Id = 1,
                    Name = "Пепперони",
                    Description = "Пепперони (50г), Моцарелла (120г), Томатный соус (80г)",
                    Price = 599,
                    ImagePath = "/Images/pepperoni.jpg"
                },
                new Pizza
                {
                    Id = 2,
                    Name = "4 сыра",
                    Description = "Моцарелла (80г), Горгонзола (40г), Пармезан (40г), Чеддер (40г)",
                    Price = 699,
                    ImagePath = "/Images/four_cheese.jpg",
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}
