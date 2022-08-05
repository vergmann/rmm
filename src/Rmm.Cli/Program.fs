open System
open System.IO
open FSharp.SystemCommandLine
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Serilog

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

let exportHandler (logger: ILogger) (connStr: string, outputDir: DirectoryInfo, startDate: DateTime, endDate: DateTime) =
    task {
        logger.Information($"Querying from {startDate} to {endDate}", startDate, endDate)            
        // Do export stuff...
    }

[<EntryPoint>]
let main argv =
    let host = buildHost argv
    let logger = host.Services.GetService<ILogger>()
    let cfg = host.Services.GetService<IConfiguration>()

    let connStr = Input.Option<string>(
        aliases = ["-c"; "--connection-string"], 
        defaultValue = cfg["ConnectionStrings:DB"],
        description = "Database connection string")

    let outputDir = Input.Option<DirectoryInfo>(
        aliases = ["-o";"--output-directory"], 
        defaultValue = DirectoryInfo(cfg["DefaultOutputDirectory"]), 
        description = "Output directory folder.")

    let startDate = Input.Option<DateTime>(
        name = "--start-date", 
        defaultValue = DateTime.Today.AddDays(-7), 
        description = "Start date (defaults to 1 week ago from today)")
        
    let endDate = Input.Option<DateTime>(
        name = "--end-date", 
        defaultValue = DateTime.Today, 
        description = "End date (defaults to today)")

    rootCommand argv {
        description "Data Export"
        inputs (connStr, outputDir, startDate, endDate)
        setHandler (exportHandler logger)
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously