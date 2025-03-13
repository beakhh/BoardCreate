using BoardCreate.Models.Board;
using Microsoft.EntityFrameworkCore;

namespace BoardCreate.DataContext
{
    public class BoardDbContext : DbContext
    {
        public BoardDbContext(DbContextOptions<BoardDbContext> options)
            : base(options) { }
        public DbSet<BoardModel> Board { get; set; }
    }
}
