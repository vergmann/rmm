namespace Rmm

open System
open System.IO
open FSharp.SystemCommandLine
open Microsoft.Extensions.Configuration
open Serilog

module DataExport =

    let dataExportCmd logger (cfg: IConfiguration) =
        let connStr =
            Input.Option<string>(
                aliases = [ "-c"; "--connection-string" ],
                defaultValue = cfg["ConnectionStrings:DB"],
                description = "Database connection string"
            )

        let outputDir =
            Input.Option<DirectoryInfo>(
                aliases = [ "-o"; "--output-directory" ],
                defaultValue = DirectoryInfo(cfg["DefaultOutputDirectory"]),
                description = "Output directory folder."
            )

        let startDate =
            Input.Option<DateTime>(
                name = "--start-date",
                defaultValue = DateTime.Today.AddDays(-7),
                description = "Start date (defaults to 1 week ago from today)"
            )

        let endDate =
            Input.Option<DateTime>(
                name = "--end-date",
                defaultValue = DateTime.Today,
                description = "End date (defaults to today)"
            )

        let exportHandler
            (logger: ILogger)
            (connStr: string, outputDir: DirectoryInfo, startDate: DateTime, endDate: DateTime)
            =
            task {
                logger.Information($"Querying {connStr} from {startDate} to {endDate}", startDate, endDate)
            // Do export stuff...
            }

        command "data-export" {
            description "Data Export"
            inputs (connStr, outputDir, startDate, endDate)
            setHandler (exportHandler logger)
        }
