using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RouteCoin.Web.Models
{
    public class DestinationAddressRouteConfirmedViewModel
    {
        public string CallerAddress { get; set; }
        public string CallerPassword { get; set; }
        public string ContractState { get; set; }
        public string ContractAddress { get; set; }
        public string TransactionHash { get; set; }
        public string Message { get; set; }
    }
}