// For more information see https://aka.ms/fsharp-console-apps

open System
open Microsoft.FSharp.Control
open Rmm.Crm.Domain
open Rmm.Crm.dbo
open Rmm.Data

printfn "Start of execution"

open Rmm.SqlServer

let dv80 =
    DB
        "Server=KBNDVCRM80\\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"

let test =
    DB
        "Server=KBNTSSQL38\\TEST02; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"

let env = dv80

let userName = @"DEMANT\sa_crm_rmm_ch"

let getUser =
    SystemUser.whereDomainName env userName

let user = getUser.Result
printfn $"Pre-User: %s{SystemUser.showSystemUser user}"

let newChUser = Option.map (SystemUser.copyWith "CH Retail Integration-User" "sa_crm_rmm_ch1@demant.com")
let updUser = newChUser user
printfn $"Post-User: %s{SystemUser.showSystemUser updUser}"
let activitiesCount =
    Activities.countWhereUser env (SystemUser.getId user)

printfn $"Activities: {activitiesCount.Result}"

let showActivity (apy: ActivityPartyBase) = printfn $"apy: {Activities.show apy}"

let printAll (acts: ActivityPartyBase array) =
    acts |> Array.iter showActivity
    printfn $"{acts.Length}----------------------------"

// let updatedActivities = Activities.updateUserInfo env user
// printfn $"Updated: {updatedActivities}"

let cNo = "C10488788"
let contactNo = ContactNumber cNo

let endResult =
    Contacts.getByNo env contactNo

let contactId =
    task {
        let! opt = Contacts.getIdByNo env (Some cNo)

        let guid =
            match opt with
            | Some g -> g
            | None -> Guid.Empty

        return guid
    }

let contact = endResult.Result
let cId = contactId.Result

let result =
    match contact with
    | Some ct -> Contact.pretty ct
    | None -> "Not found!"

printfn "Contact: %s" result
printfn $"Id: {cId.ToString()}"

printfn $"End of Program: "
