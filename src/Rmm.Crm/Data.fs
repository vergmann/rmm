namespace Rmm

open SqlHydra.Query
open Rmm.Crm.Domain
open Rmm.Crm
// open Rmm.Crm.dbo
open Rmm.SqlServer

module Data =

    let selectTask' ct = selectTask HydraReader.Read ct
    let selectTaskNew (db: DB) = selectTask' (Create db.OpenContext)
    let selectAsync' ct = selectAsync HydraReader.Read ct
    let selectAsyncNew (db: DB) = selectAsync' (Create db.OpenContext)
    let fallback value = Option.defaultValue value
    let idStr userId = userId.ToString()
    let emailStr = fallback "NO EmailAddress"
    let fnStr = fallback "NO Full Name"
    let dnStr = fallback "NO Domain Name"
    let cNoStr = fallback "NO Contact Number"
    let posInt = fallback -1

    module SystemUser =
        let userTable = table<dbo.SystemUserBase> |> inSchema (nameof dbo)
        let otc = 8

        let showSystemUser (userOpt: dbo.SystemUserBase option) =

            let show =
                match userOpt with
                | Some u ->
                    $"%s{fnStr u.FullName} | %s{u.DomainName} | %s{emailStr u.InternalEMailAddress} | %s{idStr u.SystemUserId}"
                | None -> "NO System User"

            show

        let toUser (su: dbo.SystemUserBase) =
            { Id = UserId su.SystemUserId
              DomainName = DomainAccountName su.DomainName
              FullName = su.FullName
              PrimaryEmail = EmailAddress su.InternalEMailAddress }

        let getUserByDomainName db name =
            selectTaskNew db {
                for u in userTable do
                    where (u.DomainName = name)
                    mapSeq (toUser u)
                    tryHead
            }

        let whereDomainName db name =
            selectTaskNew db {
                for u in userTable do
                    where (u.DomainName = name)
                    tryHead
            }

        let getId (systemUser: dbo.SystemUserBase option) =
            match systemUser with
            | Some su -> Some su.SystemUserId
            | None -> None

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
        let contactBaseTable = table<dbo.ContactBase> |> inSchema (nameof dbo)

        let toContact (ct: dbo.ContactBase) : Contact =
            { Id = ContactId ct.ContactId
              Number =
                match ct.dgs_ContactNumber with
                | Some cn -> ContactNumber cn
                | None -> ContactNumber "ERROR"
              FullName =
                match ct.FullName with
                | Some fnStr -> fnStr

                | None -> "ERROR" }

        let getContactById (db: DB) contactId =
            let (ContactId id) = contactId

            selectTask' (Create db.OpenContext) {
                for ct in contactBaseTable do
                    where (ct.ContactId = id)
                    mapSeq (toContact ct)
                    tryHead
            }

        let getContactByNo (db: DB) contactNumber =
            let (ContactNumber cno) = contactNumber

            selectTask' (Create db.OpenContext) {
                for ct in contactBaseTable do
                    where (ct.dgs_ContactNumber = Some cno)
                    // select (ct.ContactId, ct.dgs_ContactNumber, ct.FullName) into selected
                    mapSeq (toContact ct)
                    tryHead
            }

        let getIdByNo (db: DB) cNo =
            selectTaskNew db {
                for ct in contactBaseTable do
                    where (ct.dgs_ContactNumber = cNo)
                    select ct.ContactId
                    tryHead
            }

    module Activities =
        let activityPartyTable =
            table<dbo.ActivityPartyBase>
            |> inSchema (nameof dbo)

        let show (party: dbo.ActivityPartyBase) =
            $"{idStr party.ActivityPartyId} | {emailStr party.AddressUsed} | {posInt party.AddressUsedEmailColumnNumber} | {fnStr party.PartyIdName}"

        let showOption (party: dbo.ActivityPartyBase option) =
            match party with
            | Some p -> show p
            | None -> "NONE"

        let whereUser db userId =
            let otc = SystemUser.otc

            selectAsyncNew db {
                for apy in activityPartyTable do
                    where (
                        apy.PartyId = userId
                        && apy.PartyObjectTypeCode = otc
                    )
            }

        let countWhereUser db userId =
            let otc = SystemUser.otc

            selectTaskNew db {
                for apy in activityPartyTable do
                    where (
                        apy.PartyId = userId
                        && apy.PartyObjectTypeCode = otc
                    )

                    count
            }
