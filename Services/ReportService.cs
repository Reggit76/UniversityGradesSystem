// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using ClosedXML.Excel;
using Npgsql;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace UniversityGradesSystem.Services
{
    public class ReportService
    {
        string _connectionString;
        public ReportService(string connectionString) { this._connectionString = connectionString; }
        public void ExportToExcel(string query, string sheetName, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var worksheet = workbook.Worksheets.Add(sheetName);
                            // Заголовки
                            for (int i = 0; i < reader.FieldCount; i++)
                                worksheet.Cell(1, i + 1).Value = reader.GetName(i);

                            int row = 2;
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                    worksheet.Cell(row, i + 1).Value = reader[i].ToString();
                                row++;
                            }
                        }
                    }
                }
                workbook.SaveAs(filePath);
                MessageBox.Show("Данные успешно экспортированы в Excel!");
            }
        }
    }
}