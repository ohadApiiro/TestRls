using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using LspTypes;
using Newtonsoft.Json;
using StreamJsonRpc;

namespace ConsoleApp;

class Program
{
    
    static async Task Main(string[] args)
    {
        //Test();
        var fileName = "/home/ohad/src/rust/hello_cargo/src/main.rs";
        var process = StartProcess("/home/ohad/src/rust/rust-analyzer/target/debug/rust-analyzer");

        using (var rpc = JsonRpc.Attach(process.StandardInput.BaseStream, process.StandardOutput.BaseStream))
        {
            await DoInitAsync("/home/ohad/src/rust/hello_cargo", rpc);
            OpenFile(fileName, rpc);

            await PrintSymbol(fileName, new Position { Line = 26, Character = 17 }, rpc);
            
        }
        Console.WriteLine("The end");
    }

    private static void Test()
    {
        
        var json = File.ReadAllText("/home/ohad/src/playgorund/ConsoleApp/res.json");
        var res = JsonConvert.DeserializeObject<InitializeResult>(json);
        Console.WriteLine();
    }
    
    private static async Task PrintSymbol(string filePath, Position position, JsonRpc rpc)
    {
        var className = await GetHoverAsync(filePath, position, rpc);

        Console.WriteLine("-------------");
        await GetHoverAsync(filePath, position, rpc);
        
        var msg = className ?? "hover api returned null";
        Console.WriteLine($"{msg}");

        GotoDefinitionParams gotoDefinitionParams = ParamsFactory.GetGotoDefinitionParams(filePath, position);
        var locations = await Invoker.InvokeGotoDefinitionAsync(gotoDefinitionParams, rpc);

        var location = locations.FirstOrDefault();
        if (location == null)
        {
            Console.WriteLine($"goto definition has no results for {filePath}:{position}");
            return;
        }
        
        Console.WriteLine($"file name {location.Uri} {className}");
    }

    private static async Task<string> GetHoverAsync(string filePath, Position position, JsonRpc rpc)
    {
        var hoverParams = ParamsFactory.GetHoverParams(filePath, position);
        Thread.Sleep(TimeSpan.FromMinutes(1));
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var hover = await Invoker.InvokeHoverAsync(hoverParams, rpc);
        stopwatch.Stop();
        Console.WriteLine($"hover time {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes");
        if (hover == null)
        {
            Console.WriteLine("Hover is null");
            return null;
        }

        
        var className = (hover.Contents.Value as MarkupContent)?.Value.Split(Environment.NewLine)[0] ?? string.Empty;
        if (className.Contains("in progress"))
        {
            Console.WriteLine("in progress");
            return null;
        }

        return className;
    }
    
    private static void OpenFile(string filePath, JsonRpc rpc)
    {
        var openParams = ParamsFactory.GetDidOpenParams(filePath);
        rpc.InvokeWithParameterObjectAsync("textDocument/didOpen", openParams);
    }
    
    private static async Task CloseAsync(int pid, JsonRpc rpc)
    {
        Console.WriteLine($"{pid}: End of operation reached");
        await rpc.InvokeWithParameterObjectAsync("shutdown", new { });
    }
        
    private static async Task DoInitAsync(string rootPath, JsonRpc rpc)
    {
        var initParams = ParamsFactory.GetInitObject(rootPath);
        var res = await Invoker.InvokeInitAsync(initParams, rpc);
        Thread.Sleep(100);
        await Invoker.InvokeInitializedAsync(rpc);
    }
    public static Process StartProcess(string exePath)
    {
        Console.WriteLine($"starting {exePath}");
        var psi = new ProcessStartInfo(exePath);
           
        var proc = new Process();
        proc.StartInfo = psi;
        proc.StartInfo.RedirectStandardInput = true;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.Start();
        return proc;
    }
    
    public class ServerCapabilities2
    {   
        
    }
}