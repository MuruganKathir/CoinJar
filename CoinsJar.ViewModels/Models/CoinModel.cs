using System.ComponentModel.DataAnnotations;

namespace CoinJar.ViewModels.Models
{
    public class CoinModel
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int Volume { get; set; }
    }
}
