using System.Data;
using System.Data.SqlClient;

namespace RaceDay_Part2.Data
{
    public class DatabaseHelper
    {
        // Your LocalDB instance and database
        private const string ConnectionString =
            @"Server=(localdb)\api_both;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;";

        public SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        public SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
        {
            var conn = GetConnection();
            conn.Open();
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }
    }
}