using System;
using System.Collections.Generic;
using System.Text;

namespace CoinJar.ViewModels.Responses
{
    public class TotalCountResponse
    {
        public CJResponse Response { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
