namespace Rmm

open SqlHydra.Query
open Rmm.Crm
open Rmm.Crm.Domain
open Rmm.SqlServer

module Data =

    let selectTask' ct = selectTask HydraReader.Read ct
    let selectTaskNew (db: DB) = selectTask' (Create db.OpenContext)
    module Logs =
        let systemLogEntryTable =
            table<dbo.dgs_systemlogentryBase>
            |> inSchema (nameof dbo)

        let count (db: DB) =
            selectAsync HydraReader.Read (Create db.OpenContext) {
                for log in systemLogEntryTable do
                    count
            }

        let latest (db: DB) =
            selectAsync HydraReader.Read (Create db.OpenContext) {
                for log in systemLogEntryTable do
                    take 5
                    toArray
            }

    module Contacts =
        let contactBaseTable =
            table<dbo.ContactBase> |> inSchema (nameof dbo)

        let toContact (ct: dbo.ContactBase) : Contact =
            { Id = ContactId ct.ContactId
              Number =
                match ct.dgs_ContactNumber with
                | Some cn -> ContactNumber cn
                | None -> ContactNumber "ERROR"
              FullName =
                match ct.FullName with
                | Some fn -> fn
                | None -> "ERROR" }

        let getContactById (db: DB) contactId =
            let (ContactId id) = contactId

            selectTask' (Create db.OpenContext) {
                for ct in contactBaseTable do
                    where (ct.ContactId = id)
                    tryHead
            }

        let getContactByNo (db: DB) contactNumber =
            let (ContactNumber cno) = contactNumber

            selectTask' (Create db.OpenContext) {
                for ct in contactBaseTable do
                    where (ct.dgs_ContactNumber = Some cno)
                    // select (ct.ContactId, ct.dgs_ContactNumber, ct.FullName) into selected
                    mapSeq ( toContact ct  )
                    tryHead
            }
            
        let getIdByNo (db: DB) cNo =
            selectTaskNew db {
                for ct in contactBaseTable do
                where (ct.dgs_ContactNumber = cNo)
                select ct.ContactId
                tryHead
            }
