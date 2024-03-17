using ChatDB;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public class MyAppContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public MyAppContext()
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies()
                .UseNpgsql("Host=localhost;Username=postgres;Password=example;Database=ChatApp");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(user => user.ID);
                entity.Property(user => user.ID);
                entity.Property(user => user.Login).HasColumnName("name").HasMaxLength(255);

                entity.HasMany(user => user.IncomingMessages).WithOne(message => message.Consumer)
                      .HasForeignKey(message => message.ConsumerID).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(user => user.OutgoingMessages).WithOne(message => message.Autor)
                      .HasForeignKey(message => message.AutorID).OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(message => message.ID);
                entity.Property(message => message.Text).HasColumnName("text");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
