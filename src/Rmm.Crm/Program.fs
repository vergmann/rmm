// For more information see https://aka.ms/fsharp-console-apps

open System
open Rmm.Crm.Domain

printfn "Start of execution"

open Rmm
open Rmm.SqlServer

let test = DB "Server=KBNTSSQL38\\TEST02; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"

let env = test

// let endResult = async {
//     let! result = Data.Logs.latest env
//     result
//     |> Array.iter (fun l -> printfn $"%A{l}")
//     return 0
// }

let cNo = "C12676941"
let contactNo = ContactNumber cNo
let endResult = task {
    let! result = Data.Contacts.getContactByNo test contactNo
    return result
}

let contactId = task {
    let! opt = Data.Contacts.getIdByNo test (Some cNo)
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
printfn $"Contact: %s{result}"
printfn $"Id: {cId.ToString()}"

printfn $"End of Program: "
