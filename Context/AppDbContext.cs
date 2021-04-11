using Microsoft.EntityFrameworkCore;
using multimedia_storage.Models;

namespace multimedia_storage.Context
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("Server=35.188.138.180;Uid=root;Pwd=leron;Database=vanta_multimedia_storage;Port=3306");

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Multimedia> multimedias { get; set; }
    }
}
