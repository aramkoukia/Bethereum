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
        private static string _abi = @"[{""constant"":false,""inputs"":[],""name"":""destinationAddressRouteFound"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""getState"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""abort"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""destinationAddressRouteConfirmed"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""constant"":true,""inputs"":[],""name"":""state"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},{""inputs"":[{""name"":""_finalDestination"",""type"":""address""},{""name"":""_contractGracePeriod"",""type"":""uint256""},{""name"":""_contractPrice"",""type"":""uint256""}],""type"":""constructor""},{""anonymous"":false,""inputs"":[],""name"":""aborted"",""type"":""event""},{""anonymous"":false,""inputs"":[],""name"":""routeFound"",""type"":""event""},{""anonymous"":false,""inputs"":[],""name"":""routeAccepted"",""type"":""event""}]";
        private static string _byteCode = "606060405260405160608061027e83395060c06040525160805160a05160008054600160a060020a03199081166c010000000000000000000000003381028190049190911783554260035560028054909216958102049490941790935560049190915560055561020a90819061007490396000f3606060405260e060020a6000350463047854d9811461004a5780631865c57d1461006757806335a063b4146100855780636c88c36c146100a2578063c19d93fb146100c1575b610002565b3461000257610073600654600090819060ff16156100d257610002565b346100025760065460ff165b60408051918252519081900360200190f35b3461000257610073600654600090819060ff161561014457610002565b346100025761007360065460009060049060ff16811461018657610002565b346100025761007360065460ff1681565b6001805473ffffffffffffffffffffffffffffffffffffffff19166c01000000000000000000000000338102041790556040517f78d20fa24b6a0a3596e34219ca2fd4ce740f5d3cce342d6b1d76bd879491bf7290600090a16006805460ff19166004179081905560ff1691505b5090565b6040517f80b62b7017bb13cf105e22749ee2a06a417ffba8c7f57b665057e0f3c2e925d990600090a16006805460ff19166003179081905560ff169150610140565b6040517f17e8425f2f0f52156cb58fae3262b87ffe900164617ac332659a4b0e2d8434f590600090a16040516006805460ff191660021790556000805460055473ffffffffffffffffffffffffffffffffffffffff9091169281156108fc0292818181858888f1935050505015156101fd57610002565b60065460ff16915061014056";
        private static string _getAddress = "./geth.ipc";
        private static Nethereum.Hex.HexTypes.HexBigInteger _accountUnlockTime = new Nethereum.Hex.HexTypes.HexBigInteger(120);

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


            // Unlock the caller's account with the given password
            var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(model.BuyerPublicKey, model.BuyerAccountPassword, _accountUnlockTime);

            var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(_abi, _byteCode, model.BuyerPublicKey, gas, model.ContractPrice * 10, model.DestinationAddress, model.ContractGracePeriod, model.ContractPrice);

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
                //ContractBalance = "0",
                //ContractPrice = "10",
                //DestinationAddress = "0xblahblah",
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
            var model = new AbortViewModel()
            {
                CallerAddress = senderAddress,
                ContractAddress = contractAddress,
                //ContractState = MapToContractStateEnum(result)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AbortContract(AbortViewModel model)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            var contract = web3.Eth.GetContract(_abi, model.ContractAddress);
            var abortFunction = contract.GetFunction("abort");
            //var result = await abortFunction.CallAsync<int>();

            // Unlock the caller's account with the given password
            var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(model.CallerAddress, model.CallerPassword, _accountUnlockTime);

            var transactionHash = await abortFunction.SendTransactionAsync(model.CallerAddress);
            model.TransactionHash = transactionHash;
            model.Message = "Abort Transaction Sent.";
            return View("Abort", model);
        }

        public async Task<ActionResult> DestinationAddressRouteFound(string senderAddress, string contractAddress)
        {
            var model = new DestinationAddressRouteFoundViewModel()
            {
                CallerAddress = senderAddress,
                ContractAddress = contractAddress,
                //ContractState = MapToContractStateEnum(result)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PostDestinationAddressRouteFound(DestinationAddressRouteFoundViewModel model)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            var contract = web3.Eth.GetContract(_abi, model.ContractAddress);
            var destinationAddressRouteFoundFunction = contract.GetFunction("destinationAddressRouteFound");

            // Unlock the caller's account with the given password
            var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(model.CallerAddress, model.CallerPassword, _accountUnlockTime);

            var transactionHash = await destinationAddressRouteFoundFunction.SendTransactionAsync(model.CallerAddress);
            model.TransactionHash = transactionHash;
            model.Message = "RouteFound Transaction Sent.";
            return View("DestinationAddressRouteFound", model);
        }

        public async Task<ActionResult> DestinationAddressRouteConfirmed(string senderAddress, string contractAddress)
        {
            var model = new DestinationAddressRouteConfirmedViewModel()
            {
                CallerAddress = senderAddress,
                ContractAddress = contractAddress,
                //ContractState = MapToContractStateEnum(result)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PostDestinationAddressRouteConfirmed(DestinationAddressRouteConfirmedViewModel model)
        {
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var web3 = new Web3(ipcClient);
            var contract = web3.Eth.GetContract(_abi, model.ContractAddress);
            var destinationAddressRouteFoundFunction = contract.GetFunction("destinationAddressRouteConfirmed");

            // Unlock the caller's account with the given password
            var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(model.CallerAddress, model.CallerPassword, _accountUnlockTime);

            var transactionHash = await destinationAddressRouteFoundFunction.SendTransactionAsync(model.CallerAddress);
            model.TransactionHash = transactionHash;
            model.Message = "Route Confrimation Transaction Sent.";
            return View("DestinationAddressRouteConfirmed", model);

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
