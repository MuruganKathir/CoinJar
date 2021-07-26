using CoinJar.ViewModels.Models;

namespace CoinJar.ViewModels.Requests
{
    public class AddCoinRequest 
    {
        public CJRequest CJRequest { get; set; }
        public CoinModel Coin { get; set; }
    }
}
