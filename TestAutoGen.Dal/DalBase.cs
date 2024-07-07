using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestAutoGen.Dal
{
   
    public class DalBase
    {
        private readonly IDbConnection _dbConnection;

        public DalBase(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public IEnumerable<T> Query<T>(string sql, object? param = null)
        {
            using (var conn = _dbConnection)
            { 
                conn.Open();
                var result = conn.Query<T>(sql, param).ToList();
                conn.Close();
                return result;  
            }
        }

        public T? FirstOrDefault<T>(string sql, object? param = null)
        {
            using (var conn = _dbConnection)
            {
                conn.Open();
                var result = conn.QueryFirstOrDefault<T>(sql, param);
                conn.Close();
                return result ?? default;    
            }
        }
        public int Execute(string sql, object? parameters = null)
        {
            using (var conn = _dbConnection)
            {
                conn.Open();
                return conn.Execute(sql, parameters);
            }
        }

        public T? ExecuteScalar<T>(string sql, object? parameters = null)
        {
            using (var conn = _dbConnection)
            {
                conn.Open();
                var result = conn.ExecuteScalar<T>(sql, parameters);
                conn.Close();

                return result ?? default;
            }
        }

        public string GenerateUpdateSql<T>(T obj)
        {
            var type = typeof(T);
            var tableName = type.Name; 
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.Name != "Id");

            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
            var whereClause = "Id = @Id";

            return $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";
        }
    }
}
