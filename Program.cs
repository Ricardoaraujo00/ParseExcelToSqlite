using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ExcelDataReader;
using InovarNasDecisoes.Server.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using ParseExcelToSqlite.Services;

namespace ParseExcelToSqlite
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ProcessarBaseDeDados1.Executar();
            //ProcessarBaseDeDados2.Executar();
            //ProcessarDadosDasDuasTabelas();

            


            Console.WriteLine("Data transferred successfully!");
        
        }


        public void ProcessarDadosDasDuasTabelas()
        {
            CriarTabelaFinal();
            DatabaseDbContext dbContext = new DatabaseDbContext(new DbContextOptions<DatabaseDbContext>());
            var dadosTabela1 = dbContext.Tabela1.ToList();
            var dadosTabela2 = dbContext.Tabela2.ToList();

            foreach (var dado1 in dadosTabela1)
            {
                foreach (var dado2 in dadosTabela2)
                {
                    if (dado1.NomeDistrito == dado2.NomeDistrito && 
                        dado1.NomeConcelho == dado2.NomeConcelho && 
                        dado1.NomeFreguesia == dado2.NomeFreguesia)
                    {
                        var novoLugar = new Lugar
                        {
                            NomeDistrito = dado1.NomeDistrito,
                            NomeConcelho = dado1.NomeConcelho,
                            NomeFreguesia = dado1.NomeFreguesia,
                            NomeLugar = dado1.NomeLugar ?? dado2.NomeLugar
                        };
                        dbContext.Lugares.Add(novoLugar);
                    }
                }
            }

            dbContext.SaveChanges();
        }

         public void CriarTabelaFinal()
        {
            string sqliteConnectionString = @"Data Source=DistritosConcelhosFreguesias.db;Version=3;";
            using (SQLiteConnection sqliteConnection = new SQLiteConnection(sqliteConnectionString))
            {
                sqliteConnection.Open();

                // Create the table if it doesn't exist
                string createTableQuery = @"CREATE TABLE IF NOT EXISTS Lugares(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NomeDistrito TEXT,
                    NomeConcelho TEXT,
                    NomeFreguesia TEXT,
                    NomeLugar TEXT
                )";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, sqliteConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

   

}
