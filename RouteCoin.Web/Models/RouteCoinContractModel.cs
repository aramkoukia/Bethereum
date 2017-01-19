using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RouteCoin.Web.Models
{
    public class RouteCoinContractModel
    {
        public string BuyerPublicKey { get; set; }
        public string BuyerAccountPassword { get; set; }
        public string DestinationAddress { get; set; }
        public int ContractPrice { get; set; }
        public int ContractGracePeriod { get; set; }

    }
}