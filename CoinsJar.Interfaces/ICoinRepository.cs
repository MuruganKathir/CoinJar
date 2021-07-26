using CoinJar.Entities;
using System.Threading.Tasks;

namespace CoinJar.Interfaces
{
    public interface ICoinRepository:IRepository<Coin>
    {
        Task<decimal> GetTotalAmount();
        Task<int> GetTotalVolume();
        Task Reset();
    }
}
