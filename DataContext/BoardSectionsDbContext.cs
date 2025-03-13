using Microsoft.EntityFrameworkCore;
using BoardCreate.Models.Board;

namespace BoardCreate.DataContext
{
    public class BoardSectionsDbContext : DbContext
    {
        public BoardSectionsDbContext(DbContextOptions<BoardSectionsDbContext> options)
            : base(options) { }

        public DbSet<BoardSectionsModel> BoardSections { get; set; }
    }
}
