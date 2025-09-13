using CarRentalManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Car> Cars { get; set; }  
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Payment> Payments { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    
                    AdminID = new Guid("E82B713C-997C-4E9A-B71C-289A4281AE80"), //  Guid
                    Username = "Admin",
                    Password = "Admin@123",
                    Name = "Administrator",
                    Email = "admin@carental.com"
                }
            );
        }







    }


}

