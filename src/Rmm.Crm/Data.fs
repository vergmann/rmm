namespace Rmm

open SqlHydra.Query
open Rmm.Crm

module Data =

    module Logs =
        let systemLogEntryTable =
            table<dbo.dgs_systemlogentryBase>
            |> inSchema (nameof dbo)

        let count (db:DB) =
            selectAsync HydraReader.Read (Create db.GetContext) {
                for log in systemLogEntryTable do
                    count
            }

        let latest (db:DB) =
            selectAsync HydraReader.Read (Create db.GetContext) {
                for log in systemLogEntryTable do
                    take 5
                    toArray
            }
