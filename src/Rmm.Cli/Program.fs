open System
open System.IO
open System.Threading
open System.Threading.Tasks
open System.CommandLine.Invocation
open System.CommandLine.Help
open FSharp.SystemCommandLine
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Rmm
open Serilog
open DataExport

let buildHost (argv: string[]) =
    Host.CreateDefaultBuilder(argv)
        .ConfigureHostConfiguration(fun configHost ->
            configHost.SetBasePath(Directory.GetCurrentDirectory()) |> ignore
            configHost.AddJsonFile("appsettings.json", optional = false) |> ignore
        )
        .UseSerilog(fun hostingContext configureLogger -> 
            configureLogger
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path = "logs/log.txt", 
                    rollingInterval = RollingInterval.Year
                )
                |> ignore
        )
        .Build()        


[<EntryPoint>]
let main argv =
    let host = buildHost argv
    let logger = host.Services.GetService<ILogger>()
    let cfg = host.Services.GetService<IConfiguration>()

    let showHelp (ctx: InvocationContext) =
        let hc = HelpContext(
            ctx.HelpBuilder,
            ctx.Parser.Configuration.RootCommand,
            Console.Out
        )
        task {
            ctx.HelpBuilder.Write(hc)
        }
        

    let ctx = Input.Context()
    rootCommand argv {
        description "RMM Utility"
        inputs ctx
        setHandler showHelp
        addCommand (dataExportCmd logger cfg)
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously