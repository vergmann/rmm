// For more information see https://aka.ms/fsharp-console-apps

open System
open Microsoft.FSharp.Control
open Rmm.Management.Data
open Rmm.Management.RmmMsCrm
open Rmm.Management.SqlServer

printfn "Start of execution"


let dv80 =
    DB
        @"Server=KBNDVCRM80\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
let dv80cfg =
    DB
        @"Server=KBNDVCRM80\DEV01; Database=MSCRM_CONFIG; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
let devTest =
    DB
        @"Server=KBNDVSQL110\DEV01; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"
let test =
    DB
        @"Server=KBNTSSQL38\TEST02; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=30; ConnectRetryCount=3; Command Timeout=60; TrustServerCertificate=True"

let cmdln = Environment.GetCommandLineArgs()

printfn $"Commandline: %A{cmdln}"
let lower (str: string) = str.ToLowerInvariant()
let upper (str: string) = str.ToUpperInvariant()

let createDomainName cc = @$"DEMANT\sa_crm_rmm_{lower cc}"

let createUserInfo (countryCode2: string) =
    let lower = countryCode2.ToLowerInvariant()
    let upper = countryCode2.ToUpperInvariant()
    ( $"{upper} Retail",
      "Account",
      $"sa_crm_rmm_{lower}@demant.com" )

let updateUser (firstName, lastName, email) =
    Option.map (
        SystemUser.copyWith (Some (firstName)) None (Some (lastName)) (Some (email))
    )

let env = test
printfn $"{env.ConnectionString}"
let countryCode =
    if cmdln.Length > 1 && cmdln[1].Length = 2 then cmdln[1]
    else failwith "Missing country code (2 letters)"
    
printfn $"{countryCode}"
printfn "--------------------------------"
// exit 99
let preDomainName = createDomainName countryCode
let preUser =
    SystemUser.whereDomainName env preDomainName
    |> Async.RunSynchronously

printfn $"Pre-User: %s{SystemUser.showSystemUser preUser}"
let userInfo = createUserInfo countryCode
let updUser = updateUser userInfo preUser
printfn $"Post-User: %s{SystemUser.showSystemUser updUser}"

let activitiesCount =
    Activities.countWhereUser env (SystemUser.getId updUser)

printfn $"Activities: {activitiesCount.Result}"

let showActivity (apy: dbo.ActivityPartyBase) = printfn $"apy: {Activities.show apy}"

let printAll (acts: dbo.ActivityPartyBase array) =
    acts |> Array.iter showActivity
    printfn $"{acts.Length}----------------------------"


let updateActivities env =
     Activities.updateUserInfo env updUser
     |> Async.RunSynchronously
let result = updateActivities env
printfn $"Result: %A{fst result}"
printfn $"Chunks: %i{(snd result).Length}" 

// let getSslDisabled env =
//     Config.selectSslCheckForEncryption env
//     |> Async.RunSynchronously
//     |> Option.flatten
//     |> Option.map (fun b -> if b then "SSL Check Disabled" else "SSL Check Enabled")
//     |> Option.defaultValue "Setting not found!!"
//
// printfn $"{getSslDisabled dv80cfg}"

// let disableSslCheck =
//     Config.updateSslCheckForEncryption dv80cfg (Some true)
//     |> Async.RunSynchronously
//     |> printfn "Rows updated: %i, to disable SSL check"
//     
// printfn "Post update check:"
//
// printfn $"{getSslDisabled dv80cfg}"


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
