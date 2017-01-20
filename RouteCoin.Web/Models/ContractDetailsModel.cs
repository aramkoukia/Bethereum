using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RouteCoin.Web.Models
{
    public class ContractDetailsModel
    {
        public string TransactionHash { get; set; }
        public string FromAddress { get; set; }
        public string ContractAddress { get; set; }
        public string ContractBalance { get; set; }
        public string State { get; set; }
        public string DestinationAddress { get; set; }
        public string ContractPrice { get; set; }

    }
}