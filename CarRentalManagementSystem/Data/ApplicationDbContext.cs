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
        public DbSet<Models.Car> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Car>()
                .Property(c => c.DailyRate)
                .HasPrecision(18, 2); // 18 total digits, 2 decimal places
        }
    }
}
