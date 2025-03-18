using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System.Reflection;
using ParseExcelToSqlite.Models;

namespace ParseExcelToSqlite.Controllers
{
    public class DatabaseDbContext : DbContext
    {
        public DatabaseDbContext(DbContextOptions<DatabaseDbContext> options) : base(options)
        {
        }

        public DbSet<Local> Locais { get; set; }
        public DbSet<DistritosConcelhosFreguesias> DistritosConcelhosFreguesias { get; set; }
        public DbSet<Lugar> Lugares { get; set; }
        public DbSet<Nivel> Niveis { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(
                    connectionString: "Filename=DistritosConcelhosFreguesias.db",
                    sqliteOptionsAction: op =>
                    {
                        op.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    });
            }
            //optionsBuilder.UseSqlite(connectionString: "Filename=DistritosConcelhosFreguesias.db",
            //        sqliteOptionsAction: op =>
            //        {
            //            op.MigrationsAssembly(
            //                Assembly.GetExecutingAssembly().FullName);
            //        });
            base.OnConfiguring(optionsBuilder);

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data if the table is empty
            ModelBuilderExtensions.SeedData(modelBuilder);
        }
    }

    public static class ModelBuilderExtensions
    {
        public static void SeedData(this ModelBuilder modelBuilder)
        {
            Seed_Nivel(modelBuilder);
        }

        public static void Seed_Nivel(ModelBuilder modelBuilder)
        {
            //if (!modelBuilder.Model.GetEntityTypes().Any(e => e.ClrType == typeof(Nivel)))
            //{
                modelBuilder.Entity<Nivel>().HasData(
                    new Nivel
                    {
                        Id = 1,
                        Nome= "Nacional"
                    },
                    new Nivel
                    {
                        Id = 2,
                        Nome = "Região"
                    },
                    new Nivel
                    {
                        Id = 3,
                        Nome = "Distrito"
                    },
                    new Nivel
                    {
                        Id = 4,
                        Nome = "Concelho"
                    },
                    new Nivel
                    {
                        Id = 5,
                        Nome = "União de Freguesias"
                    },
                    new Nivel
                    {
                        Id = 6,
                        Nome = "Freguesia"
                    }, 
                    new Nivel
                    {
                        Id = 7,
                        Nome = "Freguesia extinta"
                    },
                    new Nivel
                    {
                        Id = 8,
                        Nome = "Lugar"
                    }
                );
            //}
        }
    }
}

