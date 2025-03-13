using Microsoft.EntityFrameworkCore;
using BoardCreate.Models.Member;


namespace BoardCreate.DataContext
{
    public class MemberDbContext : DbContext
    {
        public MemberDbContext(DbContextOptions<MemberDbContext> options)
            : base(options) { }

        public DbSet<MemberModel> Member { get; set; }
    }
}
