using Nethereum.RPC.Shh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whisper
{
    class Program
    {
        private static string _getAddress = "./geth.ipc";

        static void Main(string[] args)
        {
            var resultIdentity = string.Empty;
            var ipcClient = new Nethereum.JsonRpc.IpcClient.IpcClient(_getAddress);
            var shh = new ShhNewIdentity(ipcClient);

            Task.Run(async () =>
            {
                resultIdentity = await shh.SendRequestAsync();
            }).GetAwaiter().GetResult();

            Console.WriteLine("Identity:");
            Console.WriteLine(resultIdentity);
            var topics = new[] { "0x68656c6c6f20776f726c64" };
            var loop = true;
            while (true)
            {
                
                var command = Console.ReadLine();
                switch (command)
                {
                    case "post":
                        Task.Run(async () =>
                        {
                            var parameters = new[] {
                                new { from = resultIdentity,
                                      to = "0x0415ea5c7448f7eacd93f01352c8d26222ba0a4d36dfabcb4d38e9f02e4391381f16b8e0d45cd8246c84e308d8699f5e1b26faa9be99a092b94e1e274367f9b8bc",
                                      topics= topics,
                                      payload= "0x68656c6c6f20776f726c64",
                                      ttl = "0x64",
                                      priority = "0x64"},
                               };

                            var rpcRequest = new EdjCase.JsonRpc.Core.RpcRequest(73, "shh_post", parameters);
                            var resultPost = await shh.Client.SendRequestAsync(rpcRequest);
                            Console.WriteLine("shh_post has errors:");
                            Console.WriteLine(resultPost.HasError.ToString());
                        }).GetAwaiter().GetResult();

                        break;

                    case "get":
                        Task.Run(async () =>
                        {
                            var parameters = new[] { "0x7" };

                            var rpcRequest = new EdjCase.JsonRpc.Core.RpcRequest(73, "shh_getMessages", parameters);
                            var resultPost = await shh.Client.SendRequestAsync(rpcRequest);
                            Console.WriteLine("shh_getMessages has errors:");
                            Console.WriteLine(resultPost.HasError.ToString());
                            Console.WriteLine($"Count: {resultPost.Result?.Count()}");
                            
                        }).GetAwaiter().GetResult();
                        break;

                    case "filter":
                        Task.Run(async () =>
                        {
                            var parameters = new[] {
                                new { to = resultIdentity,
                                      topics= topics,
                               }
                            };

                            var rpcRequest = new EdjCase.JsonRpc.Core.RpcRequest(73, "shh_newFilter", parameters);
                            var resultPost = await shh.Client.SendRequestAsync(rpcRequest);
                            Console.WriteLine("shh_newFilter has errors:");
                            Console.WriteLine(resultPost.HasError.ToString());
                            Console.WriteLine($"Count: {resultPost.Result?.Count()}");

                        }).GetAwaiter().GetResult();
                        break;

                    default:
                        loop = false;
                        break;
                }
            }

            Console.WriteLine("Enter to quit");
            Console.ReadLine();

        }
    }
}
