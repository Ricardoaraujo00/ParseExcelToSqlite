using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using ExcelDataReader;

namespace ParseExcelToSqlite
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string excelFilePath = @"D:\Users\RicardoAraujo\Downloads\DistritosConcelhosFreguesias_CAOP2013_Populacao_Censos2011.xls";
            string sqliteConnectionString = @"Data Source=DistritosConcelhosFreguesias.db;Version=3;";

            // Register the ExcelDataReader encoding provider (required for reading Excel files)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DataTable dt;

            // Read data from the Excel file
            using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });

                    // Assuming data is in the first sheet
                    dt = result.Tables[0];
                }
            }

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(sqliteConnectionString))
            {
                sqliteConnection.Open();

                // Create the table if it doesn't exist
                string createTableQuery = @"CREATE TABLE IF NOT EXISTS DistritosConcelhosFreguesias (
                    CodDistrito TEXT,
                    NomeDistrito TEXT,
                    CodConcelho TEXT,
                    NomeConcelho TEXT,
                    CodFreguesia TEXT,
                    NomeFreguesia TEXT,
                    Pop2011 INTEGER,
                    Rural TEXT,
                    Lituraneo TEXT
                )";

                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, sqliteConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Insert data into the SQLite database
                foreach (DataRow row in dt.Rows)
                {
                    string insertQuery = @"INSERT INTO DistritosConcelhosFreguesias 
                        (CodDistrito, NomeDistrito, CodConcelho, NomeConcelho, CodFreguesia, NomeFreguesia, Pop2011, Rural, Lituraneo) 
                        VALUES 
                        (@CodDistrito, @NomeDistrito, @CodConcelho, @NomeConcelho, @CodFreguesia, @NomeFreguesia, @Pop2011, @Rural, @Lituraneo)";

                    using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, sqliteConnection))
                    {
                        // Using column indexes (0-based)
                        cmd.Parameters.AddWithValue("@CodDistrito", row[0]);  // Column A
                        cmd.Parameters.AddWithValue("@NomeDistrito", row[1]);  // Column B
                        cmd.Parameters.AddWithValue("@CodConcelho", row[2]);   // Column C
                        cmd.Parameters.AddWithValue("@NomeConcelho", row[3]);  // Column D
                        cmd.Parameters.AddWithValue("@CodFreguesia", row[4]);  // Column E
                        cmd.Parameters.AddWithValue("@NomeFreguesia", row[5]); // Column F
                        cmd.Parameters.AddWithValue("@Pop2011", row[6]);       // Column G
                        cmd.Parameters.AddWithValue("@Rural", row[7]);         // Column H
                        cmd.Parameters.AddWithValue("@Lituraneo", row[8]);     // Column I

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            Console.WriteLine("Data transferred successfully!");

        }
    }
}
