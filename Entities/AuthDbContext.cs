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
        public DbSet<Nationality> Nationality { get; set; }
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


            // Seed DB
            modelBuilder.Entity<Gender>().HasData(
                new Gender(){ Id=1, Name="Masculino"},
                new Gender(){ Id=2, Name="Femenino"}
            );
            modelBuilder.Entity<MaritalStatus>().HasData(
                new MaritalStatus(){ Id=1, Name="SOLTERO(A)"},
                new MaritalStatus(){ Id=2, Name="CASADO(A)"},
                new MaritalStatus(){ Id=3, Name="DIVORCIADO(A)"},
                new MaritalStatus(){ Id=4, Name="UDO(A)"},
                new MaritalStatus(){ Id=5, Name="UNION LIBRE"}
            );

        }

    }
}