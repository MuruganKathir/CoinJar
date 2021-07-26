using Dapper;
using System;
using System.Collections.Generic;

namespace CoinJar.Interfaces
{
    public interface ISQLProc : IDisposable
    {
        IEnumerable<T> GetList<T>(string procName, DynamicParameters parameters = null);
        void ExecuteSP(string procName, DynamicParameters parameters = null);
        T ExecuteScaler<T>(string procName, DynamicParameters parameters = null);

        IEnumerable<T> GetListBySQL<T>(string SQLStatement);
        T GetEntityBySQL<T>(string SQLStatement);
    }
}
