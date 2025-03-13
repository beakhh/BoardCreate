using Microsoft.EntityFrameworkCore;
using BoardCreate.Models;

namespace BoardCreate.DataContext
{
    public class NotificationHub : DbContext
    {
        public NotificationHub(DbContextOptions<NotificationHub> options)
            : base(options) { }

        public DbSet<NotificationHub> UserPreferences { get; set; }
    }
}
