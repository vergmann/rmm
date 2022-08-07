namespace Rmm

open SqlHydra.Query
open Rmm.Crm

module Data =
    let context db = DB.getContext db

    module Logs =
        let systemLogEntryTable =
            table<dbo.dgs_systemlogentryBase>
            |> inSchema (nameof dbo)

        let count db =
            let ctx = context db
            selectAsync HydraReader.Read (Shared ctx) {
                for log in systemLogEntryTable do
                    count
            }

        let latest db =
            let ctx = context db
            selectAsync HydraReader.Read (Shared ctx) {
                for log in systemLogEntryTable do
                    take 5
                    toArray
            }
