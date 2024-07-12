using Microsoft.EntityFrameworkCore;

namespace ASP_P15.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Entities.User>  Users  { get; set; }
        public DbSet<Entities.Token> Tokens { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
        }
    }
}
