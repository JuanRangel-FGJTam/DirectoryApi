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
        public DbSet<ProceedingFile> ProceedingFiles {get;set;}
        public DbSet<DocumentType> DocumentTypes {get;set;}
        public DbSet<AccountRecoveryFile> AccountRecoveryFiles {get;set;}
        public DbSet<AccountRecovery> AccountRecoveryRequests {get;set;}
        public DbSet<Role> Roles {get;set;}
        public DbSet<UserRole> UserRoles {get;set;}
        public DbSet<UserClaim> UserClaims {get;set;}

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
            var sessionEntity = modelBuilder.Entity<Session>();
            sessionEntity.Property( b => b.BegginAt)
                .HasDefaultValueSql("getDate()")
                .ValueGeneratedOnAdd();
            sessionEntity.Navigation(n => n.Person).AutoInclude();

            // * Procedding Entity
            var proccedingEntity = modelBuilder.Entity<Proceeding>();
            proccedingEntity.Property( b => b.CreatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime");
            proccedingEntity.Property( b => b.UpdatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime")
                .ValueGeneratedOnAddOrUpdate();
            proccedingEntity.HasMany(p => p.Files)
                .WithOne( f => f.Proceeding)
                .HasForeignKey(f => f.ProceedingId);

            // * Procedding File Entity
            var proceddingFileEntity = modelBuilder.Entity<ProceedingFile>();
            proceddingFileEntity.Property( b => b.CreatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime");
            proceddingFileEntity.Property( b => b.UpdatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime")
                .ValueGeneratedOnAddOrUpdate();
            
            // * Roles Entity
            var roleEntity = modelBuilder.Entity<Role>( entity => {
                entity.HasKey( r => r.Id);
                entity.HasData(
                    new Role{ Id = 1, Name = "Admin", Description = "Has access to all system features." },
                    new Role{ Id = 2, Name = "User", Description = "Can view the people but not modify it" },
                    new Role{ Id = 3, Name = "Manager", Description = "Can view and modify it the people." }
                );
            });


            // * User Roles Entity
            var userRoleEntity = modelBuilder.Entity<UserRole>( entity => {
                entity.HasKey( ur => ur.Key);

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property( ur => ur.Key).IsRequired();
            });


            // * User Claim Entity
            var userClaimEnity = modelBuilder.Entity<UserClaim>( entity => {
                entity.HasKey( uc => uc.Id);

                entity.HasOne(uc => uc.User)
                    .WithMany(u => u.UserClaims)
                    .HasForeignKey(uc => uc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property( uc => uc.Id).IsRequired();
            });

            // * Document Types
            var documentTypeEntity = modelBuilder.Entity<DocumentType>();
            documentTypeEntity.Property(b => b.CreatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime2");
            documentTypeEntity.Property(b => b.UpdatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime2");
            documentTypeEntity.HasData(
                new DocumentType {Id = 1, Name = "INE" },
                new DocumentType {Id = 2, Name = "CURP" },
                new DocumentType {Id = 3, Name = "Acta de nacimiento" },
                new DocumentType {Id = 4, Name = "Pasaporte" }
            );

            // * Account Recovery Files
            var accountRecoveryFile = modelBuilder.Entity<AccountRecoveryFile>();
            accountRecoveryFile.Property(b => b.CreatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime2");

            // * Account Recovery Requests
            var accountRecovery = modelBuilder.Entity<AccountRecovery>();
            accountRecovery.Property(b => b.CreatedAt)
                .HasDefaultValueSql("getDate()")
                .HasColumnType("datetime2");


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