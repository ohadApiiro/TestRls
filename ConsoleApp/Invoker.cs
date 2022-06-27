using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LspTypes;
using Polly;
using Polly.Retry;
using StreamJsonRpc;

namespace ConsoleApp
{
    public class InitResult
    {
        //public ServerCapabilities capabilities { get; set; }
        //public Capabilities capabilities { get; set; }
        public _InitializeResults_ServerInfo serverInfo { get; set; }
    }

    public class GotoDefinitionResponse
    {
        public List<Location> Locations { get; set; }
    }
    
    public class Invoker
    {
        private static readonly int NumberOfRetries = 10;
        private static readonly AsyncRetryPolicy RetryPolicy = Policy.Handle<TaskCanceledException>()
            .WaitAndRetryAsync(NumberOfRetries, _ => TimeSpan.FromMilliseconds(3));

        public static async Task<InitializeResult> InvokeInitAsync(InitializeParamsExt initParams, JsonRpc rpc)
        {
            return await InvokeWithParameterObjectAsync<InitializeResult>("initialize", rpc, initParams);
        }

        public static async Task<InitResult> InvokeInitAsync2(InitializeParams initParams, JsonRpc rpc)
        {
            return await InvokeWithParameterObjectAsync<InitResult>("initialize", rpc, initParams);
        }

        public static async Task InvokeInitializedAsync(JsonRpc rpc)
        {
            await rpc.NotifyAsync("initialized");
        }
        
        public static async Task<Hover> InvokeHoverAsync(HoverParams hoverParams ,JsonRpc rpc)
        {
            return  await InvokeWithParameterObjectAsync<Hover>("textDocument/hover", rpc, hoverParams);
        }
        
        public static async Task<Location[]> InvokeGotoDefinitionAsync(GotoDefinitionParams gotoDefinitionParams, JsonRpc rpc)
        {
            return  await InvokeWithParameterObjectAsync<Location[]>("textDocument/definition", rpc, gotoDefinitionParams); 
        }
        
        private static async Task InvokeWithParameterObjectAsync(string targetName, JsonRpc rpc, object argument = null)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                await rpc.InvokeWithParameterObjectAsync(targetName, argument ?? new { });
            });
        }

        private static async Task<TResult> InvokeWithParameterObjectAsync<TResult>(string targetName, JsonRpc rpc, object argument = null)
        {
            TResult result = default;
            await RetryPolicy.ExecuteAsync(async () =>
            {
                result  = await rpc.InvokeWithParameterObjectAsync<TResult>(targetName, argument ?? new {} );
            });
        
            return result;
        }
    }
}