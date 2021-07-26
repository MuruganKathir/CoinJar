namespace CoinJar.DataAccess
{
    using CoinJar.Interfaces;
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CJDbContext cjDb;

        public UnitOfWork(CJDbContext dbContext)
        {
            cjDb = dbContext;
            Coins = new CoinRepository(cjDb);
            SQLProc = new SQLProc(cjDb);
        }

        public ICoinRepository Coins { get; private set; }
        public ISQLProc SQLProc { get; private set; }

        public int Complete()
        {
            return cjDb.SaveChanges();
        }
        public void Dispose()
        {
            cjDb.Dispose();
        }

        public void Save()
        {
            cjDb.SaveChanges();
        }
    }
}
