using CustomerService.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
