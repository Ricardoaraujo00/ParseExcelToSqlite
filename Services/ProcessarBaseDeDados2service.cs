using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ExcelDataReader; 

namespace ParseExcelToSqlite.Services
{
    public class ProcessarBaseDeDados2Service
    {
        public ProcessarBaseDeDados2Service()
        {
            // Construtor do serviço
        }

        public static void Executar(string databasePath)
        {
            string excelFilePath = @"D:\Users\RicardoAraujo\source\repos\ParseExcelToSqlite\Lista de Lugares, Freguesias e Oragos.xlsx";
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
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });

                    // Assuming data is in the first sheet
                    dt = result.Tables[0].Copy();
                }
            }

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

                Console.WriteLine($"Total de linhas lidas: {dt.Rows.Count}");
                // Insert data into the SQLite database
                foreach (DataRow row in dt.Rows)
                {
                    string insertQuery = @"INSERT INTO Lugares 
                        (NomeDistrito, NomeConcelho, NomeFreguesia, NomeLugar) 
                        VALUES 
                        (@NomeDistrito, @NomeConcelho, @NomeFreguesia, @NomeLugar)";

                    using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, sqliteConnection))
                    {

                        cmd.Parameters.AddWithValue("@NomeDistrito", row[0]);  // Column B

                        cmd.Parameters.AddWithValue("@NomeConcelho", row[1]);  // Column D

                        cmd.Parameters.AddWithValue("@NomeFreguesia", row[2]); // Column F
                        cmd.Parameters.AddWithValue("@NomeLugar", row[3]); // Column F


                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}