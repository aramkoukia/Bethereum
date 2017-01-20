using Nethereum.ABI.FunctionEncoding;
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
        // Smart Contract API (json interface) and byte code
        private static string _abi = @"[{""constant"":false,""inputs"":[],""name"":""destinationAddressRouteFound"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""getState"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""abort"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""destinationAddressRouteConfirmed"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""inputs"":[{""name"":""_finalDestination"",""type"":""address""},{""name"":""_contractGracePeriod"",""type"":""uint256""},{""name"":""_contractPrice"",""type"":""uint256""}],""type"":""constructor""},{""anonymous"":false,""inputs"":[],""name"":""aborted"",""type"":""event""},{""anonymous"":false,""inputs"":[],""name"":""routeFound"",""type"":""event""},{""anonymous"":false,""inputs"":[],""name"":""routeAccepted"",""type"":""event""}]";
        private static string _byteCode = "0x606060405260405160608061022083395060c06040525160805160a05160008054600160a060020a03199081166c01000000000000000000000000338102819004919091178355426003556002805490921695810204949094179093556004919091556005556101ac90819061007490396000f3606060405260e060020a6000350463047854d9811461003f5780631865c57d1461005c57806335a063b41461007a5780636c88c36c14610097575b610002565b3461000257610068600654600090819060ff16156100b657610002565b346100025760065460ff165b60408051918252519081900360200190f35b3461000257610068600654600090819060ff161561012857610002565b346100025761006860065460009060049060ff16811461016a57610002565b6001805473ffffffffffffffffffffffffffffffffffffffff19166c01000000000000000000000000338102041790556040517f78d20fa24b6a0a3596e34219ca2fd4ce740f5d3cce342d6b1d76bd879491bf7290600090a16006805460ff19166004179081905560ff1691505b5090565b6040517f80b62b7017bb13cf105e22749ee2a06a417ffba8c7f57b665057e0f3c2e925d990600090a16006805460ff19166003179081905560ff169150610124565b6040517f17e8425f2f0f52156cb58fae3262b87ffe900164617ac332659a4b0e2d8434f590600090a16006805460ff19166002179081905560ff16915061012456";
        private static string _getAddress = "./geth.ipc";
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateContract(RouteCoinContractModel model)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);

            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(300000);
            var balance = new Nethereum.Hex.HexTypes.HexBigInteger(120);
            var accountUnlockTime = new Nethereum.Hex.HexTypes.HexBigInteger(120);

            // Unlock the caller's account with the given password
            var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(model.BuyerPublicKey, model.BuyerAccountPassword, accountUnlockTime);

            var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(_abi, _byteCode, model.BuyerPublicKey, gas, balance, model.DestinationAddress, model.ContractGracePeriod, model.ContractPrice);

            return RedirectToAction("ContractCreated", new { transactionHash = transactionHash });
        }

        public async Task<ActionResult> ContractCreated(string fromAddress, string transactionHash, string contractAddress)
        {
            if (string.IsNullOrEmpty(contractAddress))
            {
                var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
                var web3 = new Web3(ipcClient);
                var reciept = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                if (reciept != null)
                    contractAddress = reciept.ContractAddress;
            }

            return View(new ContractCreatedModel() { TransactionHash = transactionHash, ContractAddress = contractAddress, FromAddress = fromAddress });
        }

        public async Task<ActionResult> WhisperContractCreationToNeigboors(string contractAddress)
        {
            var neigboors = new List<string> {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
            };

            foreach (var neigboorAddress in neigboors)
            {
                WhisperContractCreation(contractAddress, neigboorAddress);
            }
            var model = new WhisperContractCreationViewModel()
            {
                Neigboors = neigboors
            };
            return View(model);
        }

        public async Task<ActionResult> ContractDetails(string transactionHash, string fromAddress)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            var reciept = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            var contractAddress = "Contract Creation Transaction not mined yet.";
            if (reciept != null)
            {
                contractAddress = reciept.ContractAddress;
                //var contract = web3.Eth.GetContract(_abi, contractAddress);
                //reciept.
            }

            var model = new ContractDetailsModel()
            {
                TransactionHash = transactionHash,
                ContractAddress = contractAddress,
                ContractBalance = "0",
                ContractPrice = "10",
                DestinationAddress = "0xblahblah",
                State = "Created",
                FromAddress = fromAddress
            };
            return View(model);
        }

        public async Task<ActionResult> GetState(string contractAddress)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            var contract = web3.Eth.GetContract(_abi, contractAddress);
            var abortFunction = contract.GetFunction("getState");
            var result = await abortFunction.CallAsync<int>();

            var model = new GetStateViewModel()
            {
                ContractState = MapToContractStateEnum(result)
            };
            return View(model);
        }


        public async Task<ActionResult> Abort(string senderAddress, string contractAddress)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            var contract = web3.Eth.GetContract(_abi, contractAddress);
            var abortFunction = contract.GetFunction("abort");
            var result = await abortFunction.CallAsync<int>();

            // Unlock the caller's account with the given password

            //var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(model.BuyerPublicKey, model.BuyerAccountPassword, accountUnlockTime);

            var transactionHash = await abortFunction.SendTransactionAsync(senderAddress);
            //var await web3.Eth.Transactions.SendTransaction.SendRequestAsync(new Nethereum.RPC.Eth.DTOs.TransactionInput() {   } )

            var model = new AbortViewModel()
            {
                ContractState = MapToContractStateEnum(result)
            };
            return View(model);
        }

        public async Task<ActionResult> DestinationAddressRouteFound(string contractAddress)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            var contract = web3.Eth.GetContract(_abi, contractAddress);
            var abortFunction = contract.GetFunction("destinationAddressRouteFound");
            var result = await abortFunction.CallAsync<int>();
            var model = new DestinationAddressRouteFoundViewModel()
            {
                ContractState = MapToContractStateEnum(result)
            };
            return View(model);
        }


        public async Task<ActionResult> DestinationAddressRouteConfirmed(string contractAddress)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            var contract = web3.Eth.GetContract(_abi, contractAddress);
            var abortFunction = contract.GetFunction("destinationAddressRouteConfirmed");
            var result = await abortFunction.CallAsync<int>();
            var model = new DestinationAddressRouteConfirmedViewModel()
            {
                ContractState = MapToContractStateEnum(result)
            };
            return View(model);
        }


        private void WhisperContractCreation(string contractAddress, string neigboorAddress)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            //web3.Client.w
        }

        private string MapToContractStateEnum(int result)
        {
            return Enum.GetName(typeof(ContractState), result);
        }

        enum ContractState
        {
            Created,
            Expired,
            Completed,
            Aborted,
            RouteFound
        }
    }
}
