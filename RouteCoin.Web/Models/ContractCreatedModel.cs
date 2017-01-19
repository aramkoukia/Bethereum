using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RouteCoin.Web.Models
{
    public class ContractCreatedModel
    {
        public string TransactionHash { get; set; }
        public string ContractAddress { get; set; }

    }
}