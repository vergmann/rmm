namespace Rmm.Management

open System
open System.Diagnostics
open Microsoft.FSharp.Core
open SqlHydra.Query
open Rmm.Management
open Rmm.Management.Domain
open Rmm.Management.SqlServer

module Data =
    module Common =
        type OperationResult =
            { Selected: int
              Inserted: int
              Updated: int
              Deleted: int
              Chunks: int
              ExecutionTime: TimeSpan
              Elapsed: TimeSpan }
            static member (+)(x: OperationResult, y: OperationResult) =
                { Selected = x.Selected + y.Selected
                  Inserted = x.Inserted + y.Inserted
                  Updated = x.Updated + y.Updated
                  Deleted = x.Deleted + y.Deleted
                  Chunks = x.Chunks + y.Chunks
                  ExecutionTime = x.ExecutionTime + y.ExecutionTime
                  Elapsed = x.Elapsed + y.Elapsed }

            static member Zero =
                { Selected = 0
                  Inserted = 0
                  Updated = 0
                  Deleted = 0
                  Chunks = 0
                  ExecutionTime = TimeSpan.Zero
                  Elapsed = TimeSpan.Zero }

        module OperationResult =
            let total results : OperationResult =
                let sum = results |> Array.sum

                { sum with
                    ExecutionTime = sum.Elapsed
                    Elapsed = TimeSpan.Zero }

        let fallback value = Option.defaultValue value
        let idStr userId = userId.ToString()
        let emailStr = fallback "NO EmailAddress"
        let fnStr = fallback "NO Full Name"
        let dnStr = fallback "NO Domain Name"
        let cNoStr = fallback "NO Contact Number"
        let posInt = fallback -1
        let updateAsync' (db: DB) = updateAsync (Create db.OpenContext)

        let createFullName firstName middleName lastName =
            match firstName, middleName, lastName with
            | Some f, Some m, Some l -> $"{f} {m} {l}" |> Some
            | Some f, None, Some l -> $"%s{f} %s{l}" |> Some
            | Some f, Some m, None -> $"%s{f} %s{m}" |> Some
            | Some f, None, None -> $"%s{f}" |> Some
            | None, Some m, Some l -> $"%s{m} %s{l}" |> Some
            | None, None, Some l -> $"%s{l}" |> Some
            | None, Some m, None -> $"%s{m}" |> Some
            | None, None, None -> None

        module MsCrm =
            open Rmm.Management.RmmMsCrm

            let selectTask' ct = selectTask HydraReader.Read ct
            let selectTaskNew (db: DB) = selectTask' (Create db.OpenContext)
            let selectAsync' ct = selectAsync HydraReader.Read ct
            let selectAsyncNew (db: DB) = selectAsync' (Create db.OpenContext)
            let updateTask' (db: DB) = updateTask (Create db.OpenContext)

        module MsCrmConfig =
            open Rmm.Management.MsCrmConfig

            let selectTask' ct = selectTask HydraReader.Read ct
            let selectTaskNew (db: DB) = selectTask' (Create db.OpenContext)
            let selectAsync' ct = selectAsync HydraReader.Read ct
            let selectAsyncNew (db: DB) = selectAsync' (Create db.OpenContext)
            let updateTask' (db: DB) = updateTask (Create db.OpenContext)

    [<RequireQualifiedAccess>]
    module SystemUser =
        open Common
        open Common.MsCrm
        open Rmm.Management.RmmMsCrm

        let userTable =
            table<dbo.SystemUserBase> |> inSchema (nameof dbo)

        let otc = 8

        let copyWith firstName middleName lastName email user : dbo.SystemUserBase =
            { user with
                InternalEMailAddress = email
                FirstName = firstName
                MiddleName = middleName
                LastName = lastName
                FullName = createFullName firstName middleName lastName }

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

        let getUserByDomainName (db: DB) name =
            selectAsync' (Create db.OpenContext) {
                for u in userTable do
                    where (u.DomainName = name)
                    mapSeq (toUser u)
                    tryHead
            }

        let whereDomainName db name =
            selectAsyncNew db {
                for u in userTable do
                    where (u.DomainName = name)
                    tryHead
            }

        let getId (systemUser: dbo.SystemUserBase option) =
            match systemUser with
            | Some su -> Some su.SystemUserId
            | None -> None

        let update db (systemUser: dbo.SystemUserBase) =
            updateAsync' db {
                for u in userTable do
                    entity systemUser
                    excludeColumn u.SystemUserId
                    where (u.SystemUserId = systemUser.SystemUserId)
            }

    [<RequireQualifiedAccess>]
    module Logs =
        open Common.MsCrm
        open Rmm.Management.RmmMsCrm

        let systemLogEntryTable =
            table<dbo.dgs_systemlogentryBase>
            |> inSchema (nameof dbo)

        let count db =
            selectAsyncNew db {
                for log in systemLogEntryTable do
                    count
            }

        let latest db =
            selectAsyncNew db {
                for log in systemLogEntryTable do
                    take 5
                    toArray
            }

    [<RequireQualifiedAccess>]
    module Contacts =
        open Common.MsCrm
        open Rmm.Management.RmmMsCrm

        let contactBaseTable =
            table<dbo.ContactBase> |> inSchema (nameof dbo)

        let toContact (ct: dbo.ContactBase) : Domain.Contact =
            { Id = ContactId ct.ContactId
              Number =
                match ct.dgs_ContactNumber with
                | Some cn -> ContactNumber cn
                | None -> ContactNumber "ERROR"
              FullName =
                match ct.FullName with
                | Some fnStr -> fnStr

                | None -> "ERROR" }

        let getById db contactId =
            let (ContactId id) = contactId

            selectAsyncNew db {
                for ct in contactBaseTable do
                    where (ct.ContactId = id)
                    mapSeq (toContact ct)
                    tryHead
            }

        let getByNo db contactNumber =
            let (ContactNumber cno) = contactNumber

            selectAsyncNew db {
                for ct in contactBaseTable do
                    where (ct.dgs_ContactNumber = Some cno)
                    // select (ct.ContactId, ct.dgs_ContactNumber, ct.FullName) into selected
                    mapSeq (toContact ct)
                    tryHead
            }

        let getIdByNo db cNo =
            selectAsyncNew db {
                for ct in contactBaseTable do
                    where (ct.dgs_ContactNumber = cNo)
                    select ct.ContactId
                    tryHead
            }

    [<RequireQualifiedAccess>]
    module Activities =
        open Common
        open Common.MsCrm
        open Rmm.Management.RmmMsCrm

        type UserInfoUpdate =
            { PartyIdName: string option
              AddressUsed: string option
              AddressUsedEmailColumnNumber: int option
              RowIds: Guid array }

        module UserInfoUpdate =
            let create (user: dbo.SystemUserBase option) (rows: Guid array) =
                user
                |> Option.map (fun u ->
                    { PartyIdName = u.FullName
                      AddressUsed = u.InternalEMailAddress
                      AddressUsedEmailColumnNumber = 15 |> Some
                      RowIds = rows })

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

        let idsWhereUser db userId =
            let otc = SystemUser.otc

            selectAsyncNew db {
                for apy in activityPartyTable do
                    where (
                        apy.PartyId = userId
                        && apy.PartyObjectTypeCode = otc
                    )

                    select apy.ActivityPartyId
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

        let private updateUserInfoAsync (db: DB) (party: UserInfoUpdate) =
            async {
                let sw = Stopwatch.StartNew()
                use ctx = db.OpenContext()
                do ctx.BeginTransaction()
                let update' = updateAsync (Shared ctx)

                let! updated =
                    update' {
                        for apy in activityPartyTable do
                            set apy.AddressUsed party.AddressUsed
                            set apy.AddressUsedEmailColumnNumber party.AddressUsedEmailColumnNumber
                            set apy.PartyIdName party.PartyIdName
                            where (apy.ActivityPartyId |=| party.RowIds)
                    }

                // let sql = updated.ToKataQuery() |> DB.toSql
                // printfn $"%s{sql}"

                do ctx.CommitTransaction()
                do sw.Stop()

                let result =
                    { OperationResult.Zero with
                        Updated = updated
                        Elapsed = sw.Elapsed }

                return result
            }

        /// New info must be in 'systemUser'
        /// Max chunksSize = 2100 (server limit)
        let updateUserInfoBatch (chunkSize: int) (db: DB) (systemUser: dbo.SystemUserBase option) =
            async {
                let sw = Stopwatch.StartNew()

                let createUpdateRec =
                    UserInfoUpdate.create systemUser

                let doUpdate = updateUserInfoAsync db
                
                let maxDegreeOfParallelism max seq = seq, max

                let chunkResults =
                    idsWhereUser db (SystemUser.getId systemUser)
                    |> Async.RunSynchronously
                    |> Seq.cache
                    |> Seq.chunkBySize chunkSize
                    |> Seq.map createUpdateRec
                    |> Seq.choose id
                    |> Seq.map doUpdate
                    |> maxDegreeOfParallelism 8
                    |> Async.Parallel
                    |> Async.RunSynchronously

                do sw.Stop()

                let executionResult =
                    let totals =
                        OperationResult.total chunkResults

                    { totals with Elapsed = sw.Elapsed }

                let result = (executionResult, chunkResults)

                return result
            }

        let updateUserInfo db user = updateUserInfoBatch 1000 db user

    module Config =
        open Common
        open Common.MsCrmConfig
        open Rmm.Management.MsCrmConfig

        let deploymentPropertiesTable =
            table<dbo.DeploymentProperties>
            |> inSchema (nameof dbo)

        type DeploymentProperties = DisableSSLCheckForEncryption of bool option

        let selectSslCheckForEncryption (db: DB) =
            let columnName =
                nameof DisableSSLCheckForEncryption

            selectAsyncNew db {
                for prop in deploymentPropertiesTable do
                    where (prop.ColumnName = columnName)
                    select prop.BitColumn
                    tryHead
            }

        let updateSslCheckForEncryption db (disable: bool option) =
            let columnName =
                nameof DisableSSLCheckForEncryption

            updateAsync' db {
                for p in deploymentPropertiesTable do
                    set p.BitColumn disable
                    where (p.ColumnName = columnName)
            }
