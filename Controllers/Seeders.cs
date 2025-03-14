using System;
using ParseExcelToSqlite.Controllers;
using ParseExcelToSqlite.Models;

namespace ParseExcelToSqlite.Seeders
{
    public static class DatabaseSeeder
    {
        public static void SeedData(DatabaseDbContext dbContext)
        {
            // Add your seeding logic here
            if (!dbContext.Nivel.Any())
            {
                dbContext.Nivel.AddRange(
                    new Nivel { Nome = "Iniciante" ,},
                    new Nivel { Nome = "Intermediário" },
                    new Nivel { Nome = "Avançado" }
                );
                dbContext.SaveChanges();
            }

             if (!dbContext.Nivel.Any()) // Evita duplicações
            {
                dbContext.Nivel.AddRange(
                    new Nivel { Id = 1, Nome = "Nacional" },
                    new Nivel { Id = 2, Nome = "Região" },
                    new Nivel { Id = 3, Nome = "Distrito" },
                    new Nivel { Id = 4, Nome = "Concelho" },
                    new Nivel { Id = 5, Nome = "União de Freguesias" },
                    new Nivel { Id = 6, Nome = "Freguesia" },
                    new Nivel { Id = 7, Nome = "Lugar" }
                );

                dbContext.SaveChanges();
            }
        }
    }
}