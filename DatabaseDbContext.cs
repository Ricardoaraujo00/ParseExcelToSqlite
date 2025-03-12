using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using InovarNasDecisoes.Shared.Models;
using System.Reflection;

namespace InovarNasDecisoes.Server.Controllers
{
    public class DatabaseDbContext : DbContext
    {
        public DatabaseDbContext(DbContextOptions<DatabaseDbContext> options) : base(options)
        {
        }

        public DbSet<Local> Local { get; set; }
        public DbSet<LocalDb1> LocalDb1 { get; set; }
        public DbSet<LocalDb2> LocalDb2 { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString: "Filename=DistritosConcelhosFreguesias.db",
                    sqliteOptionsAction: op =>
                    {
                        op.MigrationsAssembly(
                            Assembly.GetExecutingAssembly().FullName);
                    });
            base.OnConfiguring(optionsBuilder);
        }
    }
}

