using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ExcelDataReader; 

namespace ParseExcelToSqlite.Services
{
    public class ProcessarBaseDeDados1Service
    {
        public ProcessarBaseDeDados1Service()
        {
            // Construtor do serviço
        }

        public static void Executar(string databasePath)
        {
            string excelFilePath = @"D:\Users\RicardoAraujo\source\repos\ParseExcelToSqlite\DistritosConcelhosFreguesias_CAOP2013_Populacao_Censos2011.xlsx";
            //string sqliteConnectionString = @"Data Source=DistritosConcelhosFreguesias.db;Version=3;";
            string sqliteConnectionString = @$"Data Source={databasePath};Version=3;";

            // Register the ExcelDataReader encoding provider (required for reading Excel files)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DataTable dt;

            // Read data from the Excel file
            using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    Console.WriteLine($"Total de linhas no ficheiro Excel: {reader.RowCount}");
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false } //Esta folha de excel não tem cabeçalho
                    });

                    // Assuming data is in the first sheet
                    dt = result.Tables[0].Copy();
                }
            }

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(sqliteConnectionString))
            {
                sqliteConnection.Open();

                // Create the table if it doesn't exist
                string createTableQuery = @"CREATE TABLE IF NOT EXISTS DistritosConcelhosFreguesias (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CodDistrito TEXT,
                    NomeDistrito TEXT,
                    CodConcelho TEXT,
                    NomeConcelho TEXT,
                    CodFreguesia TEXT,
                    NomeFreguesia TEXT,
                    Populacao INTEGER,
                    FreguesiaLitoranea BOOLEAN
                )";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, sqliteConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine($"Total de linhas lidas: {dt.Rows.Count}");
                // Insert data into the SQLite database
                foreach (DataRow row in dt.Rows)
                {
                    string insertQuery = @"INSERT INTO DistritosConcelhosFreguesias 
                        (CodDistrito, NomeDistrito, CodConcelho, NomeConcelho, CodFreguesia, NomeFreguesia, Populacao, FreguesiaLitoranea) 
                        VALUES 
                        (@CodDistrito, @NomeDistrito, @CodConcelho, @NomeConcelho, @CodFreguesia, @NomeFreguesia, @Populacao, @FreguesiaLitoranea)";

                    using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, sqliteConnection))
                    {
                        // Using column indexes (0-based)
                        row[0] = row[0] != null ? row[0].ToString().Replace("'", "") : "";
                        //Console.WriteLine($"Valor da célula: {cellValue}");
                        //Console.WriteLine($"Valor da row[0]: {row[0].ToString()}");
                        cmd.Parameters.AddWithValue("@CodDistrito", row[0]);  // Column A
                        cmd.Parameters.AddWithValue("@NomeDistrito", row[1]);  // Column B

                        row[2] = row[2] != null ? row[2].ToString().Replace("'", "") : "";
                        cmd.Parameters.AddWithValue("@CodConcelho", row[2]);   // Column C
                        cmd.Parameters.AddWithValue("@NomeConcelho", row[3]);  // Column D
                        
                        row[4] = row[4] != null ? row[4].ToString().Replace("'", "") : "";
                        cmd.Parameters.AddWithValue("@CodFreguesia", row[4]);  // Column E
                        cmd.Parameters.AddWithValue("@NomeFreguesia", row[5]); // Column F
                        row[6] = row[6] != DBNull.Value ? row[6] : 0;
                        cmd.Parameters.AddWithValue("@Populacao", row[6]);       // Column G
                        

                        var value = row[7];

                        // Handle potential null value and check if it equals "S"
                        if (value != DBNull.Value)
                        {
                            cmd.Parameters.AddWithValue("@FreguesiaLitoranea", false);
                            
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@FreguesiaLitoranea", true);
                        }
                        //cmd.Parameters.AddWithValue("@Rural", row[7]);         // Column H
                        //cmd.Parameters.AddWithValue("@Lituraneo", row[8]);     // Column I
                        //Console.WriteLine($"Distrito: {row[0]}, Concelho: {row[2]}, Freguesia: {row[5]}, População: {row[6]}, Litorânea: {row[7]}");

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}