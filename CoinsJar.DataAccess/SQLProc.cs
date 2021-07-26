namespace CoinJar.DataAccess
{
    using Dapper;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using CoinJar.Interfaces;

    public class SQLProc : ISQLProc
    {
        private readonly CJDbContext _dbContext;
        private static string connectionString = "";
        public SQLProc(CJDbContext dbContext)
        {
            _dbContext = dbContext;
            //passing connection string from DbContext object.
            connectionString = _dbContext.Database.GetDbConnection().ConnectionString;
        }
        /// <summary>
        /// executing stored procedure and returning the list of values in generic type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName">this is procedure name</param>
        /// <param name="parameters">passing parameters for executing stored procedure</param>
        /// <returns></returns>
        public IEnumerable<T> GetList<T>(string procName, DynamicParameters parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.Query<T>(procName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        /// <summary>
        /// executing stored procedure without returning, that can be either insert or update or delete
        /// </summary>
        /// <param name="procName">this is procedure name</param>
        /// <param name="parameters">passing parameters for executing stored procedure</param>
        public void ExecuteSP(string procName, DynamicParameters parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.Execute(procName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public T ExecuteScaler<T>(string procName, DynamicParameters parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return (T)Convert.ChangeType(conn.ExecuteScalar<T>(procName, parameters, commandType: System.Data.CommandType.StoredProcedure), typeof(T));
            }
        }

        public IEnumerable<T> GetListBySQL<T>(string SQLStatement)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.Query<T>(SQLStatement, commandType: System.Data.CommandType.Text);
            }
        }

        public T GetEntityBySQL<T>(string SQLStatement)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.Query<T>(SQLStatement, commandType: System.Data.CommandType.Text).FirstOrDefault();
            }
        }
        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
