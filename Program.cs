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
using ParseExcelToSqlite.Seeders;
using ParseExcelToSqlite.Models; // Add this line to include the DatabaseSeeder class

namespace ParseExcelToSqlite
{
    
    internal class Program
    {
        public class Lugar
        {
            public string Nome { get; set; }
        }
        public class Freguesia
        {
            public string Nome { get; set; }
            public List<Lugar> Lugares { get; set; } = new List<Lugar>();
        }

        public class Concelho
        {
            public string Nome { get; set; }
            public List<Freguesia> Freguesias { get; set; } = new List<Freguesia>();
        }

        public class Distrito
        {
            public string Nome { get; set; }
            public List<Concelho> Concelhos { get; set; } = new List<Concelho>();
        }
        static void Main(string[] args)
        {
            //ProcessarBaseDeDados1Service.Executar();
           //ProcessarBaseDeDados2Service.Executar();
            //ProcessarDadosDasDuasTabelas();


            

            
            Console.WriteLine("Data transferred successfully!");
        
        }


        public static void ProcessarDadosDasDuasTabelas()
        {
            var listaDistritos2 = ObterEstruturaDadosTabela2();
            var listaDistritos1 = ObterEstruturaDadosTabela1();
            Local locais = new Local();

            foreach (var distrito in listaDistritos2)
            {
                Console.WriteLine($"Distrito: {distrito.Nome}");
                
                Local localDistrito = new Local()
                {
                     
                }
                foreach (var concelho in distrito.Concelhos)
                {
                    Console.WriteLine($"  Concelho: {concelho.Nome}");
                    foreach (var freguesia in concelho.Freguesias)
                    {
                        Console.WriteLine($"    Freguesia: {freguesia.Nome}");
                    }
                }
            }

        }

        public static List<Distrito> ObterEstruturaDadosTabela1()
        {
            //CriarTabelaFinal();
            DatabaseDbContext dbContext = new DatabaseDbContext(new DbContextOptions<DatabaseDbContext>());
            
            // Obter os dados da tabela original
            var dados = dbContext.DistritosConcelhosFreguesias.ToList();

            // Criar um dicionário para evitar duplicações e facilitar a busca
            var distritos = new Dictionary<string, Distrito>();

            foreach (var dado in dados)
            {
                // Verifica se o distrito já foi adicionado
                if (!distritos.ContainsKey(dado.NomeDistrito))
                {
                    distritos[dado.NomeDistrito] = new Distrito { Nome = dado.NomeDistrito };
                }

                var distrito = distritos[dado.NomeDistrito];

                // Verifica se o concelho já existe dentro do distrito
                var concelho = distrito.Concelhos.FirstOrDefault(c => c.Nome == dado.NomeConcelho);
                if (concelho == null)
                {
                    concelho = new Concelho { Nome = dado.NomeConcelho };
                    distrito.Concelhos.Add(concelho);
                }

                // Adiciona a freguesia dentro do concelho correspondente
                concelho.Freguesias.Add(new Freguesia { Nome = dado.NomeFreguesia });
            }

            // Lista final com todos os distritos e suas hierarquias
            var listaDistritos = distritos.Values.ToList();
            return listaDistritos;
        }

        public static List<Distrito> ObterEstruturaDadosTabela2()
        {
            //CriarTabelaFinal();
            DatabaseDbContext dbContext = new DatabaseDbContext(new DbContextOptions<DatabaseDbContext>());
            
            // Obter os dados da tabela original
            var dados = dbContext.Lugares.ToList();

            // Criar um dicionário para evitar duplicações e facilitar a busca
            var distritos = new Dictionary<string, Distrito>();

            foreach (var dado in dados)
            {
                // Verifica se o distrito já foi adicionado
                if (!distritos.ContainsKey(dado.NomeDistrito))
                {
                    distritos[dado.NomeDistrito] = new Distrito { Nome = dado.NomeDistrito };
                }

                var distrito = distritos[dado.NomeDistrito];

                // Verifica se o concelho já existe dentro do distrito
                var concelho = distrito.Concelhos.FirstOrDefault(c => c.Nome == dado.NomeConcelho);
                if (concelho == null)
                {
                    concelho = new Concelho { Nome = dado.NomeConcelho };
                    distrito.Concelhos.Add(concelho);
                }

                // Verifica se o concelho já existe dentro do Concelho
                var freguesia = concelho.Freguesias.FirstOrDefault(f => f.Nome == dado.NomeFreguesia);
                if (freguesia == null)
                {
                    freguesia = new Freguesia { Nome = dado.NomeFreguesia };
                    concelho.Freguesias.Add(freguesia);
                }

                // Adiciona a freguesia dentro do concelho correspondente
                freguesia.Lugares.Add(new Lugar { Nome = dado.NomeLugar });
            }

            // Lista final com todos os distritos e suas hierarquias
            var listaDistritos = distritos.Values.ToList();
            return listaDistritos;
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
