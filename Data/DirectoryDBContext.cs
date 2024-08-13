using System;
using Microsoft.EntityFrameworkCore;
using AuthApi.Entities;
using AuthApi.Data.Utils;

namespace AuthApi.Data
{
    
    public class DirectoryDBContext : DbContext {
        public DbSet<Preregistration> Preregistrations { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Gender> Gender { get; set; }
        public DbSet<MaritalStatus> MaritalStatus { get; set; }
        public DbSet<Nationality> Nationality { get; set; }
        public DbSet<Occupation> Occupation { get; set; }
        public DbSet<ContactType> ContactTypes { get; set; }
        public DbSet<Colony> Colonies { get; set; }
        public DbSet<Municipality> Municipalities { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ContactInformation> ContactInformations { get; set; }
        public DbSet<User> Users {get;set;}
        public DbSet<Session> Sessions {get;set;}
        public DbSet<Area> Area {get;set;}
        public DbSet<ProceedingStatus> ProceedingStatus {get;set;}
        public DbSet<Proceeding> Proceeding {get;set;}


        private readonly ICryptographyService cryptographyService;

        public DirectoryDBContext(DbContextOptions options, ICryptographyService cryptographyService ) : base(options)
        {
            this.cryptographyService = cryptographyService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // * Convert all columns in comel case
            foreach( var entity in modelBuilder.Model.GetEntityTypes() )
            {
                foreach( var property in entity.GetProperties() )
                {
                    var _propertyName = property.Name;
                    property.SetColumnName(  Char.ToLowerInvariant(_propertyName[0]) + _propertyName.Substring(1) );
                }
            }

            // * Person entity
            var personEntity = modelBuilder.Entity<Person>();
            personEntity.Property( p => p.Curp).HasConversion(
                v => cryptographyService.EncryptData(v??""),
                v => cryptographyService.DecryptData(v)
            );
            personEntity.Property( p => p.Rfc).HasConversion(
                v => cryptographyService.EncryptData(v??""),
                v => cryptographyService.DecryptData(v)
            );
            personEntity.Property( p => p.Name).HasConversion(
                v => cryptographyService.EncryptData(v??""),
                v => cryptographyService.DecryptData(v)
            );
            personEntity.Property( p => p.FirstName).HasConversion(
                v => cryptographyService.EncryptData(v??""),
                v => cryptographyService.DecryptData(v)
            );
            personEntity.Property( p => p.LastName).HasConversion(
                v => cryptographyService.EncryptData(v??""),
                v => cryptographyService.DecryptData(v)
            );
            personEntity.Property( p => p.CreatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime");
            personEntity.Property( p => p.UpdatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime")
                .ValueGeneratedOnAddOrUpdate();
                
            // * Address entity
            var addressEntity = modelBuilder.Entity<Address>();
            addressEntity.Property( b => b.CreatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime");
            addressEntity.Property( b => b.UpdatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime")
                .ValueGeneratedOnAddOrUpdate();
            addressEntity.Navigation(n => n.Country).AutoInclude();
            addressEntity.Navigation(n => n.State).AutoInclude();
            addressEntity.Navigation(n => n.Municipality).AutoInclude();
            addressEntity.Navigation(n => n.Colony).AutoInclude();

            // * Contact information entity
            var contactInformation = modelBuilder.Entity<ContactInformation>();
            contactInformation.Property( p => p.Value).HasConversion(
                v => cryptographyService.EncryptData(v??""),
                v => cryptographyService.DecryptData(v)
            );
            contactInformation.Property( b => b.CreatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime");
            contactInformation.Property( b => b.UpdatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime")
                .ValueGeneratedOnAddOrUpdate();
            contactInformation.Navigation( n => n.ContactType ).AutoInclude();

            // * Pre-Registration entity
            var preRegister = modelBuilder.Entity<Preregistration>();
            preRegister.Property( p => p.Password).HasConversion(
                v => cryptographyService.EncryptData(v??""),
                v => cryptographyService.DecryptData(v)
            );

            // * Session entity
            var sessionEntity =  modelBuilder.Entity<Session>();
            sessionEntity.Property( b => b.BegginAt)
                .HasDefaultValueSql("getDate()")
                .ValueGeneratedOnAdd();
            sessionEntity.Navigation(n => n.Person).AutoInclude();

            // * Seed DB
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "System",
                    LastName = "",
                    Email = "system@email.com",
                    Password = cryptographyService.HashData("system")
                }
            );
            base.OnModelCreating(modelBuilder);
            
        }

    }
}