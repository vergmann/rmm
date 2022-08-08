// For more information see https://aka.ms/fsharp-console-apps

open System
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

let getUser = SystemUser.whereDomainName env userName
let user = getUser.Result
printfn $"User: %s{SystemUser.showSystemUser user}"

let activitiesCount = Activities.countWhereUser env (SystemUser.getId user)

printfn $"Activities: {activitiesCount.Result}"

let printAct (apy: ActivityPartyBase) = printfn $"apy: {Activities.show apy}"

let printAll (acts: ActivityPartyBase array) =
    acts |> Array.iter printAct
    printfn $"{acts.Length}----------------------------"

let activities =
    Activities.whereUser env (SystemUser.getId user)
    |> Async.RunSynchronously
    |> Seq.cache
    |> Seq.chunkBySize 50
    |> Seq.iter printAll

// let endResult = async {
//     let! result = Data.Logs.latest env
//     result
//     |> Array.iter (fun l -> printfn $"%A{l}")
//     return 0
// }

let cNo = "C10488788"
let contactNo = ContactNumber cNo
let endResult = Contacts.getContactByNo env contactNo

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
