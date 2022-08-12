// For more information see https://aka.ms/fsharp-console-apps

open System
open Microsoft.FSharp.Control
open Rmm.Management.Data
open Rmm.Management.SqlServer

printfn "Start of execution"

let cmdln = Environment.GetCommandLineArgs()
let args = cmdln[1..]

printfn $"Args: %A{args}"

let env = DB.testRmm
printfn $"{env.ConnectionString}\n"
// #########################
let toStr str = 
    match str with
        | Some wr -> $"{wr}"
        | None -> Common.base64Encode "NONE"

printfn "Orginal content:"
let cdiSettingsCurrent =
    WebResources.getCdiSettings env
    |> Async.RunSynchronously
    |> Option.flatten
    |> toStr
printfn $"{cdiSettingsCurrent}\n"

let currentDecoded = Common.base64Decode cdiSettingsCurrent
printfn "Decoded content"
printfn $"{currentDecoded}\n"

let outputXml = WebResources.updateAccountKey currentDecoded "agviDRxD2y0SHY2gXXhcjl" "atVCEw"
printfn $"%s{outputXml}"

printfn @$"Xml match?: %b{outputXml = currentDecoded}"

printfn "Re-encoded:"
let currentEncoded = Common.base64Encode outputXml
printfn $"{currentEncoded}"

printfn $"Match? {cdiSettingsCurrent = currentEncoded}"


/////////////////// ACTIVITIES //////////////////
// let lower (str: string) = str.ToLowerInvariant()
// let upper (str: string) = str.ToUpperInvariant()
//
// let createDomainName cc = @$"DEMANT\sa_crm_rmm_{lower cc}"
//
// let createUserInfo (countryCode2: string) =
//     let lower = countryCode2.ToLowerInvariant()
//     let upper = countryCode2.ToUpperInvariant()
//     ( $"{upper} Retail",
//       "Account",
//       $"sa_crm_rmm_{lower}@demant.com" )
//
// let updateUser (firstName, lastName, email) =
//     Option.map (
//         SystemUser.copyWith (Some firstName) None (Some lastName) (Some email)
//     )
//
// let countryCode =
//     if args.Length > 0 && args[0].Length = 2 then args[0]
//     else failwith "Missing country code (2 letters)"
//
// printfn $"{countryCode}"
// printfn "--------------------------------"
// // exit 99
// let preDomainName = createDomainName countryCode
// let preUser =
//     SystemUser.whereDomainName env preDomainName
//     |> Async.RunSynchronously
//
// printfn $"Pre-User: %s{SystemUser.showSystemUser preUser}"
// let userInfo = createUserInfo countryCode
// let updUser = updateUser userInfo preUser
// printfn $"Post-User: %s{SystemUser.showSystemUser updUser}"
//
// let activitiesCount =
//     Activities.countWhereUser env (SystemUser.getId updUser)
//
// printfn $"Activities: {activitiesCount.Result}"
// exit 88
//
// let updateActivities env =
//      Activities.updateUserInfo env updUser
//      |> Async.RunSynchronously
// let result = updateActivities env
// printfn $"Result: %A{fst result}"
// printfn $"Chunks: %i{(snd result).Length}" 

/////////////////// ACTIVITIES //////////////////

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
