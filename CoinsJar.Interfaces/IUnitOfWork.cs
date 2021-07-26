
namespace CoinJar.Interfaces
{
    using System;
    public interface IUnitOfWork : IDisposable
    {
        ICoinRepository Coins { get; }
        ISQLProc SQLProc { get; }
        void Save();
    }
}
