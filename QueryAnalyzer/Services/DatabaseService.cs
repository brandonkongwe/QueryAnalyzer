using System;
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace QueryAnalyzer.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databaseType;

        public DatabaseService(string databaseType, string connectionString)
        {
            _databaseType = databaseType.ToLower();
            _connectionString = connectionString;
        }

        // query execution method that returns the results, execution time, and any error
        public (List<Dictionary<string, object>> Results, TimeSpan ExecutionTime, string Error) ExecuteQuery(string query)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            TimeSpan executionTime = TimeSpan.Zero;
            string error = null;

            try
            {
                // only using MySQL for now :'(
                IDbConnection connection = _databaseType.ToLower() switch
                {
                    "sqlserver" => new SqlConnection(_connectionString),
                    "mysql" => new MySqlConnection(_connectionString),
                    _ => throw new ArgumentException("Unsupported database type")
                };

                using (connection)
                {
                    connection.Open();
                    // only using MySQL for now :'(
                    IDbCommand command = _databaseType.ToLower() switch
                    {
                        "sqlserver" => new SqlCommand(query, (SqlConnection)connection),
                        "mysql" => new MySqlCommand(query, (MySqlConnection)connection),
                        _ => throw new ArgumentException("Unsupported database type")
                    };

                    // measuring the execution time of the query
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    using (var reader = command.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        results = ToDictionaryList(dataTable);
                    }
                    stopwatch.Stop();
                    executionTime = stopwatch.Elapsed;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return (results, executionTime, error);
        }

        // method for converting a DataTable to a list of dictionaries
        public static List<Dictionary<string, object>> ToDictionaryList(DataTable dataTable)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    dict[column.ColumnName] = row[column];
                }

                result.Add(dict);
            }

            return result;
        }

        // method to get the query plan for a query
        public string GetQueryPlan(string query)
        {
            string plan = string.Empty;

            try
            {
                using (IDbConnection connection = CreateConnection())
                {
                    connection.Open();
                    using (IDbCommand command = CreateCommand($"EXPLAIN {query}", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    plan += $"{reader.GetName(i)}: {reader[i]}; ";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                plan = $"Error generating query plan: {ex.Message}";
            }

            return plan;
        }

        // method for analyzing the query plan and providing optimization suggestions
        public List<string> AnalyzeQueryPlan(string queryPlan)
        {
            var suggestions = new List<string>();

            if (queryPlan.Contains("Using filesort"))
            {
                suggestions.Add("Suggestion: Avoid filesort by adding an appropriate index or optimizing the ORDER BY clause.");
            }

            if (queryPlan.Contains("Using temporary"))
            {
                suggestions.Add("Suggestion: Reduce temporary table usage by optimizing GROUP BY, ORDER BY, or using indexes.");
            }

            if (queryPlan.Contains("full table scan") || queryPlan.Contains("ALL"))
            {
                suggestions.Add("Suggestion: A full table scan was detected. Consider adding indexes to improve performance.");
            }

            if (queryPlan.Contains("rows:"))
            {
                Match match = Regex.Match(queryPlan, @"rows=(\d+)");
                if (match.Success && int.Parse(match.Groups[1].Value) > 100000)
                {
                    suggestions.Add("Suggestion: A large number of rows are being processed. Consider filtering data early in the query.");
                }
            }

            if (queryPlan.Contains("Using join buffer"))
            {
                suggestions.Add("Suggestion: Optimize JOIN conditions by ensuring indexes exist on the joined columns.");
            }

            if (queryPlan.Contains("Using where"))
            {
                suggestions.Add("Good practice: The query is filtering data with WHERE conditions. Ensure proper indexing.");
            }

            if (queryPlan.Contains("index"))
            {
                suggestions.Add("Good practice: The query is using an index. Verify that it's the most efficient one.");
            }
            return suggestions;
        }

        // method for creating a database connection
        private IDbConnection CreateConnection()
        {
            return _databaseType switch
            {
                "sqlserver" => new SqlConnection(_connectionString),
                "mysql" => new MySqlConnection(_connectionString),
                _ => throw new Exception("Invalid database type")
            };
        }

        // method for creating a database command
        private IDbCommand CreateCommand(string query, IDbConnection connection)
        {
            return _databaseType switch
            {
                "sqlserver" => new SqlCommand(query, (SqlConnection)connection),
                "mysql" => new MySqlCommand(query, (MySqlConnection)connection),
                _ => throw new Exception("Invalid database type")
            };
        }
    }
}
