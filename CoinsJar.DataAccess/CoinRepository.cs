using CoinJar.Entities;
using CoinJar.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CoinJar.DataAccess
{
    public class CoinRepository : Repository<Coin>, ICoinRepository
    {
        private readonly CJDbContext _cjDbContext;
        public CoinRepository(CJDbContext dbContext) : base(dbContext)
        {
            _cjDbContext = dbContext;
        }

        public async Task<decimal> GetTotalAmount()
        {
            return await _cjDbContext.Coins.SumAsync(_ => _.Amount);
        }

        public async Task<int> GetTotalVolume()
        {
            return await _cjDbContext.Coins.SumAsync(_ => _.Volume);
        }

        public async Task Reset()
        {
            var coins =  await _cjDbContext.Coins.ToListAsync();
            coins.ForEach(_ => _.Amount = 0);
            coins.ForEach(_ => _.Volume = 0);
            _cjDbContext.SaveChanges();
        }

    }
}
