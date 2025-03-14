using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ExcelDataReader;
using ParseExcelToSqlite.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using ParseExcelToSqlite.Services;
using Microsoft.Extensions.DependencyInjection;
using ParseExcelToSqlite.Seeders; // Add this line to include the DatabaseSeeder class

namespace ParseExcelToSqlite
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ProcessarBaseDeDados1Service.Executar();
            //ProcessarBaseDeDados2.Executar();
            ProcessarDadosDasDuasTabelas();

            // DatabaseDbContext dbContext = new DatabaseDbContext(new DbContextOptions<DatabaseDbContext>());
            // ModelBuilderExtensions.SeedData(dbContext.ModelBuilder);
            // CriarTabelaFinal();
            Console.WriteLine("Data transferred successfully!");
        
        }


        public static void ProcessarDadosDasDuasTabelas()
        {
            //CriarTabelaFinal();
            DatabaseDbContext dbContext = new DatabaseDbContext(new DbContextOptions<DatabaseDbContext>());
            //ModelBuilderExtensions.SeedData(dbContext.);
            Console.WriteLine($"Quant:{dbContext.DistritosConcelhosFreguesias.Count()}");
            var dadosTabela1 = dbContext.DistritosConcelhosFreguesias.ToList();
            var dadosTabela2 = dbContext.Lugares.ToList();
            var dadosAgrupados = dadosTabela1
                .GroupBy(d => new { d.CodDistrito, d.CodConcelho })
                .Select(g => new 
                {
                    CodDistrito = g.Key.CodDistrito,
                    CodConcelho = g.Key.CodConcelho,
                    Total = g.Count(),
                    Registos = g.ToList() // Converte o grupo para uma lista, se você quiser acessar os registros individuais
                }).ToList();
            foreach (var dado in dadosAgrupados)
            {
                Console.WriteLine($"CodDistrito: {dado.CodDistrito}, CodConcelho: {dado.CodConcelho}, Total: {dado.Total}"); 
            }
            // foreach (var dado1 in dadosTabela1)
            // {
            //     foreach (var dado2 in dadosTabela2)
            //     {
            //         if (dado1.NomeDistrito == dado2.NomeDistrito && 
            //             dado1.NomeConcelho == dado2.NomeConcelho && 
            //             dado1.NomeFreguesia == dado2.NomeFreguesia)
            //         {
            //             var novoLugar = new Lugar
            //             {
            //                 NomeDistrito = dado1.NomeDistrito,
            //                 NomeConcelho = dado1.NomeConcelho,
            //                 NomeFreguesia = dado1.NomeFreguesia,
            //                 NomeLugar = dado1.NomeLugar ?? dado2.NomeLugar
            //             };
            //             dbContext.Lugares.Add(novoLugar);
            //         }
            //     }
            // }

            dbContext.SaveChanges();
        }


         public static void CriarTabelaFinal()
        {
            string sqliteConnectionString = @"Data Source=DistritosConcelhosFreguesias.db;Version=3;";
            using (SQLiteConnection sqliteConnection = new SQLiteConnection(sqliteConnectionString))
            {
                sqliteConnection.Open();

                // Create the table if it doesn't exist
                string createTableQuery = @"CREATE TABLE IF NOT EXISTS Local(
                    CodLocal TEXT PRIMARY KEY,
                    Nivel INTEGER,
                    Nome TEXT,
                    DependeDeLocal  TEXT,
                    FOREIGN KEY (DependeDeLocal) REFERENCES Local(CodLocal)
                )";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, sqliteConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CriarTabelaNivelEFazerSeed()
        {
            using (var dbContext = new DatabaseDbContext(new DbContextOptions<DatabaseDbContext>()))
            using (var connection = dbContext.Database.GetDbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // Verifica se a tabela Nivel existe
                    command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Nivel';";
                    var result = command.ExecuteScalar() as long?;
                    var exists = result.HasValue && result.Value > 0;

                    if (!exists)
                    {
                        // Cria a tabela se não existir
                        command.CommandText = @"
                            CREATE TABLE Nivel (
                                Id INTEGER PRIMARY KEY, 
                                Nome TEXT NOT NULL
                            );";
                        command.ExecuteNonQuery();

                        
                    }
                    // Faz o seed dos dados iniciais
                    command.CommandText = @"
                        INSERT INTO Nivel (Id, Nome) VALUES
                        (1, 'Nacional'),
                        (2, 'Região'),
                        (3, 'Distrito'),
                        (4, 'Concelho'),
                        (5, 'União de Freguesias'),
                        (6, 'Freguesia'),
                        (7, 'Lugar');";
                    command.ExecuteNonQuery();
                }
            }

        }
    }

   

}
