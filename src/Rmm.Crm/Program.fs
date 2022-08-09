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
        @"Server=KBNDVCRM80\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
let devTest =
    DB
        @"Server=KBNDVSQL110\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
let test =
    DB
        @"Server=KBNTSSQL38\TEST02; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"

let env = dv80

let userName = @"DEMANT\sa_crm_rmm_ch"

let getUser =
    SystemUser.whereDomainName env userName

let user = getUser.Result
printfn $"Pre-User: %s{SystemUser.showSystemUser user}"

let createUserInfo (countryCode2: string) =
    let lower = countryCode2.ToLowerInvariant()
    let upper = countryCode2.ToUpperInvariant()
    ( $"{upper} Retail",
      "Integration-Account",
      $"sa_crm_rmm_{lower}@demant.com" )

let updateUser (fname, lname, email) =
    Option.map (
        SystemUser.copyWith (Some (fname)) None (Some (lname)) (Some (email))
    )

let userInfo = createUserInfo "CH"
let updUser = updateUser userInfo user
printfn $"Post-User: %s{SystemUser.showSystemUser updUser}"

let activitiesCount =
    Activities.countWhereUser env (SystemUser.getId updUser)

printfn $"Activities: {activitiesCount.Result}"

let showActivity (apy: ActivityPartyBase) = printfn $"apy: {Activities.show apy}"

let printAll (acts: ActivityPartyBase array) =
    acts |> Array.iter showActivity
    printfn $"{acts.Length}----------------------------"

let updateActivities =
    Activities.updateUserInfo env updUser

printfn $"Result: %A{fst updateActivities}"

// let cNo = "C10488788"
// let contactNo = ContactNumber cNo
//
// let endResult =
//     Contacts.getByNo env contactNo

// let contactId =
//     task {
//         let! opt = Contacts.getIdByNo env (Some cNo)
//
//         let guid =
//             match opt with
//             | Some g -> g
//             | None -> Guid.Empty
//
//         return guid
//     }
//
// let contact = endResult.Result
// let cId = contactId.Result
//
// let result =
//     match contact with
//     | Some ct -> Contact.pretty ct
//     | None -> "Not found!"
//
// printfn "Contact: %s" result
// printfn $"Id: {cId.ToString()}"

printfn $"End of Program: "
