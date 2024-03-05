using System;
using Microsoft.EntityFrameworkCore;
using AuthApi.Entities;

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

        private readonly ICryptographyService cryptographyService;

        public DirectoryDBContext(DbContextOptions options, ICryptographyService cryptographyService ) : base(options)
        {
            this.cryptographyService = cryptographyService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            
            // Convert all columns in comel case
            foreach( var entity in modelBuilder.Model.GetEntityTypes() )
            {
                foreach( var property in entity.GetProperties() )
                {
                    var _propertyName = property.Name;
                    property.SetColumnName(  Char.ToLowerInvariant(_propertyName[0]) + _propertyName.Substring(1) );
                }
            }

            // Person entity
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
            personEntity.Property( p => p.CreatedAt).HasDefaultValueSql("getDate()");
            personEntity.Property( p => p.UpdatedAt).HasComputedColumnSql("getDate()");


            modelBuilder.Entity<Address>().Property( b => b.CreatedAt).HasComputedColumnSql("getDate()");
            modelBuilder.Entity<Address>().Property( b => b.UpdatedAt).HasComputedColumnSql("getDate()");

            modelBuilder.Entity<ContactInformation>().Property( b => b.CreatedAt).HasComputedColumnSql("getDate()");
            modelBuilder.Entity<ContactInformation>().Property( b => b.UpdatedAt).HasComputedColumnSql("getDate()");

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
            modelBuilder.Entity<ContactType>().HasData(
                new ContactType(){ Id=1, Name="TELEFONO CELULAR" },
                new ContactType(){ Id=2, Name="TELEFONO DE CASA" },
                new ContactType(){ Id=3, Name="TELEFONO DE TRABAJO" },
                new ContactType(){ Id=4, Name="CORREO ELECTRONICO" }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "System",
                    LastName = "",
                    Username = "System",
                    Password = "System",
                }
            );

            base.OnModelCreating(modelBuilder);
            
        }

        

    }
}