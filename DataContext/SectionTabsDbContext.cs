using Microsoft.EntityFrameworkCore;
using BoardCreate.Models;

namespace BoardCreate.DataContext
{
    public class SectionTabsDbContext : DbContext
    {
        public SectionTabsDbContext(DbContextOptions<SectionTabsDbContext> options)
            : base(options) { }

        public DbSet<SectionTabsDbContext> SectionTabs { get; set; }
    }
}
