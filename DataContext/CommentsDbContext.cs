using BoardCreate.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardCreate.DataContext
{
    public class CommentsDbContext : DbContext
    {
        public CommentsDbContext(DbContextOptions<CommentsDbContext> options)
            : base(options) { }
        public DbSet<CommentsModel> Comments { get; set; }
    }
}
