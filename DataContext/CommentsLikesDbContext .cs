using BoardCreate.Models;
using BoardCreate.Models.Board;
using Microsoft.EntityFrameworkCore;

namespace BoardCreate.DataContext
{
    public class CommentsLikesDbContext : DbContext
    {
        public CommentsLikesDbContext(DbContextOptions<CommentsLikesDbContext> options)
            : base(options) { }
        public DbSet<CommentsLikesModel> CommentsLikes { get; set; }
    }
}
