using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoinJar.Entities
{
    [Table("Coins")]
    public class Coin
    {
        [Key]
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int Volume { get; set; }
    }
}
