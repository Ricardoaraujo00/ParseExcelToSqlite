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
using ParseExcelToSqlite.Models;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Reflection;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions; // Add this line to include the DatabaseSeeder class

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
            public string Codigo {get;set;} ="";
            public string Nome { get; set; }
            public List<Lugar> Lugares { get; set; } = new List<Lugar>();
        }

        public class Concelho
        {
            public string Codigo {get;set;} ="";
            public string Nome { get; set; }
            public List<Freguesia> Freguesias { get; set; } = new List<Freguesia>();
        }

        public class Distrito
        {
            public string Codigo {get;set;} ="";
            public string Nome { get; set; }
            public List<Concelho> Concelhos { get; set; } = new List<Concelho>();
        }
        static void Main(string[] args)
        {

            // Navega até a raiz do projeto a partir do diretório bin
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

            // Define o caminho completo para o banco de dados na raiz do projeto
            var databasePath = Path.Combine(projectRoot, "DistritosConcelhosFreguesias.db");

            // Configura as opções do DbContext
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseDbContext>();
            optionsBuilder.UseSqlite(
                connectionString: $"Filename={databasePath}",
                sqliteOptionsAction: op =>
                {
                    op.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                });


            using (var dbContext = new DatabaseDbContext(optionsBuilder.Options))
            {
                //dbContext.Database.EnsureCreated();
                //CriarTabelaNivelEFazerSeed();
                //ProcessarBaseDeDados1Service.Executar(databasePath);
                //ProcessarBaseDeDados2Service.Executar(databasePath);
                //var distritos = ObterEstruturaDadosTabela1(true);
                //CriarTabelaFinal();
                //ProcessarDadosDasDuasTabelas(dbContext);
                //CocatenarCodigosIlhas(dbContext);
            }

            Console.WriteLine("Data transferred successfully!");
        
        }

        public static async void CocatenarCodigosIlhas(DatabaseDbContext dbContext)
        {
            var locaisAConcatenar = dbContext.Locais.Where(x=>x.Id>= 36499).ToList();
            foreach (var local in locaisAConcatenar)
            {
                var localPai = dbContext.Locais.FirstOrDefault(x => x.Id == local.DependeDeId);
                if (localPai != null)
                {
                    local.Codigo = localPai.Codigo + local.Codigo;
                    dbContext.Update(local);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        static string RemoverConteudoEntreParenteses(string input)
        {
            // Regex para encontrar tudo entre parênteses, incluindo os parênteses
            string pattern = @"\s*\([^()]*\)";
            return Regex.Replace(input, pattern, string.Empty);
        }

        static string ExtrairConteudoEntreParenteses(string input)
        {
            // Regex para encontrar o conteúdo entre parênteses
            string pattern = @"\(([^()]*)\)";
            Match match = Regex.Match(input, pattern);

            // Retorna o conteúdo capturado (o que está entre parênteses)
            if (match.Success)
            {
                return match.Groups[1].Value; // Groups[1] contém o conteúdo entre parênteses
            }

            return string.Empty; // Retorna uma string vazia se não houver parênteses
        }


        public static async void ProcessarDadosDasDuasTabelas(DatabaseDbContext dbContext)
        {
            
            
            var niveis = dbContext.Niveis.ToList();

            var listaDCF = ObterEstruturaDadosTabela1(dbContext);
            //Imprimir(listaDCF);
            var listaDCFcomLugares = ObterEstruturaDadosTabela2(dbContext);
            //Imprimir(listaDCFcomLugares);
            
            Local locais = new Local();
            Console.WriteLine("Início ProcessarDadosDasDuasTabelas");
            Console.ReadKey();

            foreach (var distrito in listaDCFcomLugares)
            {


                //Console.WriteLine("Prima uma tecla para continuar");
                //Console.ReadKey();
                //Ir buscar o codNivel de distrito

                //Declarado aqui porque se o distrito fôr uma ilha o código vai ter que ser obtido de outra forma

                var distritoDependeDeID = 0;
                
                var distritoDCF = listaDCF.FirstOrDefault(x=>x.Nome==distrito.Nome);
                //Vou inicializar aqui o localRegiao porque depois, se não fôr uma ilha tenho que ligar o distrito ao Portugal Continental
                Local localRegiao = new();
                bool distritoIlha = false;
                //Se não exite é porque será uma ilha
                if (distritoDCF.Nome.Contains("Ilha"))
                {
                    distritoIlha = true;
                    var nivelRegiao = niveis.FirstOrDefault(x => x.Nome == "Região").Id;
                    var nomeDaRegiao = ExtrairConteudoEntreParenteses(distritoDCF.Nome);
                    localRegiao = dbContext.Locais.Where(x=>x.CodNivel==nivelRegiao).FirstOrDefault(x => x.Nome.Contains(nomeDaRegiao));
                        
                    //O distrito a ser adicionado a seguir vai depender desta região
                    distritoDependeDeID = localRegiao.Id;                    
                }
                else
                {
                    //Se não fôr uma ilha o LocalDistro vai depender de Portugal Continental
                    distritoDependeDeID = dbContext.Locais.FirstOrDefault(x => x.CodNivel == 1).Id;
                }


                //Criar o objecto distrito e adicionar à base de dados e gravar e obter o ID
                Local localDistrito = new Local()
                {
                    Codigo = distritoDCF.Codigo,
                    Nome = distrito.Nome,
                    CodNivel = niveis.FirstOrDefault(x=> x.Nome=="Distrito").Id,
                    DependeDeId= distritoDependeDeID
                };
                dbContext.Add(localDistrito);
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"Distrito[cod - nome]: {localDistrito.Id} - {localDistrito.Nome}");

                //Depois fazer igual para os restantes
                foreach (var concelho in distrito.Concelhos)
                {
                    string codigo = "";                    
                    var concelhoDCF = distritoDCF.Concelhos.FirstOrDefault(y => y.Nome.ToUpper() == concelho.Nome.ToUpper());
                    if(concelhoDCF==null) distritoDCF.Concelhos.FirstOrDefault(y => concelho.Nome.ToUpper().Contains(y.Nome.ToUpper()));
                    Local localConcelho = new Local()
                    {
                        Codigo = concelhoDCF.Codigo,
                        Nome = concelho.Nome,
                        CodNivel = niveis.FirstOrDefault(x => x.Nome == "Concelho").Id,
                        DependeDeId = localDistrito.Id // O concelho depende do distrito que foi criado anteriormente
                    };
                    dbContext.Add(localConcelho);
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"  Concelho[id - cod - nome - dependeDeId]: {localConcelho.Id} - {localConcelho.Codigo} - {localConcelho.Nome} - {localConcelho.DependeDeId}");

                    foreach (var freguesia in concelho.Freguesias)
                    {
                        //Ir buscar à tabela 1 as freguesias do corrente concelho que são uniões de freguesia
                        List<Freguesia> freguesiasDCF = concelhoDCF
                                                        .Freguesias
                                                        .Where(f => f.Nome.ToUpper().Contains("União") || f.Nome.ToUpper().Contains(" e "))
                                                        .ToList();
                        Freguesia freguesiaEmUniao = null;
                        if(freguesiasDCF!=null) freguesiaEmUniao = freguesiasDCF.FirstOrDefault(f => f.Nome.ToUpper().Contains(freguesia.Nome.ToUpper()));
                        if (freguesiaEmUniao==null && freguesia.Nome.Contains("("))
                        {
                            // Extrai o conteúdo fora e dentro dos parênteses
                            string conteudoForaParenteses;
                            string conteudoDentroParenteses;
                            // Encontra a posição do parêntese de abertura e fechamento
                            int inicioParenteses = freguesia.Nome.IndexOf('(');
                            int fimParenteses = freguesia.Nome.IndexOf(')');

                            // Extrai o conteúdo fora dos parênteses
                            conteudoForaParenteses = freguesia.Nome.Substring(0, inicioParenteses).Trim();

                            // Extrai o conteúdo dentro dos parênteses
                            conteudoDentroParenteses = freguesia.Nome.Substring(inicioParenteses + 1, fimParenteses - inicioParenteses - 1).Trim();

                            freguesiaEmUniao = freguesiasDCF.FirstOrDefault(f => f.Nome.Contains(conteudoDentroParenteses) || f.Nome.Contains(conteudoForaParenteses));

                        }

                      
                        //Variáveis que vão servir para adicionar a freguesia verdadeira dependendo se a união existe ou não
                        //O ID de hierárquia depende se esta está numa união ou não
                        //Assim como o código de freguesia. Se estiver em união não terá código de freguesia
                        var FreguesiaDependedeID = 0;
                        var codigoDaFreguesia = "";
                        var NivelDaFreguesia = 0;
                        //Adicionar união de freguesias se existir o nome da freguua nessa união
                        if (freguesiaEmUniao!=null)
                        {
                            Console.WriteLine($"Freguesia pertence a união de freguesias: {freguesiaEmUniao.Nome}");
                            var uniaoDeFreguesias = dbContext.Locais.FirstOrDefault(x => x.Nome == freguesiaEmUniao.Nome);
                            if(uniaoDeFreguesias==null)//Se não existir a união de freguesias registada na base de dados vai ser feito o registo
                            {
                                uniaoDeFreguesias = new Local()
                                {
                                    Codigo = freguesiaEmUniao.Codigo,
                                    Nome = freguesiaEmUniao.Nome, // Preencher com o nome da freguesia encontrada ou o nome original
                                    CodNivel = niveis.FirstOrDefault(x => x.Nome == "União de Freguesias").Id,
                                    DependeDeId = localConcelho.Id // A freguesia depende do concelho que foi criado anteriormente
                                };
                                dbContext.Add(uniaoDeFreguesias);
                                await dbContext.SaveChangesAsync();
                                Console.WriteLine($"União de Freguesia[id - cod - nome - dependeDeId]: {uniaoDeFreguesias.Id} - {uniaoDeFreguesias.Codigo} - {uniaoDeFreguesias.Nome} - {uniaoDeFreguesias.DependeDeId}");
                            }

                            //A freguesia verdadeira depende da união de freguesias
                            FreguesiaDependedeID = uniaoDeFreguesias.Id;
                            //E se está numa união é porque foi extinta
                            NivelDaFreguesia = niveis.FirstOrDefault(x => x.Nome == "Freguesia extinta").Id;

                        }
                        else
                        {
                            //A frequesia verdadeira depende do concelho
                            FreguesiaDependedeID = localConcelho.Id;
                            //E nesse caso tem um código de freguesia
                            var tempFreguesia = concelhoDCF
                                                .Freguesias.FirstOrDefault(z => z.Nome == freguesia.Nome);
                            if(tempFreguesia!=null)
                            {
                                codigoDaFreguesia = tempFreguesia.Codigo;
                            }
                            //E está no nível de freguesia
                            NivelDaFreguesia = niveis.FirstOrDefault(x => x.Nome == "Freguesia").Id;
                        }

                        //Adicionar a freguesia verdadeira sem união
                        Local localFreguesia = new Local()
                        {
                            Codigo = codigoDaFreguesia,
                            Nome = freguesia.Nome, // Preencher com o nome da freguesia encontrada ou o nome original
                            CodNivel = NivelDaFreguesia,
                            DependeDeId = FreguesiaDependedeID // A freguesia depende do concelho que foi criado anteriormente
                        };
                        dbContext.Add(localFreguesia);
                        await dbContext.SaveChangesAsync();
                        //Console.WriteLine($"Freguesia[id - cod - nome - dependeDeId]: {localFreguesia.Id} - {localFreguesia.Codigo} - {localFreguesia.Nome} - {localFreguesia.DependeDeId}");

                        
                        foreach (var lugar in freguesia.Lugares)
                        {
                            Local localLugarDeFreguesia = new()
                            {
                                Codigo = "",
                                Nome = lugar.Nome,
                                CodNivel = niveis.FirstOrDefault(x => x.Nome == "Lugar").Id,
                                DependeDeId = localFreguesia.Id
                            };
                            dbContext.Add(localLugarDeFreguesia);
                            await dbContext.SaveChangesAsync();
                            //Console.WriteLine($"Lugar [id - cod - nome - dependeDeId]: {localLugarDeFreguesia.Id} - {localLugarDeFreguesia.Codigo} - {localLugarDeFreguesia.Nome} - {localLugarDeFreguesia.DependeDeId}");
                            
                        }
                        
                    }
                }
            }
        }

        public static List<Distrito> ObterEstruturaDadosTabela1(DatabaseDbContext dbContext, bool imprimir = false)
        {
            Console.WriteLine("ObterEstruturaDadosTabela1");
            //CriarTabelaFinal();
            
            // Obter os dados da tabela original
            var dados = dbContext.DistritosConcelhosFreguesias.Where(x=>x.NomeDistrito.Contains("Ilha")).ToList();

            // Criar um dicionário para evitar duplicações e facilitar a busca
            var distritos = new List<Distrito>();

            foreach (var dado in dados)
            {
                Distrito distrito = distritos.FirstOrDefault(x=>x.Nome==dado.NomeDistrito);
                // Verifica se o distrito já foi adicionado
                if (distrito==null)
                {
                    distrito = new Distrito { Codigo=dado.CodDistrito, Nome = dado.NomeDistrito , Concelhos=new()};
                    distritos.Add(distrito);
                }

                // Verifica se o concelho já existe dentro do distrito
                var concelho = distrito.Concelhos.FirstOrDefault(c => c.Nome == dado.NomeConcelho);
                if (concelho == null)
                {
                    concelho = new Concelho {Codigo=dado.CodConcelho, Nome = dado.NomeConcelho , Freguesias=new()};
                    distrito.Concelhos.Add(concelho);
                }

                // Adiciona a freguesia dentro do concelho correspondente
                concelho.Freguesias.Add(new Freguesia {Codigo=dado.CodFreguesia, Nome = dado.NomeFreguesia, Lugares=new() });
            }

            //################################################################################################################################
            if(imprimir)
            {
                ConsoleKeyInfo key = new ConsoleKeyInfo('B', ConsoleKey.B, false, false, false);
                foreach (var distrito in distritos)
                {
                    if (key.Key != ConsoleKey.A)
                    {
                        Console.WriteLine($"Distrito[cod- nome]: {distrito.Codigo}-{distrito.Nome}");

                        foreach (var concelho in distrito.Concelhos)
                        {
                            Console.WriteLine($"  Concelho[cod-nome]: {concelho.Codigo}-{concelho.Nome}");
                            foreach (var freguesia in concelho.Freguesias)
                            {
                                Console.WriteLine($"    Freguesia[cod-nome]: {freguesia.Codigo}-{freguesia.Nome}");
                                if (freguesia.Lugares.Count > 0)
                                {
                                    foreach (var lugar in freguesia.Lugares)
                                    {
                                        Console.WriteLine($"      Lugar: {lugar.Nome}");
                                    }
                                }
                            }
                        }

                        Console.WriteLine("Prima uma tecla para continuar. Para abortar a impressão prima a tecla 'A'.");
                        Console.WriteLine("########################################################################################################################");
                        key = Console.ReadKey();
                        if (key.Key == ConsoleKey.A)
                        {
                            Console.WriteLine("Impressão abortada pelo utilizador.");
                        }

                    }
                    
                }
            }
            //################################################################################################################################


            // Lista final com todos os distritos e suas hierarquias
            
            return distritos;
        }

        public static List<Distrito> ObterEstruturaDadosTabela2(DatabaseDbContext dbContext, bool imprimir=false)
        {
            Console.WriteLine("ObterEstruturaDadosTabela2");
            //CriarTabelaFinal();
            
            // Obter os dados da tabela original
            var dados = dbContext.Lugares.Where(x=>x.Id> 28387).ToList();

            // Criar um dicionário para evitar duplicações e facilitar a busca
            var distritos = new List<Distrito>();

            foreach (var dado in dados)
            {

                Distrito distrito = distritos.FirstOrDefault(x=>x.Nome==dado.NomeDistrito);
                // Verifica se o distrito já foi adicionado
                if (distrito==null)
                {
                    distrito = new Distrito { Nome = dado.NomeDistrito , Concelhos=new()};
                    distritos.Add(distrito);
                }                

                // Verifica se o concelho já existe dentro do distrito
                var concelho = distrito.Concelhos.FirstOrDefault(c => c.Nome == dado.NomeConcelho);
                if (concelho == null)
                {
                    concelho = new Concelho { Nome = dado.NomeConcelho, Freguesias=new() } ;
                    distrito.Concelhos.Add(concelho);
                    
                }
 

                // Verifica se a freguesia já existe dentro do Concelho
                var freguesia = concelho.Freguesias.FirstOrDefault(f => f.Nome == dado.NomeFreguesia);
                if (freguesia == null)
                {
                    freguesia = new Freguesia { Nome = dado.NomeFreguesia, Lugares=new() };
                    concelho.Freguesias.Add(freguesia);
                }

                // Adiciona a freguesia dentro do concelho correspondente
                freguesia.Lugares.Add(new Lugar { Nome = dado.NomeLugar });
            }

            //################################################################################################################################
            if(imprimir)
            {
                ConsoleKeyInfo key = new ConsoleKeyInfo('B', ConsoleKey.B, false, false, false);
                foreach (var distrito in distritos)
                {
                    if (key.Key != ConsoleKey.A)
                    {
                        Console.WriteLine($"Distrito: {distrito.Nome}");

                        foreach (var concelho in distrito.Concelhos)
                        {
                            Console.WriteLine($"  Concelho: {concelho.Nome}");
                            foreach (var freguesia in concelho.Freguesias)
                            {
                                Console.WriteLine($"    Freguesia: {freguesia.Nome}");
                                if (freguesia.Lugares.Count > 0)
                                {
                                    foreach (var lugar in freguesia.Lugares)
                                    {
                                        Console.WriteLine($"      Lugar: {lugar.Nome}");
                                    }
                                }
                            }
                        }

                        Console.WriteLine("Prima uma tecla para continuar. Para abortar a impressão prima a tecla 'A'.");
                        Console.WriteLine("########################################################################################################################");
                        key = Console.ReadKey();
                        if (key.Key == ConsoleKey.A)
                        {
                            Console.WriteLine("Impressão abortada pelo utilizador.");
                        }
                    }
                }
            }
            //################################################################################################################################

            // Lista final com todos os distritos e suas hierarquias
            return distritos;
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
                    DependeDeLocal  INTEGER
                )";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, sqliteConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void CriarTabelaNivelEFazerSeed(DatabaseDbContext dbContext)
        {
            using (var connection = dbContext.Database.GetDbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // Verifica se a tabela Nivel existe
                    command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Niveis';";
                    var result = command.ExecuteScalar() as long?;
                    var exists = result.HasValue && result.Value > 0;

                    if (!exists)
                    {
                        // Cria a tabela se não existir
                        command.CommandText = @"
                            CREATE TABLE Niveis (
                                Id INTEGER PRIMARY KEY, 
                                Nome TEXT NOT NULL
                            );";
                        command.ExecuteNonQuery();

                        
                    }
                    // Faz o seed dos dados iniciais
                    command.CommandText = @"
                        INSERT INTO Niveis (Id, Nome) VALUES
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
