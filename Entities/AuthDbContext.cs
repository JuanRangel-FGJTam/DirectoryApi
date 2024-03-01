using System;
using Microsoft.EntityFrameworkCore;
using AuthApi.Entities;

namespace AuthApi
{
    
    public class AuthDbContext : DbContext {
        public DbSet<Preregistration> Preregistrations { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Gender> Gender { get; set; }
        public DbSet<MaritalStatus> MaritalStatus { get; set; }
        public DbSet<Notionality> Notionality { get; set; }
        public DbSet<Occupation> Occupation { get; set; }


        public AuthDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .Property( b => b.CreatedAt)
                .HasDefaultValueSql("getDate()");

            modelBuilder.Entity<Person>()
                .Property( b => b.UpdatedAt)
                .HasComputedColumnSql("getDate()");
        }

    }
}