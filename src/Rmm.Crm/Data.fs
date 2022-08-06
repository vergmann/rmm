namespace Rmm

open SqlHydra.Query
open Rmm.Crm

module Data =

    let openContext () =
        let compiler = SqlKata.Compilers.SqlServerCompiler()
        let conn = DB.openConnection()
        new QueryContext(conn, compiler)

    module Logs =
        let systemLogEntryTable = table<dbo.dgs_systemlogentryBase> |> inSchema (nameof dbo)

        let count =
            selectAsync HydraReader.Read (Create openContext) {
                for log in systemLogEntryTable do
                count
            }

        let latest =
            selectAsync HydraReader.Read (Create openContext) {
                for log in systemLogEntryTable do
                take 5
                toArray
            }
