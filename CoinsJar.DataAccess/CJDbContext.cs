
namespace CoinJar.DataAccess
{
    using CoinJar.Entities;
    using Microsoft.EntityFrameworkCore;
    public class CJDbContext : DbContext
    {
        public CJDbContext(DbContextOptions<CJDbContext> options)
           : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        public DbSet<Coin> Coins{ get; set; }
    }
}
