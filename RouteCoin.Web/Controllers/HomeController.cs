using Nethereum.Web3;
using RouteCoin.Web.Models;
using RouteCoin.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RouteCoin.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateContract(RouteCoinContractModel model)
        {
            // Smart Contract API (json interface) and byte code
            var abi = @"[{""constant"":false,""inputs"":[],""name"":""destinationAddressRouteFound"",""outputs"":[],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""abort"",""outputs"":[],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""destinationAddressRouteConfirmed"",""outputs"":[],""payable"":true,""type"":""function""},{""inputs"":[{""name"":""_finalDestination"",""type"":""address""},{""name"":""_contractGracePeriod"",""type"":""uint256""},{""name"":""_contractPrice"",""type"":""uint256""}],""type"":""constructor""},{""anonymous"":false,""inputs"":[],""name"":""aborted"",""type"":""event""},{""anonymous"":false,""inputs"":[],""name"":""routeFound"",""type"":""event""},{""anonymous"":false,""inputs"":[],""name"":""routeAccepted"",""type"":""event""}]";
            var byteCode = "0x606060405260405160608061023f83395060c06040525160805160a05160008054600160a060020a03199081166c01000000000000000000000000338102819004919091178355426003556002805490921695810204949094179093556004919091556005556101cb90819061007490396000f3606060405260e060020a6000350463047854d9811461003457806335a063b41461004f5780636c88c36c14610072575b610002565b34610002576100906004546003540142101561009257610002565b346100025761009060005433600160a060020a0390811691161461010257610002565b61009060005433600160a060020a0390811691161461014e57610002565b005b60065460009060ff16156100a557610002565b6001805473ffffffffffffffffffffffffffffffffffffffff19166c01000000000000000000000000338102041790556040517f78d20fa24b6a0a3596e34219ca2fd4ce740f5d3cce342d6b1d76bd879491bf7290600090a15b50565b60065460009060ff161561011557610002565b6040517f80b62b7017bb13cf105e22749ee2a06a417ffba8c7f57b665057e0f3c2e925d990600090a16006805460ff1916600317905550565b60065460009060ff161561016157610002565b6040517f17e8425f2f0f52156cb58fae3262b87ffe900164617ac332659a4b0e2d8434f590600090a16040516006805460ff1916600217905560008054600554600160a060020a039091169281156108fc0292818181858888f1935050505015156100ff5761000256";

            var senderAddress = model.BuyerPublicKey; 
            var password = model.BuyerAccountPassword; 
            var finalDestination = model.DestinationAddress; 
            var contractGracePeriod = model.ContractGracePeriod; 
            var contractPrice = model.ContractPrice; 

            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient("./geth.ipc");
            var web3 = new Web3(ipcClient);

            var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(senderAddress, password, new Nethereum.Hex.HexTypes.HexBigInteger(120));

            var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, senderAddress, new Nethereum.Hex.HexTypes.HexBigInteger(300000), finalDestination, contractGracePeriod, contractPrice);

            return RedirectToAction("ContractCreated", new { transactionHash = transactionHash } );
        }

        public async Task<ActionResult> ContractCreated(string transactionHash, string contractAddress)
        {
            if(string.IsNullOrEmpty(contractAddress))
            { 
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient("./geth.ipc");
            var web3 = new Web3(ipcClient);
            var reciept = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                if (reciept != null)
                    contractAddress = reciept.ContractAddress;
            }

            return View(new ContractCreatedModel() { TransactionHash = transactionHash, ContractAddress = contractAddress });
        }

    }
}